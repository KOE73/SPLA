package layout

import (
	"math"
	"spla-arch/model"
)

const (
	DefaultNodeWidth  = 180.0
	DefaultNodeHeight = 52.0
	PadX              = 20.0
	PadY              = 18.0
	HeaderOffset      = 46.0
	DefaultCols       = 3
)

// ApplyGrid positions nodes cleanly inside their respective zones
func ApplyGrid(diag *model.Diagram) {
	nodesByZone := make(map[string][]*model.Node)
	for i := range diag.Nodes {
		n := &diag.Nodes[i]
		nodesByZone[n.Zone] = append(nodesByZone[n.Zone], n)
	}

	for i := range diag.Zones {
		z := &diag.Zones[i]
		zNodes := nodesByZone[z.ID]
		if len(zNodes) == 0 {
			continue
		}

		cols := DefaultCols
		if len(zNodes) <= 2 {
			cols = 1
		} else if len(zNodes) <= 6 {
			cols = 2
		} else if len(zNodes) > 15 {
			cols = 4
		}

		rows := int(math.Ceil(float64(len(zNodes)) / float64(cols)))

		// Calculate needed zone dimensions
		neededW := float64(cols)*(DefaultNodeWidth+PadX) + PadX
		neededH := float64(rows)*(DefaultNodeHeight+PadY) + HeaderOffset + PadY

		if z.Width < neededW {
			z.Width = neededW
		}
		if z.Height < neededH {
			z.Height = neededH
		}

		for idx, n := range zNodes {
			r := float64(idx / cols)
			c := float64(idx % cols)

			w := n.Width
			if w < 100 {
				w = DefaultNodeWidth
			}
			h := n.Height
			if h < 30 {
				h = DefaultNodeHeight
			}

			n.Width = w
			n.Height = h
			n.X = z.X + PadX + c*(w+PadX)
			n.Y = z.Y + HeaderOffset + r*(h+PadY)
		}
	}
}
