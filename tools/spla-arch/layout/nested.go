package layout

import (
	"math"
	"sort"

	"spla-arch/model"
)

// Nested lays out the "semantic atlas" variant: a tree of containers of
// unlimited depth. Every block is measured bottom-up (its own node grid plus
// its shelf-packed children), then placed top-down. Containers never overlap
// and every node lies fully inside its own container.
//
// Coordinates here are derived, not authored. The durable artifact is the
// mapping file; this function is free to move everything on every run.
const (
	nodeW    = 170.0
	nodeH    = 50.0
	gapX     = 14.0
	gapY     = 14.0
	pad      = 14.0
	header   = 30.0
	childGap = 16.0
	rootGap  = 36.0
	rootMaxW = 2600.0
)

// Box is one container in the tree to lay out: a block, its own nodes, and
// its children. Depth is unlimited.
type Box struct {
	ID       string
	Name     string
	Theme    string
	Type     string // "boundary" for depth 0, "component" deeper
	NodeIDs  []string
	Children []*Box

	// measured
	w, h       float64
	gridW      float64
	gridH      float64
	gridCols   int
	childRows  [][]*Box
	childRowsH []float64
}

// Palette maps a theme name to zone colours.
var Palette = map[string]model.ZoneStyle{
	"green":   {Fill: "#f0fdf4", Stroke: "#86efac", StrokeWidth: 2, HeaderBg: "#dcfce7"},
	"blue":    {Fill: "#eff6ff", Stroke: "#93c5fd", StrokeWidth: 2, HeaderBg: "#dbeafe"},
	"fuchsia": {Fill: "#fdf4ff", Stroke: "#f0abfc", StrokeWidth: 2, HeaderBg: "#fae8ff"},
	"red":     {Fill: "#fff1f2", Stroke: "#fca5a5", StrokeWidth: 2, HeaderBg: "#ffe4e6"},
	"yellow":  {Fill: "#fefce8", Stroke: "#fde047", StrokeWidth: 2, HeaderBg: "#fef9c3"},
	"slate":   {Fill: "#f8fafc", Stroke: "#cbd5e1", StrokeWidth: 2, HeaderBg: "#e2e8f0"},
	"violet":  {Fill: "#f5f3ff", Stroke: "#c4b5fd", StrokeWidth: 2, HeaderBg: "#ede9fe"},
	"amber":   {Fill: "#fffbeb", Stroke: "#fde68a", StrokeWidth: 2, HeaderBg: "#fef3c7"},
	"sky":     {Fill: "#f0f9ff", Stroke: "#7dd3fc", StrokeWidth: 2, HeaderBg: "#e0f2fe"},
	"cyan":    {Fill: "#ecfeff", Stroke: "#67e8f9", StrokeWidth: 2, HeaderBg: "#cffafe"},
	"lime":    {Fill: "#f7fee7", Stroke: "#bef264", StrokeWidth: 2, HeaderBg: "#ecfccb"},
	"pink":    {Fill: "#fdf2f8", Stroke: "#f9a8d4", StrokeWidth: 2, HeaderBg: "#fce7f3"},
	"gray":    {Fill: "#f1f5f9", Stroke: "#94a3b8", StrokeWidth: 2, HeaderBg: "#e2e8f0"},
	// Reserved for the parking lot — deliberately loud.
	"unplaced": {Fill: "#fff7ed", Stroke: "#fb923c", StrokeWidth: 3, StrokeDasharray: "8 4", HeaderBg: "#ffedd5"},
}

func styleFor(theme string, depth int) model.ZoneStyle {
	s, ok := Palette[theme]
	if !ok {
		s = Palette["slate"]
	}
	if depth > 0 {
		// nested containers read as sub-divisions, not as separate regions
		s.StrokeWidth = math.Max(1, s.StrokeWidth-0.5*float64(depth))
	}
	return s
}

func colsFor(n int) int {
	switch {
	case n <= 0:
		return 1
	case n <= 3:
		return n
	case n <= 8:
		return 3
	case n <= 16:
		return 4
	default:
		return 5
	}
}

// measure computes the size of a box bottom-up.
func measure(b *Box) {
	if n := len(b.NodeIDs); n > 0 {
		b.gridCols = colsFor(n)
		rows := (n + b.gridCols - 1) / b.gridCols
		b.gridW = float64(b.gridCols)*nodeW + float64(b.gridCols-1)*gapX
		b.gridH = float64(rows)*nodeH + float64(rows-1)*gapY
	}

	for _, c := range b.Children {
		measure(c)
	}

	innerW, innerH := b.gridW, b.gridH

	if len(b.Children) > 0 {
		// Shelf-pack children into rows, aiming for a readable aspect ratio
		// rather than one very tall or very wide column.
		total := 0.0
		widest := 0.0
		for _, c := range b.Children {
			total += c.w * c.h
			widest = math.Max(widest, c.w)
		}
		budget := math.Max(widest, math.Sqrt(total*1.9))
		budget = math.Max(budget, b.gridW)

		var row []*Box
		rowW, rowH := 0.0, 0.0
		flush := func() {
			if len(row) == 0 {
				return
			}
			b.childRows = append(b.childRows, row)
			b.childRowsH = append(b.childRowsH, rowH)
			innerW = math.Max(innerW, rowW)
			row, rowW, rowH = nil, 0, 0
		}
		for _, c := range b.Children {
			add := c.w
			if len(row) > 0 {
				add += childGap
			}
			if len(row) > 0 && rowW+add > budget {
				flush()
				add = c.w
			}
			row = append(row, c)
			rowW += add
			rowH = math.Max(rowH, c.h)
		}
		flush()

		for i, rh := range b.childRowsH {
			innerH += rh
			if i > 0 || b.gridH > 0 {
				innerH += childGap
			}
		}
	}

	b.w = innerW + 2*pad
	b.h = header + innerH + pad
}

// place assigns absolute coordinates top-down.
func place(b *Box, x, y float64, depth int, out *[]model.Zone, nodeByID map[string]*model.Node) {
	zType := b.Type
	if zType == "" {
		if depth == 0 {
			zType = "boundary"
		} else {
			zType = "component"
		}
	}
	*out = append(*out, model.Zone{
		ID:         "z_" + b.ID,
		Name:       b.Name,
		Type:       zType,
		SemanticID: "block." + b.ID,
		X:          x, Y: y, Width: b.w, Height: b.h,
		Style:    styleFor(b.Theme, depth),
		Metadata: map[string]interface{}{"depth": depth},
	})

	cursorY := y + header

	if len(b.NodeIDs) > 0 {
		for i, id := range b.NodeIDs {
			n := nodeByID[id]
			if n == nil {
				continue
			}
			r, c := i/b.gridCols, i%b.gridCols
			n.Zone = "z_" + b.ID
			n.X = x + pad + float64(c)*(nodeW+gapX)
			n.Y = cursorY + float64(r)*(nodeH+gapY)
			n.Width, n.Height = nodeW, nodeH
		}
		cursorY += b.gridH
		if len(b.Children) > 0 {
			cursorY += childGap
		}
	}

	for ri, row := range b.childRows {
		cursorX := x + pad
		for _, c := range row {
			place(c, cursorX, cursorY, depth+1, out, nodeByID)
			cursorX += c.w + childGap
		}
		cursorY += b.childRowsH[ri]
		if ri < len(b.childRows)-1 {
			cursorY += childGap
		}
	}
}

// ApplyNested measures and places the whole forest, then replaces diag.Zones.
func ApplyNested(diag *model.Diagram, roots []*Box) {
	nodeByID := map[string]*model.Node{}
	for i := range diag.Nodes {
		nodeByID[diag.Nodes[i].ID] = &diag.Nodes[i]
	}

	for _, r := range roots {
		measure(r)
	}

	var zones []model.Zone
	cursorX, cursorY, rowMaxH := rootGap, rootGap, 0.0
	for _, r := range roots {
		if cursorX+r.w > rootMaxW && cursorX > rootGap {
			cursorX = rootGap
			cursorY += rowMaxH + rootGap
			rowMaxH = 0
		}
		place(r, cursorX, cursorY, 0, &zones, nodeByID)
		cursorX += r.w + rootGap
		rowMaxH = math.Max(rowMaxH, r.h)
	}

	diag.Zones = zones
}

// SortNodeIDs orders a container's nodes alphabetically by label — inside a
// container, membership carries the meaning, not sequence.
func SortNodeIDs(ids []string, label map[string]string) {
	sort.Slice(ids, func(i, j int) bool { return label[ids[i]] < label[ids[j]] })
}
