package main

import (
	"fmt"
	"sort"
	"strings"

	"spla-arch/layout"
	"spla-arch/mapping"
	"spla-arch/model"
	"spla-arch/parser"
)

const unplacedID = "__unplaced"

type buildResult struct {
	Diagram      *model.Diagram
	Drift        mapping.Drift
	Placement    map[string]string
	Problems     []string
	Unplaced     int
	Inherited    int
	LayoutNotes  []string
}

// buildAtlas regenerates a semantic-atlas diagram from a mapping file.
// Everything the mapping does not claim is parked in a loud zone on the canvas
// instead of being silently dropped — that parking lot is the work queue.
//
// prevDiagram is the atlas this same command wrote last time, if any. Its
// geometry (zone/node X/Y/Width/Height), its zone Style and Metadata, and
// any node Metadata.description a human or a prior session wrote are
// authored content, not derived data — build never recomputes them. Only
// genuinely new entities and zones get a fresh position; see
// layout.ApplyPreserving.
func buildAtlas(
	m *mapping.Mapping, repoRoot string, prev *mapping.Known, semantic []model.Edge, prevDiagram *model.Diagram,
) (*buildResult, error) {
	res, problems := mapping.NewResolver(m)

	scan, err := parser.ScanSources(repoRoot, m.Sources)
	if err != nil {
		return nil, err
	}
	entities := scan.Entities
	for _, root := range scan.MissingRoots {
		problems = append(problems, fmt.Sprintf("источник %q не найден на диске", root))
	}
	for name, refs := range scan.Collisions {
		problems = append(problems, fmt.Sprintf(
			"имя %q объявлено в нескольких файлах (%s) — на схеме останется одно",
			name, strings.Join(refs, ", ")))
	}

	nodeID := func(name string) string { return "n_" + strings.ToLower(name) }

	placement := map[string]string{}
	members := map[string][]string{}
	label := map[string]string{}
	var nodes []model.Node

	// Pass 1: rules. Pass 2: file-mate inheritance — a companion type declared
	// in a file whose main type is already placed belongs to the same block,
	// because one file is one unit of meaning. Only a type with no placed
	// sibling is a real decision and gets parked.
	resolved := make([]string, len(entities))
	fileOwner := map[string]string{}
	for i, e := range entities {
		if r := res.Resolve(e.Name, e.CodeRef); r != nil {
			resolved[i] = r.LeafID
			if _, taken := fileOwner[e.CodeRef]; !taken {
				fileOwner[e.CodeRef] = r.LeafID
			}
		}
	}
	inherited := 0
	for i, e := range entities {
		if resolved[i] == "" {
			if owner, ok := fileOwner[e.CodeRef]; ok {
				resolved[i] = owner
				inherited++
			}
		}
	}

	prevNodes := map[string]model.Node{}
	if prevDiagram != nil {
		for _, n := range prevDiagram.Nodes {
			prevNodes[n.ID] = n
		}
	}

	for i, e := range entities {
		target := unplacedID
		if resolved[i] != "" {
			target = resolved[i]
			placement[e.Name] = target
		} else {
			placement[e.Name] = ""
		}
		id := nodeID(e.Name)
		members[target] = append(members[target], id)
		label[id] = e.Name

		meta := map[string]interface{}{
			"codeRef": e.CodeRef,
			"type":    strings.ToUpper(e.Kind[:1]) + e.Kind[1:],
		}
		// A description is never derived from the code — nothing here writes
		// one. If a human (or an earlier session) already put one on this
		// node, carry it forward instead of dropping it on every rebuild.
		if pn, ok := prevNodes[id]; ok {
			if desc, ok := pn.Metadata["description"]; ok {
				meta["description"] = desc
			}
		}

		nodes = append(nodes, model.Node{
			ID:       id,
			Label:    e.Name,
			Type:     parser.NodeType(e),
			Metadata: meta,
		})
	}

	// --- build the box tree, mirroring the mapping tree at any depth ---
	var toBox func(b mapping.Block, inheritedTheme string) *layout.Box
	toBox = func(b mapping.Block, inheritedTheme string) *layout.Box {
		theme := b.Theme
		if theme == "" {
			theme = inheritedTheme
		}
		box := &layout.Box{ID: b.ID, Name: b.Name, Theme: theme}
		if ids := members[b.ID]; len(ids) > 0 {
			layout.SortNodeIDs(ids, label)
			box.NodeIDs = ids
		}
		for _, c := range b.Children {
			cb := toBox(c, theme)
			if cb != nil {
				box.Children = append(box.Children, cb)
			}
		}
		if len(box.NodeIDs) == 0 && len(box.Children) == 0 {
			return nil // an empty block is not drawn
		}
		box.Name = fmt.Sprintf("%s (%d)", b.Name, countNodes(box))
		return box
	}

	var roots []*layout.Box
	for _, b := range m.Blocks {
		if box := toBox(b, b.Theme); box != nil {
			roots = append(roots, box)
		}
	}

	unplaced := members[unplacedID]
	if len(unplaced) > 0 {
		layout.SortNodeIDs(unplaced, label)
		roots = append(roots, &layout.Box{
			ID:      unplacedID,
			Name:    fmt.Sprintf("НЕ РАЗМЕЩЕНО (%d) — требует решения", len(unplaced)),
			Theme:   "unplaced",
			NodeIDs: unplaced,
		})
	}

	diag := &model.Diagram{
		Metadata: model.Metadata{
			Title:       m.Title,
			Description: fmt.Sprintf("%d entities from %s", len(nodes), strings.Join(m.Sources, ", ")),
			Version:     "2.0.0",
		},
		Views: []model.View{{ID: "all", Name: "Полный атлас", Icon: "🧠"}},
		Nodes: nodes,
	}
	layoutNotes := layout.ApplyPreserving(diag, roots, layout.LoadPrevLayout(prevDiagram))

	// --- edges: structure extracted from code, flows curated by hand ---
	present := map[string]bool{}
	kind := map[string]string{}
	for _, e := range entities {
		present[e.Name] = true
		kind[e.Name] = e.Kind
	}
	if stale := res.StaleNames(present); len(stale) > 0 {
		problems = append(problems, fmt.Sprintf(
			"правила указывают на несуществующие типы (переименованы или удалены): %s",
			strings.Join(stale, ", ")))
	}
	var edges []model.Edge
	for _, e := range entities {
		for _, base := range e.Bases {
			if !present[base] {
				continue // outside the scanned scope: not our diagram's business
			}
			t := "extends"
			if kind[base] == "interface" {
				t = "implements"
			}
			edges = append(edges, model.Edge{
				ID:   fmt.Sprintf("s_%s_%s", strings.ToLower(e.Name), strings.ToLower(base)),
				From: nodeID(e.Name), To: nodeID(base), Type: t,
			})
		}
	}
	edges = append(edges, semantic...)
	// keep only edges whose ends exist
	have := map[string]bool{}
	for _, n := range nodes {
		have[n.ID] = true
	}
	var filtered []model.Edge
	for _, e := range edges {
		if have[e.From] && have[e.To] {
			filtered = append(filtered, e)
		}
	}
	sortEdges(filtered)
	diag.Edges = filtered

	return &buildResult{
		Diagram:     diag,
		Drift:       mapping.Compare(prev, placement),
		Placement:   placement,
		Problems:    problems,
		Unplaced:    len(unplaced),
		Inherited:   inherited,
		LayoutNotes: layoutNotes,
	}, nil
}

func countNodes(b *layout.Box) int {
	n := len(b.NodeIDs)
	for _, c := range b.Children {
		n += countNodes(c)
	}
	return n
}

// report prints the human-facing summary: what is parked, what appeared,
// what a mapping edit moved.
func report(r *buildResult) {
	d := r.Drift
	for _, p := range r.Problems {
		fmt.Printf("  ⚠ mapping: %s\n", p)
	}
	if r.Inherited > 0 {
		fmt.Printf("  ↳ по соседству в файле размещено: %d\n", r.Inherited)
	}
	if r.Unplaced > 0 {
		fmt.Printf("  📦 не размещено: %d (оранжевая зона на схеме)\n", r.Unplaced)
	}
	if len(d.NewUnplaced) > 0 {
		fmt.Printf("  🆕 новые и НЕ размещённые (%d):\n", len(d.NewUnplaced))
		for _, it := range d.NewUnplaced {
			fmt.Printf("      %s\n", it.Name)
		}
	}
	if len(d.NewPlaced) > 0 {
		fmt.Printf("  🆕 новые, размещены правилом (%d) — стоит проверить:\n", len(d.NewPlaced))
		for _, it := range d.NewPlaced {
			fmt.Printf("      %s → %s\n", it.Name, it.To)
		}
	}
	if len(d.Moved) > 0 {
		fmt.Printf("  ↔ переехали (%d):\n", len(d.Moved))
		for _, it := range d.Moved {
			fmt.Printf("      %s: %s → %s\n", it.Name, orNone(it.From), orNone(it.To))
		}
	}
	if len(d.Removed) > 0 {
		fmt.Printf("  🗑 удалены из кода (%d): %s\n", len(d.Removed), strings.Join(d.Removed, ", "))
	}
	if len(r.LayoutNotes) > 0 {
		fmt.Printf("  📐 новая геометрия — не тронута кураторская, но требует взгляда (%d):\n", len(r.LayoutNotes))
		for _, note := range r.LayoutNotes {
			fmt.Printf("      %s\n", note)
		}
	}
	if d.Clean() && r.Unplaced == 0 && len(r.Problems) == 0 && len(r.LayoutNotes) == 0 {
		fmt.Println("  ✓ всё разложено, изменений с прошлого прогона нет")
	}
}

func orNone(s string) string {
	if s == "" {
		return "(не размещено)"
	}
	return s
}

// loadSemanticEdges reads the hand-curated flow edges (call, data-flow, event,
// security, storage). They are authored, not extracted, because runtime intent
// is not visible in a type declaration.
func sortEdges(edges []model.Edge) {
	sort.Slice(edges, func(i, j int) bool { return edges[i].ID < edges[j].ID })
}
