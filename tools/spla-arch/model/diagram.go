package model

// Diagram represents the complete visual and semantic architecture model
type Diagram struct {
	Metadata Metadata `json:"metadata"`
	Views    []View   `json:"views"`
	Zones    []Zone   `json:"zones"`
	Nodes    []Node   `json:"nodes"`
	Edges    []Edge   `json:"edges"`
}

type Metadata struct {
	Title       string `json:"title"`
	Description string `json:"description,omitempty"`
	Version     string `json:"version,omitempty"`
	// Layout names the compositional variant (see docs/diagrams/README_RU.md).
	Layout string `json:"layout,omitempty"`
	// Mapping and Generated mark a file as derived: its coordinates come from
	// a mapping file and hand edits to them will be overwritten.
	Mapping   string `json:"mapping,omitempty"`
	Generated bool   `json:"generated,omitempty"`
}

type View struct {
	ID             string   `json:"id"`
	Name           string   `json:"name"`
	Icon           string   `json:"icon,omitempty"`
	Description    string   `json:"description,omitempty"`
	HighlightNodes []string `json:"highlightNodes,omitempty"`
	HighlightZones []string `json:"highlightZones,omitempty"`
}

type ZoneStyle struct {
	Fill            string  `json:"fill"`
	Stroke          string  `json:"stroke"`
	StrokeWidth     float64 `json:"strokeWidth"`
	StrokeDasharray string  `json:"strokeDasharray,omitempty"`
	HeaderBg        string  `json:"headerBg"`
}

type Zone struct {
	ID         string                 `json:"id"`
	Name       string                 `json:"name"`
	Type       string                 `json:"type"` // subsystem, boundary, layer, pipeline
	SemanticID string                 `json:"semanticId,omitempty"`
	X          float64                `json:"x"`
	Y          float64                `json:"y"`
	Width      float64                `json:"width"`
	Height     float64                `json:"height"`
	// StyleID names an entry in the editor's style library. Appearance is
	// edited there, once, instead of being copied into every zone.
	StyleID string `json:"styleId,omitempty"`
	// Style is the old inline colouring. Deprecated: still read so older
	// files keep rendering, never written by the generator any more. It is a
	// pointer because a struct value is never omitted by encoding/json —
	// without one, every zone would carry an empty style object for ever.
	Style    *ZoneStyle             `json:"style,omitempty"`
	Metadata map[string]interface{} `json:"metadata,omitempty"`
}

type Node struct {
	ID         string                 `json:"id"`
	Label      string                 `json:"label"`
	Type       string                 `json:"type"` // component, service, concept, database, security-component, tool, note
	Zone       string                 `json:"zone,omitempty"`
	SemanticID string                 `json:"semanticId,omitempty"`
	X          float64                `json:"x"`
	Y          float64                `json:"y"`
	Width      float64                `json:"width"`
	Height     float64                `json:"height"`
	Tags       []string               `json:"tags,omitempty"`
	Metadata   map[string]interface{} `json:"metadata,omitempty"`
}

type Edge struct {
	ID    string `json:"id"`
	From  string `json:"from"`
	To    string `json:"to"`
	Type  string `json:"type"` // call, data-flow, security, storage, implements, extends
	Label string `json:"label,omitempty"`
}
