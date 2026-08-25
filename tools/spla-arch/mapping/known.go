package mapping

import (
	"encoding/json"
	"os"
	"sort"
	"time"
)

// Known is a lockfile: every entity the previous run saw, and where it landed.
// Diffing it against the current scan is what turns "regenerate the diagram"
// into "tell me what changed since a human last looked".
type Known struct {
	Mapping   string            `json:"mapping"`
	UpdatedAt string            `json:"updatedAt"`
	Entities  map[string]string `json:"entities"` // type name -> leaf id ("" = unplaced)
}

// LoadKnown reads a lockfile. A missing file is not an error — the first run
// simply has no baseline, and every entity counts as pre-existing.
func LoadKnown(path string) (*Known, bool) {
	raw, err := os.ReadFile(path)
	if err != nil {
		return &Known{Entities: map[string]string{}}, false
	}
	var k Known
	if err := json.Unmarshal(raw, &k); err != nil {
		return &Known{Entities: map[string]string{}}, false
	}
	if k.Entities == nil {
		k.Entities = map[string]string{}
	}
	return &k, true
}

// SaveKnown writes the lockfile with deterministic key order.
func SaveKnown(path, mappingID string, entities map[string]string) error {
	k := Known{
		Mapping:   mappingID,
		UpdatedAt: time.Now().UTC().Format("2006-01-02T15:04:05Z"),
		Entities:  entities,
	}
	raw, err := json.MarshalIndent(k, "", "  ")
	if err != nil {
		return err
	}
	return os.WriteFile(path, append(raw, '\n'), 0644)
}

// Drift is what changed between the lockfile and the current scan.
type Drift struct {
	NewPlaced   []DriftItem // appeared and a rule claimed it — placed, but worth a look
	NewUnplaced []DriftItem // appeared and nothing claimed it — parked on the canvas
	Moved       []DriftItem // a rule change relocated an existing entity
	Removed     []string    // gone from the code
}

type DriftItem struct {
	Name string
	From string
	To   string
}

// Compare diffs the previous lockfile against the current placement.
func Compare(prev *Known, current map[string]string) Drift {
	var d Drift
	for name, leaf := range current {
		before, existed := prev.Entities[name]
		switch {
		case !existed && leaf == "":
			d.NewUnplaced = append(d.NewUnplaced, DriftItem{Name: name})
		case !existed:
			d.NewPlaced = append(d.NewPlaced, DriftItem{Name: name, To: leaf})
		case before != leaf:
			d.Moved = append(d.Moved, DriftItem{Name: name, From: before, To: leaf})
		}
	}
	for name := range prev.Entities {
		if _, still := current[name]; !still {
			d.Removed = append(d.Removed, name)
		}
	}
	sortItems(d.NewPlaced)
	sortItems(d.NewUnplaced)
	sortItems(d.Moved)
	sort.Strings(d.Removed)
	return d
}

// Clean reports whether a run needs human attention.
func (d Drift) Clean() bool {
	return len(d.NewUnplaced) == 0 && len(d.NewPlaced) == 0 && len(d.Moved) == 0
}

func sortItems(items []DriftItem) {
	sort.Slice(items, func(i, j int) bool { return items[i].Name < items[j].Name })
}
