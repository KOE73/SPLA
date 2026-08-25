// Package mapping holds the curated meaning layer: which code entity belongs
// to which logical block. Extraction (parser) says WHAT exists; mapping says
// WHAT IT MEANS. Only the mapping is hand-authored and versioned — diagram
// coordinates are derived from it and are never edited by hand.
package mapping

import (
	"encoding/json"
	"fmt"
	"os"
	"regexp"
	"sort"
	"strings"
)

// Mapping is one themed atlas: a set of sources to scan and a tree of blocks.
type Mapping struct {
	ID      string   `json:"id"`
	Title   string   `json:"title"`
	Layout  string   `json:"layout"`
	Sources []string `json:"sources"`
	Blocks  []Block  `json:"blocks"`
}

// Block is a logical container. A block with children is a boundary; a block
// without children is a leaf that holds nodes directly.
type Block struct {
	ID       string  `json:"id"`
	Name     string  `json:"name"`
	Theme    string  `json:"theme,omitempty"`
	Match    *Match  `json:"match,omitempty"`
	Children []Block `json:"children,omitempty"`
}

// Match decides which entities land in a block.
//
//	Name      exact type names — highest precedence, for entities whose meaning
//	          is not inferable from their location.
//	NameRegex regex over the type name.
//	Path      repo-relative path prefixes. Longest matching prefix wins, so
//	          "…/Llm/Middleware/" beats "…/Llm/". This is what makes the tool
//	          automatic: a new file in a known folder places itself.
type Match struct {
	Name      []string `json:"name,omitempty"`
	NameRegex []string `json:"nameRegex,omitempty"`
	Path      []string `json:"path,omitempty"`
}

// Slot is a block that can hold nodes directly: either a leaf, or an inner
// block that also declares its own Match. Nesting depth is unlimited — go as
// deep as the meaning goes.
type Slot struct {
	Block Block
	Path  []string // ancestor ids, root first, excluding Block itself
	Depth int
}

// Resolution is the outcome of matching one entity against the mapping.
type Resolution struct {
	LeafID string
	Reason string // "name", "regex", "path:<prefix>"
}

// Resolver matches entities to leaves.
type Resolver struct {
	slots   []Slot
	byName  map[string]string // type name -> block id
	byRegex []regexRule
	byPath  []pathRule
}

type regexRule struct {
	re     *regexp.Regexp
	leafID string
	src    string
}

type pathRule struct {
	prefix string
	leafID string
}

// Load reads a mapping file from disk.
func Load(path string) (*Mapping, error) {
	raw, err := os.ReadFile(path)
	if err != nil {
		return nil, err
	}
	var m Mapping
	if err := json.Unmarshal(raw, &m); err != nil {
		return nil, fmt.Errorf("%s: %w", path, err)
	}
	if len(m.Blocks) == 0 {
		return nil, fmt.Errorf("%s: mapping has no blocks", path)
	}
	return &m, nil
}

// Slots walks the block tree to any depth and returns every block that can
// hold nodes: leaves, plus inner blocks that declare their own Match.
func (m *Mapping) Slots() []Slot {
	var out []Slot
	var walk func(blocks []Block, path []string, depth int)
	walk = func(blocks []Block, path []string, depth int) {
		for _, b := range blocks {
			if len(b.Children) == 0 || b.Match != nil {
				out = append(out, Slot{Block: b, Path: append([]string(nil), path...), Depth: depth})
			}
			if len(b.Children) > 0 {
				walk(b.Children, append(path, b.ID), depth+1)
			}
		}
	}
	walk(m.Blocks, nil, 0)
	return out
}

// Walk visits every block in the tree, parents before children.
func (m *Mapping) Walk(fn func(b Block, path []string, depth int)) {
	var walk func(blocks []Block, path []string, depth int)
	walk = func(blocks []Block, path []string, depth int) {
		for _, b := range blocks {
			fn(b, path, depth)
			if len(b.Children) > 0 {
				walk(b.Children, append(path, b.ID), depth+1)
			}
		}
	}
	walk(m.Blocks, nil, 0)
}

// NewResolver builds the lookup tables and reports mapping-level mistakes
// (the same name or path prefix claimed by two blocks).
func NewResolver(m *Mapping) (*Resolver, []string) {
	r := &Resolver{byName: map[string]string{}}
	var problems []string
	seenPrefix := map[string]string{}
	seenBlockID := map[string]bool{}

	m.Walk(func(b Block, _ []string, _ int) {
		if seenBlockID[b.ID] {
			problems = append(problems, fmt.Sprintf("duplicate block id %q", b.ID))
		}
		seenBlockID[b.ID] = true
	})

	r.slots = m.Slots()
	for _, sl := range r.slots {
		if sl.Block.Match == nil {
			continue
		}
		for _, n := range sl.Block.Match.Name {
			if prev, dup := r.byName[n]; dup {
				problems = append(problems, fmt.Sprintf(
					"name %q claimed by both %q and %q", n, prev, sl.Block.ID))
				continue
			}
			r.byName[n] = sl.Block.ID
		}
		for _, rx := range sl.Block.Match.NameRegex {
			re, err := regexp.Compile(rx)
			if err != nil {
				problems = append(problems, fmt.Sprintf(
					"block %q: bad regex %q: %v", sl.Block.ID, rx, err))
				continue
			}
			r.byRegex = append(r.byRegex, regexRule{re: re, leafID: sl.Block.ID, src: rx})
		}
		for _, p := range sl.Block.Match.Path {
			p = normalizePrefix(p)
			if prev, dup := seenPrefix[p]; dup {
				problems = append(problems, fmt.Sprintf(
					"path prefix %q claimed by both %q and %q", p, prev, sl.Block.ID))
				continue
			}
			seenPrefix[p] = sl.Block.ID
			r.byPath = append(r.byPath, pathRule{prefix: p, leafID: sl.Block.ID})
		}
	}

	// longest prefix first, so the most specific rule wins
	sort.Slice(r.byPath, func(i, j int) bool {
		return len(r.byPath[i].prefix) > len(r.byPath[j].prefix)
	})
	return r, problems
}

// Slots exposes the flattened container list.
func (r *Resolver) Slots() []Slot { return r.slots }

// StaleNames returns explicit name rules that matched nothing in this scan.
// A rule pointing at a type that no longer exists is mapping rot: it hides a
// rename or a deletion behind a diagram that still looks complete.
func (r *Resolver) StaleNames(present map[string]bool) []string {
	var stale []string
	for name := range r.byName {
		if !present[name] {
			stale = append(stale, name)
		}
	}
	sort.Strings(stale)
	return stale
}

// Resolve matches one entity. Returns nil when nothing claims it — the caller
// parks it in the unplaced zone.
func (r *Resolver) Resolve(typeName, codeRef string) *Resolution {
	if id, ok := r.byName[typeName]; ok {
		return &Resolution{LeafID: id, Reason: "name"}
	}
	for _, rr := range r.byRegex {
		if rr.re.MatchString(typeName) {
			return &Resolution{LeafID: rr.leafID, Reason: "regex:" + rr.src}
		}
	}
	ref := normalizePrefix(codeRef)
	for _, pr := range r.byPath {
		if strings.HasPrefix(ref, pr.prefix) {
			return &Resolution{LeafID: pr.leafID, Reason: "path:" + pr.prefix}
		}
	}
	return nil
}

func normalizePrefix(p string) string {
	p = strings.ReplaceAll(p, "\\", "/")
	p = strings.TrimSuffix(p, "*")
	return p
}
