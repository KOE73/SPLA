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
)

// runBuild regenerates one atlas from its mapping.
//
//	build  — writes the diagram and updates the lockfile
//	check  — writes nothing, exits 1 if anything needs a human decision
//
// The split matters: `build` is what a hook or CI runs on every change;
// `check` is what fails the build when something new has nowhere to live.
func runBuild(cmd, mapPath, repoRoot, outPath, edgesPath string) {
	m, err := mapping.Load(mapPath)
	if err != nil {
		fmt.Printf("❌ %v\n", err)
		os.Exit(1)
	}

	dir := filepath.Dir(mapPath)
	if outPath == "" {
		outPath = filepath.Join(dir, "model-"+m.ID+".json")
	}
	lockPath := filepath.Join(dir, m.ID+".known.json")
	if edgesPath == "" {
		candidate := filepath.Join(dir, m.ID+".edges.json")
		if _, err := os.Stat(candidate); err == nil {
			edgesPath = candidate
		}
	}

	semantic := loadEdges(edgesPath)
	prev, hadLock := mapping.LoadKnown(lockPath)

	res, err := buildAtlas(m, repoRoot, prev, semantic)
	if err != nil {
		fmt.Printf("❌ %v\n", err)
		os.Exit(1)
	}

	fmt.Printf("🗺  %s — %d сущностей, %d зон, %d связей\n",
		m.Title, len(res.Diagram.Nodes), len(res.Diagram.Zones), len(res.Diagram.Edges))
	if !hadLock {
		fmt.Println("  ℹ первый прогон: базы для сравнения нет, дрейф не считается")
	}
	report(res)

	needsAttention := res.Unplaced > 0 || len(res.Problems) > 0 || !res.Drift.Clean()

	if cmd == "check" {
		if needsAttention {
			fmt.Println("\n❌ check: схема требует решения человека")
			os.Exit(1)
		}
		fmt.Println("\n✓ check: чисто")
		return
	}

	res.Diagram.Metadata.Layout = "semantic-atlas"
	res.Diagram.Metadata.Mapping = filepath.Base(mapPath)
	res.Diagram.Metadata.Generated = true
	writeJSON(outPath, res.Diagram)
	if err := mapping.SaveKnown(lockPath, m.ID, res.Placement); err != nil {
		fmt.Printf("⚠ не удалось записать %s: %v\n", lockPath, err)
	}
	fmt.Printf("\n✅ %s\n   lock: %s\n", outPath, lockPath)
	if needsAttention {
		fmt.Println("   ⤷ есть что разобрать — см. список выше и оранжевую зону на схеме")
	}
}

// runVerify checks one diagram, or every diagram in a directory, against the
// code and (when given) against the rules. It writes nothing: the report is
// the product. Exit 1 means a human or an agent has a decision to make.
func runVerify(diagPath, dirPath, mapPath, repoRoot string) {
	var targets []string
	if diagPath != "" {
		targets = append(targets, diagPath)
	}
	if dirPath != "" {
		matches, err := filepath.Glob(filepath.Join(dirPath, "model*.json"))
		if err != nil {
			fmt.Printf("❌ %v\n", err)
			os.Exit(1)
		}
		sort.Strings(matches)
		targets = append(targets, matches...)
	}

	allClean := true
	for _, t := range targets {
		mp := mapPath
		if mp == "" {
			mp = guessMapping(t)
		}
		v, err := verifyDiagram(t, mp, repoRoot)
		if err != nil {
			fmt.Printf("\n❌ %s: %v\n", filepath.Base(t), err)
			allClean = false
			continue
		}
		if !printVerdict(v) {
			allClean = false
		}
	}

	if allClean {
		fmt.Println("\n✓ всё сходится")
		return
	}
	fmt.Println("\n⤷ выше список того, что надо поправить — либо правила, либо схему")
	os.Exit(1)
}

// guessMapping finds the rules a generated diagram declares in its metadata.
// Rule sets are filed per point of view (mapping/<layout>/<id>.map.json), so
// the lookup walks the mapping tree rather than assuming one flat folder.
func guessMapping(diagPath string) string {
	d, err := readDiagram(diagPath)
	if err != nil || d.Metadata.Mapping == "" {
		return ""
	}
	root := filepath.Join(filepath.Dir(diagPath), "mapping")
	var found string
	filepath.Walk(root, func(p string, info os.FileInfo, err error) error {
		if err != nil || info.IsDir() || found != "" {
			return nil
		}
		if info.Name() == d.Metadata.Mapping {
			found = p
		}
		return nil
	})
	return found
}

func loadEdges(path string) []model.Edge {
	if path == "" {
		return nil
	}
	raw, err := os.ReadFile(path)
	if err != nil {
		return nil
	}
	var wrapper struct {
		Edges []model.Edge `json:"edges"`
	}
	if err := json.Unmarshal(raw, &wrapper); err != nil {
		fmt.Printf("⚠ %s: %v\n", path, err)
		return nil
	}
	for i := range wrapper.Edges {
		wrapper.Edges[i].From = normalizeRef(wrapper.Edges[i].From)
		wrapper.Edges[i].To = normalizeRef(wrapper.Edges[i].To)
		if wrapper.Edges[i].ID == "" {
			wrapper.Edges[i].ID = fmt.Sprintf("f_%s_%s", wrapper.Edges[i].From, wrapper.Edges[i].To)
		}
	}
	fmt.Printf("  ↳ смысловых связей из %s: %d\n", filepath.Base(path), len(wrapper.Edges))
	return wrapper.Edges
}

// normalizeRef lets the curated edges file name plain type names
// ("ChatManager") instead of node ids ("n_chatmanager").
func normalizeRef(s string) string {
	if strings.HasPrefix(s, "n_") {
		return s
	}
	return "n_" + strings.ToLower(s)
}

func writeJSON(path string, v interface{}) {
	raw, err := json.MarshalIndent(v, "", "  ")
	if err != nil {
		fmt.Printf("❌ serialize: %v\n", err)
		os.Exit(1)
	}
	if err := os.WriteFile(path, append(raw, '\n'), 0644); err != nil {
		fmt.Printf("❌ write %s: %v\n", path, err)
		os.Exit(1)
	}
}
