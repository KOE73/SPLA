package layout

import (
	"fmt"
	"math"

	"spla-arch/model"
)

// PrevLayout is the geometry a human already curated: positions the diagram
// editor moved by hand, colours picked there, and prose descriptions no
// generator writes. Frozen at load time and never recomputed.
type PrevLayout struct {
	Zones map[string]model.Zone // by zone id, geometry + style + metadata
	Nodes map[string]model.Node // by node id, geometry + metadata
}

// LoadPrevLayout indexes a previously generated diagram by id. A nil diagram
// (first run, or no prior file) yields an empty layout — everything is then
// "new" and gets a fresh position, same as before this existed.
func LoadPrevLayout(diag *model.Diagram) *PrevLayout {
	p := &PrevLayout{Zones: map[string]model.Zone{}, Nodes: map[string]model.Node{}}
	if diag == nil {
		return p
	}
	for _, z := range diag.Zones {
		p.Zones[z.ID] = z
	}
	for _, n := range diag.Nodes {
		p.Nodes[n.ID] = n
	}
	return p
}

type rect struct{ x, y, w, h float64 }

// ApplyPreserving lays out the forest like ApplyNested, except geometry a
// human already placed is never recomputed.
//
// The rule, per element:
//   - A zone that existed before keeps its X/Y/Width/Height, Style and
//     Metadata (which is where a hand-written description lives) verbatim.
//     Only its Name is refreshed, because that is where the live node count
//     ("Цепочка middleware (9)") lives and staleness there is misleading.
//   - A node that existed before, in the same zone as before, keeps its
//     geometry verbatim.
//   - Anything genuinely new — a node the mapping just started claiming, a
//     node whose zone changed, a whole new zone — gets packed into free
//     space: inside its zone if the zone already existed (growing the
//     zone's height if there is no room), or past the right edge of
//     everything else if the zone itself is new.
//
// Nothing here ever moves an existing rectangle. The return value lists
// what had to be placed fresh or what a zone had to grow by, so the caller
// can tell a human to go look rather than silently trusting an algorithm to
// have picked a sensible spot.
func ApplyPreserving(diag *model.Diagram, roots []*Box, prev *PrevLayout) []string {
	nodeByID := map[string]*model.Node{}
	for i := range diag.Nodes {
		nodeByID[diag.Nodes[i].ID] = &diag.Nodes[i]
	}

	var zones []model.Zone
	var warnings []string

	// Fresh top-level zones go past the right edge of whatever the previous
	// file already had — never into a gap between existing zones, which
	// would risk landing on top of one a human moved off its generated spot.
	freshX := rootGap
	for _, z := range prev.Zones {
		freshX = math.Max(freshX, z.X+z.Width+rootGap)
	}
	freshY := rootGap

	for _, r := range roots {
		if pz, ok := prev.Zones["z_"+r.ID]; ok {
			warnings = append(warnings, placeExisting(r, pz, prev, nodeByID, &zones)...)
			continue
		}
		measure(r)
		place(r, freshX, freshY, 0, &zones, nodeByID)
		warnings = append(warnings, fmt.Sprintf(
			"new top-level zone %q placed at (%.0f, %.0f) — it has no curated position yet, move it by hand",
			r.ID, freshX, freshY))
		freshX += r.w + rootGap
	}

	diag.Zones = zones
	return warnings
}

// placeExisting reuses pz's geometry for box b outright, recurses into every
// child zone that also already existed, and packs anything new — nodes or
// child zones the mapping just started routing here — into space freed up
// below what pz already contains. It grows pz's own height if that space
// runs out, since extending one zone downward, on its own, is the one
// mutation to already-placed geometry that cannot disturb a sibling that
// sits beside it rather than below it.
func placeExisting(
	b *Box, pz model.Zone, prev *PrevLayout, nodeByID map[string]*model.Node, out *[]model.Zone,
) []string {
	var warnings []string

	zoneID := "z_" + b.ID
	zone := pz    // copy: geometry, style and metadata (incl. any hand-written
	zone.Name = b.Name // description) ride along untouched; only the label,
	// which carries the live node count, is worth refreshing on every build.

	// A styleId is a choice someone made in the editor, exactly like a
	// position — never overwrite one. Only a zone that has never had one gets
	// the theme's default, which is how pre-styleId files migrate.
	if zone.StyleID == "" {
		zone.StyleID = styleIDFor(b.Theme)
	}

	var occupied []rect // zone-local, used to find free space for new content

	var freshNodeIDs []string
	for _, id := range b.NodeIDs {
		if pn, ok := prev.Nodes[id]; ok && pn.Zone == zoneID {
			n := nodeByID[id]
			if n == nil {
				continue
			}
			n.Zone, n.X, n.Y, n.Width, n.Height = zoneID, pn.X, pn.Y, pn.Width, pn.Height
			occupied = append(occupied, rect{pn.X - zone.X, pn.Y - zone.Y, pn.Width, pn.Height})
			continue
		}
		freshNodeIDs = append(freshNodeIDs, id)
	}

	var freshChildren []*Box
	for _, c := range b.Children {
		if cpz, ok := prev.Zones["z_"+c.ID]; ok {
			warnings = append(warnings, placeExisting(c, cpz, prev, nodeByID, out)...)
			occupied = append(occupied, rect{cpz.X - zone.X, cpz.Y - zone.Y, cpz.Width, cpz.Height})
			continue
		}
		freshChildren = append(freshChildren, c)
	}

	if len(freshNodeIDs) > 0 || len(freshChildren) > 0 {
		bottom := header
		for _, o := range occupied {
			bottom = math.Max(bottom, o.y+o.h)
		}
		cursorY := zone.Y + bottom + gapY

		if len(freshNodeIDs) > 0 {
			cols := colsFor(len(freshNodeIDs))
			for i, id := range freshNodeIDs {
				n := nodeByID[id]
				if n == nil {
					continue
				}
				r, c := i/cols, i%cols
				n.Zone = zoneID
				n.X = zone.X + pad + float64(c)*(nodeW+gapX)
				n.Y = cursorY + float64(r)*(nodeH+gapY)
				n.Width, n.Height = nodeW, nodeH
			}
			rows := (len(freshNodeIDs) + cols - 1) / cols
			cursorY += float64(rows)*nodeH + float64(rows-1)*gapY + childGap
			warnings = append(warnings, fmt.Sprintf(
				"%d new node(s) appended at the bottom of %q: %s", len(freshNodeIDs), zoneID, freshNodeIDs))
		}

		for _, c := range freshChildren {
			measure(c)
			place(c, zone.X+pad, cursorY, 1, out, nodeByID)
			cursorY += c.h + childGap
			warnings = append(warnings, fmt.Sprintf(
				"new nested zone %q appended inside %q — it has no curated position yet", c.ID, b.ID))
		}

		if needed := cursorY - zone.Y + pad; needed > zone.Height {
			warnings = append(warnings, fmt.Sprintf(
				"zone %q grown from height %.0f to %.0f to fit new content — check it doesn't now overlap a sibling",
				zoneID, zone.Height, needed))
			zone.Height = needed
		}
	}

	*out = append(*out, zone)
	return warnings
}
