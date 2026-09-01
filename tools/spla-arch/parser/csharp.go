package parser

import (
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"strings"

	"spla-arch/layout"
	"spla-arch/model"
)

// typeDeclRegex captures: 1 = access modifier (may be empty), 2 = kind,
// 3 = type name, 4 = base list. Any number of trailing modifiers is allowed,
// so "public static sealed partial class" parses like "class".
// The tail is deliberately loose: a record's primary constructor may run over
// several lines, and "record X(...) : Base;" ends in a semicolon, so requiring
// a terminator here silently drops real types.
var typeDeclRegex = regexp.MustCompile(
	`(?m)^\s*(public|internal|private|protected)?\s*(?:(?:static|sealed|abstract|partial|readonly|ref|unsafe|new|file)\s+)*` +
		`(class|interface|record|struct|enum)\s+(?:class\s+|struct\s+)?([A-Za-z0-9_]+)` +
		`\s*(?:<[^>{]*>)?\s*(?:\([^()]*\))?` +
		`(?:\s*:\s*([A-Za-z0-9_,\s<>\.\?\[\]]+?)\s*(?:\{|where|;|$))?`)

// group indexes into typeDeclRegex matches
const (
	mAccess = 1
	mKind   = 2
	mName   = 3
	mBases  = 4
)

type ParsedType struct {
	Kind       string // class, interface, record, struct
	Name       string
	BaseTypes  []string
	Namespace  string
	FilePath   string
	ProjectDir string
}

// ParseCSharpFolder recursively scans a C# source directory and generates a diagram model
func ParseCSharpFolder(rootDir string, title string) (*model.Diagram, error) {
	absRoot, err := filepath.Abs(rootDir)
	if err != nil {
		return nil, err
	}

	var parsedTypes []ParsedType
	typeMap := make(map[string]*ParsedType)

	err = filepath.Walk(absRoot, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}
		if info.IsDir() {
			name := info.Name()
			if name == "bin" || name == "obj" || name == ".git" || name == ".vs" {
				return filepath.SkipDir
			}
			return nil
		}

		if !strings.HasSuffix(path, ".cs") {
			return nil
		}

		// Skip generated files
		baseName := info.Name()
		if strings.Contains(baseName, "AssemblyInfo") ||
			strings.Contains(baseName, "GlobalUsings") ||
			strings.Contains(baseName, "AssemblyAttributes") {
			return nil
		}

		contentBytes, err := os.ReadFile(path)
		if err != nil {
			return nil
		}
		content := string(contentBytes)

		relPath, _ := filepath.Rel(absRoot, path)
		relPath = filepath.ToSlash(relPath)
		projectDir := strings.Split(relPath, "/")[0]

		matches := typeDeclRegex.FindAllStringSubmatch(content, -1)
		for _, m := range matches {
			if m[mAccess] == "private" || m[mAccess] == "protected" {
				continue // implementation detail, not architecture
			}
			kind := m[mKind]
			name := m[mName]
			var baseTypes []string

			if m[mBases] != "" {
				rawBases := strings.Split(m[mBases], ",")
				for _, b := range rawBases {
					bClean := strings.TrimSpace(strings.Split(b, "<")[0])
					if bClean != "" && bClean != "where" {
						baseTypes = append(baseTypes, bClean)
					}
				}
			}

			pt := ParsedType{
				Kind:       kind,
				Name:       name,
				BaseTypes:  baseTypes,
				FilePath:   relPath,
				ProjectDir: projectDir,
			}
			parsedTypes = append(parsedTypes, pt)
			typeMap[name] = &parsedTypes[len(parsedTypes)-1]
		}

		return nil
	})

	if err != nil {
		return nil, err
	}

	// Build zones based on projects/folders
	zoneMap := make(map[string]*model.Zone)
	var zones []model.Zone
	var nodes []model.Node
	var edges []model.Edge

	curY := 40.0
	for _, pt := range parsedTypes {
		zID := "zone_" + sanitizeID(pt.ProjectDir)
		if _, exists := zoneMap[zID]; !exists {
			z := model.Zone{
				ID:         zID,
				Name:       pt.ProjectDir,
				Type:       "subsystem",
				SemanticID: "zone." + pt.ProjectDir,
				X:          40.0,
				Y:          curY,
				Width:      600.0,
				Height:     400.0,
				Style: &model.ZoneStyle{
					Fill:        "#f8fafc",
					Stroke:      "#cbd5e1",
					StrokeWidth: 2,
					HeaderBg:    "#e2e8f0",
				},
				Metadata: map[string]interface{}{
					"source": pt.ProjectDir,
				},
			}
			zones = append(zones, z)
			zoneMap[zID] = &zones[len(zones)-1]
			curY += 450.0
		}

		nodeType := "component"
		if pt.Kind == "interface" {
			nodeType = "service"
		} else if strings.Contains(strings.ToLower(pt.Name), "tool") {
			nodeType = "tool"
		} else if strings.Contains(strings.ToLower(pt.Name), "security") || strings.Contains(strings.ToLower(pt.Name), "permission") {
			nodeType = "security-component"
		} else if strings.Contains(strings.ToLower(pt.Name), "store") || strings.Contains(strings.ToLower(pt.Name), "db") {
			nodeType = "database"
		}

		nodeID := strings.ToLower(fmt.Sprintf("%s_%s", zID, pt.Name))
		node := model.Node{
			ID:     nodeID,
			Label:  pt.Name,
			Type:   nodeType,
			Zone:   zID,
			Width:  layout.DefaultNodeWidth,
			Height: layout.DefaultNodeHeight,
			Metadata: map[string]interface{}{
				"kind":    pt.Kind,
				"codeRef": pt.FilePath,
				"type":    strings.Title(pt.Kind),
			},
		}
		nodes = append(nodes, node)
	}

	// Create map of label -> node ID for edge generation
	labelToID := make(map[string]string)
	for _, n := range nodes {
		labelToID[n.Label] = n.ID
	}

	// Generate inheritance / implementation edges
	edgeIndex := 1
	for _, pt := range parsedTypes {
		fromID, okFrom := labelToID[pt.Name]
		if !okFrom {
			continue
		}

		for _, base := range pt.BaseTypes {
			if toID, okTo := labelToID[base]; okTo {
				edgeType := "extends"
				if strings.HasPrefix(base, "I") && len(base) > 1 && strings.ToUpper(string(base[1])) == string(base[1]) {
					edgeType = "implements"
				}
				edges = append(edges, model.Edge{
					ID:    fmt.Sprintf("e_auto_%d", edgeIndex),
					From:  fromID,
					To:    toID,
					Type:  edgeType,
					Label: edgeType,
				})
				edgeIndex++
			}
		}
	}

	if title == "" {
		title = fmt.Sprintf("Architecture: %s", filepath.Base(absRoot))
	}

	diag := &model.Diagram{
		Metadata: model.Metadata{
			Title:       title,
			Description: fmt.Sprintf("Extracted %d types across %d modules", len(nodes), len(zones)),
			Version:     "1.0.0",
		},
		Views: []model.View{
			{ID: "all", Name: "Все элементы", Icon: "🏛"},
		},
		Zones: zones,
		Nodes: nodes,
		Edges: edges,
	}

	layout.ApplyGrid(diag)
	return diag, nil
}

func sanitizeID(s string) string {
	s = strings.ReplaceAll(s, ".", "_")
	s = strings.ReplaceAll(s, "/", "_")
	s = strings.ReplaceAll(s, "\\", "_")
	s = strings.ReplaceAll(s, "-", "_")
	return strings.ToLower(s)
}
