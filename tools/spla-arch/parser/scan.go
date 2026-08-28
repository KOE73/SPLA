package parser

import (
	"os"
	"path/filepath"
	"sort"
	"strings"
)

// Entity is one type declaration found in the source tree, with a
// repo-relative CodeRef so mapping rules stay stable regardless of where the
// tool is invoked from.
type Entity struct {
	Name    string
	Kind    string // class, interface, record, struct
	CodeRef string // repo-relative, forward slashes
	Bases   []string
}

// ScanResult is the discovered-entity list plus everything the caller needs
// to judge how trustworthy it is.
type ScanResult struct {
	Entities   []Entity
	Collisions map[string][]string // type name -> the extra files declaring it
	// MissingRoots are declared source roots that do not exist on disk.
	MissingRoots []string
}

// ScanSources walks each source root and returns every non-private declared
// type, sorted by name. repoRoot anchors CodeRef; roots are relative to it.
func ScanSources(repoRoot string, roots []string) (ScanResult, error) {
	var out []Entity
	var missingRoots []string
	seen := map[string]string{}
	collisions := map[string][]string{}

	for _, rel := range roots {
		abs := filepath.Join(repoRoot, filepath.FromSlash(rel))
		if st, err := os.Stat(abs); err != nil || !st.IsDir() {
			// A source root that no longer exists is a finding for the caller,
			// not a crash: it usually means the tree was reorganised and a
			// diagram still points at the old layout.
			missingRoots = append(missingRoots, rel)
			continue
		}
		err := filepath.Walk(abs, func(path string, info os.FileInfo, err error) error {
			if err != nil {
				return err
			}
			if info.IsDir() {
				switch info.Name() {
				case "bin", "obj", ".git", ".vs", "node_modules":
					return filepath.SkipDir
				}
				return nil
			}
			if !strings.HasSuffix(path, ".cs") {
				return nil
			}
			base := info.Name()
			if strings.Contains(base, "AssemblyInfo") ||
				strings.Contains(base, "GlobalUsings") ||
				strings.Contains(base, "AssemblyAttributes") ||
				strings.HasSuffix(base, ".g.cs") ||
				strings.HasSuffix(base, ".Designer.cs") {
				return nil
			}

			content, err := os.ReadFile(path)
			if err != nil {
				return nil
			}
			refRel, err := filepath.Rel(repoRoot, path)
			if err != nil {
				return nil
			}
			ref := filepath.ToSlash(refRel)

			for _, m := range typeDeclRegex.FindAllStringSubmatch(string(content), -1) {
				if m[mAccess] == "private" || m[mAccess] == "protected" {
					continue // implementation detail, not architecture
				}
				name := m[mName]
				if prev, dup := seen[name]; dup {
					// partial class split across files, or a genuine name
					// collision. One node per name keeps "one entity, one
					// place"; collisions across different files are reported.
					if prev != ref {
						collisions[name] = append(collisions[name], ref)
					}
					continue
				}
				seen[name] = ref

				var bases []string
				if m[mBases] != "" {
					for _, b := range strings.Split(m[mBases], ",") {
						c := strings.TrimSpace(strings.Split(b, "<")[0])
						if i := strings.LastIndex(c, "."); i >= 0 {
							c = c[i+1:]
						}
						if c != "" && c != "where" {
							bases = append(bases, c)
						}
					}
				}
				out = append(out, Entity{Name: name, Kind: m[mKind], CodeRef: ref, Bases: bases})
			}
			return nil
		})
		if err != nil {
			return ScanResult{}, err
		}
	}

	sort.Slice(out, func(i, j int) bool { return out[i].Name < out[j].Name })
	return ScanResult{Entities: out, Collisions: collisions, MissingRoots: missingRoots}, nil
}

// NodeType reports the kind of declaration as it was written: class,
// interface, record, struct, enum.
//
// It deliberately makes no visual decision. Colours and shapes live in the
// editor's style library, which looks a style up by this very value, so
// collapsing "record" and "class" into one visual bucket here would destroy
// the distinction before anything could ever draw it. The generator's job is
// to record what it knows for certain; what that means on screen is decided
// elsewhere and can be changed without regenerating.
func NodeType(e Entity) string {
	switch e.Kind {
	case "class", "interface", "record", "struct", "enum":
		return e.Kind
	default:
		// An unparsed or unknown declaration: fall back to the neutral type
		// so the node still gets a style rather than rendering unstyled.
		return "component"
	}
}
