package main

import (
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"
	"sort"
	"strings"

	"spla-arch/mapping"
	"spla-arch/model"
	"spla-arch/parser"
)

var jsonUnmarshal = json.Unmarshal

// Verification takes the triple — discovered entities, the rules for one point
// of view, and the diagram JSON — and answers one question: is anything
// broken, extra, or missing? It changes nothing. The report is the handoff to
// an agent, who then edits either the rules or the schema.
type finding struct {
	Kind   string // "сломано" | "лишнее" | "не хватает"
	Detail string
}

type verdict struct {
	Diagram  string
	Layout   string
	Findings []finding
}

func (v *verdict) add(kind, format string, args ...interface{}) {
	v.Findings = append(v.Findings, finding{Kind: kind, Detail: fmt.Sprintf(format, args...)})
}

const (
	kBroken  = "сломано"
	kExtra   = "лишнее"
	kMissing = "не хватает"
)

// verifyDiagram runs every check that applies to this diagram's variant.
// Completeness is demanded only from a semantic-atlas: a message-flow shows a
// deliberate subset, and calling that "missing" would be wrong.
func verifyDiagram(diagPath, mapPath, repoRoot string) (*verdict, error) {
	diag, err := readDiagram(diagPath)
	if err != nil {
		return nil, err
	}
	v := &verdict{Diagram: filepath.Base(diagPath), Layout: diag.Metadata.Layout}
	if v.Layout == "" {
		v.Layout = "(не объявлен)"
		v.add(kBroken, "не объявлен metadata.layout — непонятно, по каким правилам судить схему")
	}

	verifyStructure(diag, v)
	verifyGeometry(diag, v)

	// Entity-level checks need a source of truth to compare against.
	var entities []parser.Entity
	var m *mapping.Mapping
	var roots []string
	if mapPath != "" {
		m, err = mapping.Load(mapPath)
		if err != nil {
			return nil, err
		}
		roots = m.Sources
	} else {
		roots = inferSources(diag)
	}
	if len(roots) > 0 {
		scan, err := parser.ScanSources(repoRoot, roots)
		if err != nil {
			return nil, err
		}
		entities = scan.Entities
		for _, r := range scan.MissingRoots {
			v.add(kExtra, "схема ссылается на исходники в %q, а такого каталога нет — дерево переехало", r)
		}
	}

	if len(entities) > 0 {
		verifyAgainstCode(diag, entities, repoRoot, v)
		if m != nil {
			verifyRules(m, diag, entities, v)
		}
	}
	return v, nil
}

// --- structural integrity: the graph must be internally consistent ---

func verifyStructure(d *model.Diagram, v *verdict) {
	nodeIDs := map[string]bool{}
	labels := map[string]int{}
	for _, n := range d.Nodes {
		if nodeIDs[n.ID] {
			v.add(kBroken, "дубль id узла: %s", n.ID)
		}
		nodeIDs[n.ID] = true
		labels[n.Label]++
	}
	for label, c := range labels {
		if c > 1 {
			v.add(kBroken, "сущность %q встречается %d раза — один класс должен быть в одном месте", label, c)
		}
	}

	zoneIDs := map[string]bool{}
	for _, z := range d.Zones {
		if zoneIDs[z.ID] {
			v.add(kBroken, "дубль id зоны: %s", z.ID)
		}
		zoneIDs[z.ID] = true
	}
	for _, n := range d.Nodes {
		if n.Zone != "" && !zoneIDs[n.Zone] {
			v.add(kBroken, "узел %q ссылается на несуществующую зону %q", n.Label, n.Zone)
		}
	}

	edgeIDs := map[string]bool{}
	for _, e := range d.Edges {
		if edgeIDs[e.ID] {
			v.add(kBroken, "дубль id связи: %s", e.ID)
		}
		edgeIDs[e.ID] = true
		if !nodeIDs[e.From] {
			v.add(kBroken, "связь %s ведёт из несуществующего узла %q", e.ID, e.From)
		}
		if !nodeIDs[e.To] {
			v.add(kBroken, "связь %s ведёт в несуществующий узел %q", e.ID, e.To)
		}
		if e.Type != "" && !knownEdgeTypes[e.Type] {
			v.add(kBroken, "связь %s имеет неизвестный тип %q — рендерер нарисует её серой заглушкой", e.ID, e.Type)
		}
	}
}

var knownEdgeTypes = map[string]bool{
	"extends": true, "implements": true, "realizes": true, "composes": true,
	"call": true, "data-flow": true, "event": true, "security": true, "storage": true,
}

// --- geometry: containers must actually contain, and not collide ---

func verifyGeometry(d *model.Diagram, v *verdict) {
	zoneByID := map[string]*model.Zone{}
	for i := range d.Zones {
		zoneByID[d.Zones[i].ID] = &d.Zones[i]
	}
	for _, n := range d.Nodes {
		z := zoneByID[n.Zone]
		if z == nil {
			continue
		}
		if n.X < z.X || n.Y < z.Y || n.X+n.Width > z.X+z.Width || n.Y+n.Height > z.Y+z.Height {
			v.add(kBroken, "узел %q торчит за границу своей зоны %q", n.Label, z.Name)
		}
	}
	depth := func(z *model.Zone) int {
		if z.Metadata != nil {
			if dv, ok := z.Metadata["depth"].(float64); ok {
				return int(dv)
			}
		}
		return 0
	}
	for i := 0; i < len(d.Zones); i++ {
		for j := i + 1; j < len(d.Zones); j++ {
			a, b := &d.Zones[i], &d.Zones[j]
			if depth(a) != depth(b) {
				continue
			}
			if a.X < b.X+b.Width && b.X < a.X+a.Width && a.Y < b.Y+b.Height && b.Y < a.Y+a.Height {
				v.add(kBroken, "зоны одного уровня перекрываются: %q и %q", a.Name, b.Name)
			}
		}
	}
}

// --- against the code: dangling references and, for an atlas, completeness ---

func verifyAgainstCode(d *model.Diagram, entities []parser.Entity, repoRoot string, v *verdict) {
	byName := map[string]parser.Entity{}
	for _, e := range entities {
		byName[e.Name] = e
	}

	onDiagram := map[string]bool{}
	for _, n := range d.Nodes {
		onDiagram[n.Label] = true
		ref, _ := n.Metadata["codeRef"].(string)
		if ref == "" {
			continue
		}
		if _, err := os.Stat(filepath.Join(repoRoot, filepath.FromSlash(ref))); err != nil {
			v.add(kExtra, "узел %q ссылается на несуществующий файл %s", n.Label, ref)
			continue
		}
		if e, ok := byName[n.Label]; ok && e.CodeRef != ref {
			v.add(kBroken, "узел %q указывает на %s, а тип объявлен в %s", n.Label, ref, e.CodeRef)
		}
	}

	// Completeness is a demand of the atlas variant only.
	if d.Metadata.Layout != "semantic-atlas" {
		return
	}
	var missing []string
	for _, e := range entities {
		if !onDiagram[e.Name] {
			missing = append(missing, e.Name)
		}
	}
	sort.Strings(missing)
	if len(missing) > 0 {
		v.add(kMissing, "атлас обязан быть полным, но %d сущностей нет на схеме: %s",
			len(missing), preview(missing, 12))
	}
	for _, n := range d.Nodes {
		if _, ok := byName[n.Label]; !ok {
			v.add(kExtra, "узел %q есть на схеме, но такого типа в коде нет", n.Label)
		}
	}
}

// --- against the rules: does the point of view still describe the code? ---

func verifyRules(m *mapping.Mapping, d *model.Diagram, entities []parser.Entity, v *verdict) {
	res, problems := mapping.NewResolver(m)
	for _, p := range problems {
		v.add(kBroken, "правила: %s", p)
	}

	present := map[string]bool{}
	for _, e := range entities {
		present[e.Name] = true
	}
	if stale := res.StaleNames(present); len(stale) > 0 {
		v.add(kExtra, "правила указывают на несуществующие типы: %s", preview(stale, 12))
	}

	fileOwner := map[string]bool{}
	for _, e := range entities {
		if res.Resolve(e.Name, e.CodeRef) != nil {
			fileOwner[e.CodeRef] = true
		}
	}
	var homeless []string
	for _, e := range entities {
		if res.Resolve(e.Name, e.CodeRef) == nil && !fileOwner[e.CodeRef] {
			homeless = append(homeless, e.Name)
		}
	}
	sort.Strings(homeless)
	if len(homeless) > 0 {
		v.add(kMissing, "ни одно правило не claims %d сущностей: %s", len(homeless), preview(homeless, 12))
	}

	// A block declared in the rules but empty in practice is dead weight.
	used := map[string]bool{}
	for _, e := range entities {
		if r := res.Resolve(e.Name, e.CodeRef); r != nil {
			used[r.LeafID] = true
		}
	}
	var empty []string
	for _, sl := range res.Slots() {
		if sl.Block.Match != nil && !used[sl.Block.ID] {
			empty = append(empty, sl.Block.ID)
		}
	}
	sort.Strings(empty)
	if len(empty) > 0 {
		v.add(kExtra, "блоки объявлены, но пусты: %s", preview(empty, 12))
	}
}

func preview(items []string, n int) string {
	if len(items) <= n {
		return strings.Join(items, ", ")
	}
	return strings.Join(items[:n], ", ") + fmt.Sprintf(" … (+%d)", len(items)-n)
}

func inferSources(d *model.Diagram) []string {
	seen := map[string]bool{}
	var out []string
	for _, n := range d.Nodes {
		ref, _ := n.Metadata["codeRef"].(string)
		parts := strings.Split(ref, "/")
		if len(parts) < 2 || parts[0] != "src" {
			continue
		}
		root := parts[0] + "/" + parts[1]
		if !seen[root] {
			seen[root] = true
			out = append(out, root)
		}
	}
	sort.Strings(out)
	return out
}

// --- reporting ---

func printVerdict(v *verdict) bool {
	byKind := map[string][]string{}
	for _, f := range v.Findings {
		byKind[f.Kind] = append(byKind[f.Kind], f.Detail)
	}
	fmt.Printf("\n🔎 %s  [%s]\n", v.Diagram, v.Layout)
	if len(v.Findings) == 0 {
		fmt.Println("   ✓ целостно: ничего не сломано, лишнего и недостающего нет")
		return true
	}
	for _, kind := range []string{kBroken, kExtra, kMissing} {
		items := byKind[kind]
		if len(items) == 0 {
			continue
		}
		icon := map[string]string{kBroken: "❌", kExtra: "➕", kMissing: "➖"}[kind]
		fmt.Printf("   %s %s (%d):\n", icon, kind, len(items))
		for _, it := range items {
			fmt.Printf("      · %s\n", it)
		}
	}
	return false
}

func readDiagram(path string) (*model.Diagram, error) {
	raw, err := os.ReadFile(path)
	if err != nil {
		return nil, err
	}
	var d model.Diagram
	if err := jsonUnmarshal(raw, &d); err != nil {
		return nil, fmt.Errorf("%s: %w", path, err)
	}
	return &d, nil
}
