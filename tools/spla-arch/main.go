package main

import (
	"encoding/json"
	"flag"
	"fmt"
	"log"
	"os"

	"spla-arch/layout"
	"spla-arch/model"
	"spla-arch/parser"
)

func main() {
	if len(os.Args) < 2 {
		printUsage()
		return
	}

	cmd := os.Args[1]

	switch cmd {
	case "parse":
		parseCmd := flag.NewFlagSet("parse", flag.ExitOnError)
		srcDir := parseCmd.String("src", "", "Path to C# source directory (e.g. ./src/core)")
		outPath := parseCmd.String("out", "model.json", "Output JSON diagram path")
		title := parseCmd.String("title", "", "Diagram title")
		parseCmd.Parse(os.Args[2:])

		if *srcDir == "" {
			fmt.Println("Error: --src is required")
			parseCmd.Usage()
			os.Exit(1)
		}

		fmt.Printf("🔍 Scanning C# codebase in '%s'...\n", *srcDir)
		diag, err := parser.ParseCSharpFolder(*srcDir, *title)
		if err != nil {
			log.Fatalf("Parse error: %v", err)
		}

		saveDiagram(diag, *outPath)
		fmt.Printf("✅ Success! Extracted %d nodes, %d zones, %d relationships -> %s\n",
			len(diag.Nodes), len(diag.Zones), len(diag.Edges), *outPath)

	case "layout":
		layoutCmd := flag.NewFlagSet("layout", flag.ExitOnError)
		filePath := layoutCmd.String("file", "", "Path to JSON diagram file")
		layoutCmd.Parse(os.Args[2:])

		if *filePath == "" {
			fmt.Println("Error: --file is required")
			layoutCmd.Usage()
			os.Exit(1)
		}

		diag := loadDiagram(*filePath)
		layout.ApplyGrid(diag)
		saveDiagram(diag, *filePath)
		fmt.Printf("✅ Grid layout updated for %s\n", *filePath)

	case "build", "check":
		buildCmd := flag.NewFlagSet(cmd, flag.ExitOnError)
		mapPath := buildCmd.String("mapping", "", "Path to a .map.json mapping file")
		repoRoot := buildCmd.String("repo", "../..", "Repository root that codeRef paths are relative to")
		outPath := buildCmd.String("out", "", "Output diagram JSON (defaults to the mapping's sibling)")
		edgesPath := buildCmd.String("edges", "", "Optional curated flow-edges JSON")
		buildCmd.Parse(os.Args[2:])

		if *mapPath == "" {
			fmt.Println("Error: --mapping is required")
			buildCmd.Usage()
			os.Exit(1)
		}
		runBuild(cmd, *mapPath, *repoRoot, *outPath, *edgesPath)

	case "verify":
		vCmd := flag.NewFlagSet("verify", flag.ExitOnError)
		diagPath := vCmd.String("diagram", "", "Diagram JSON to verify (omit with --dir to verify all)")
		dirPath := vCmd.String("dir", "", "Verify every model-*.json in this directory")
		mapPath := vCmd.String("mapping", "", "Rules for this point of view (optional)")
		repoRoot := vCmd.String("repo", "../..", "Repository root that codeRef paths are relative to")
		vCmd.Parse(os.Args[2:])

		if *diagPath == "" && *dirPath == "" {
			fmt.Println("Error: pass --diagram or --dir")
			vCmd.Usage()
			os.Exit(1)
		}
		runVerify(*diagPath, *dirPath, *mapPath, *repoRoot)

	case "help":
		printUsage()

	default:
		fmt.Printf("Unknown command: %s\n", cmd)
		printUsage()
		os.Exit(1)
	}
}

func printUsage() {
	fmt.Println("=====================================================")
	fmt.Println("  SPLA Architecture CLI (tools/spla-arch)")
	fmt.Println("=====================================================")
	fmt.Println("Commands:")
	fmt.Println("  parse   - Scan C# directory, extract classes/interfaces/relations into JSON")
	fmt.Println("            Example: go run main.go parse --src ../../src/core --out model-core.json")
	fmt.Println("")
	fmt.Println("  layout  - Align nodes to grid inside their zones")
	fmt.Println("            Example: go run main.go layout --file model.json")
	fmt.Println("=====================================================")
}

func loadDiagram(path string) *model.Diagram {
	bytes, err := os.ReadFile(path)
	if err != nil {
		log.Fatalf("Cannot read file %s: %v", path, err)
	}
	var diag model.Diagram
	if err := json.Unmarshal(bytes, &diag); err != nil {
		log.Fatalf("Cannot parse JSON in %s: %v", path, err)
	}
	return &diag
}

func saveDiagram(diag *model.Diagram, path string) {
	bytes, err := json.MarshalIndent(diag, "", "  ")
	if err != nil {
		log.Fatalf("Cannot serialize diagram: %v", err)
	}
	if err := os.WriteFile(path, bytes, 0644); err != nil {
		log.Fatalf("Cannot write file %s: %v", path, err)
	}
}
