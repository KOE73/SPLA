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
// This is the from-scratch layout: every element gets a fresh position, as
// if nothing existed before. It is what a first run uses, and what
// ApplyPreserving (preserve.go) falls back to for a genuinely new subtree.
// It is NOT what a rebuild of an existing diagram uses — see
// ApplyPreserving, which keeps a human's placement for everything that
// already had one and only calls into this file for what is actually new.
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

// Themes are the zone theme names a mapping may declare. The colours behind
// them are no longer here: they live in docs/diagrams/styles.json under the
// id "zone.<theme>" and are edited in the diagram editor. This set exists
// only to catch a theme name that does not exist, so a typo falls back to
// "slate" instead of producing a zone the editor cannot style.
var Themes = map[string]bool{
	"green": true, "blue": true, "fuchsia": true, "red": true,
	"yellow": true, "slate": true, "violet": true, "amber": true,
	"sky": true, "cyan": true, "lime": true, "pink": true, "gray": true,
	// Reserved for the parking lot — deliberately loud.
	"unplaced": true,
}

// styleIDFor names the style library entry for a zone's theme. Depth plays no
// part: nesting is already legible from the geometry, and one id per theme
// keeps the library small enough to edit by hand.
func styleIDFor(theme string) string {
	if !Themes[theme] {
		theme = "slate"
	}
	return "zone." + theme
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
		StyleID:  styleIDFor(b.Theme),
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
