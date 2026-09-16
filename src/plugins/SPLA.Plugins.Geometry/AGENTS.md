# SPLA.Plugins.Geometry — rules for this code

Read [`docs/adr/ADR_20260914_plugins_geometry-workspace.md`](../../../docs/adr/ADR_20260914_plugins_geometry-workspace.md)
for what the plugin is, and
[`docs/adr/ADR_20260914-3_plugins_grounded-vocabulary.md`](../../../docs/adr/ADR_20260914-3_plugins_grounded-vocabulary.md)
for how it is allowed to speak.

## The rule everything here follows

**Every number the model has to supply has a visible counterpart in the render, and the reply text
calls it by the same word.** A term the model must remember is a place where its habit can differ
from ours; a term it can see in the picture cannot differ. Two live failures paid for that sentence:
the model inverted the angle sign because school mathematics puts `y` upward, and it described the
box by its edges six times running while the tool only offered a symmetric `dw`.

Before adding any argument, ask what the model will look at to decide its value. If the answer is
"it will remember the convention", the argument is not finished.

## Frozen: the edge palette and the winding order

Corners run in the order fixed by `Obb.Corners` — top-left, top-right, bottom-right, bottom-left of
the *unrotated* box. Edge *i* runs from corner *i* to corner *i+1*, and a corner dot takes the colour
of the edge that **starts** at it.

| edge | side of the box's own axes | colour | hex |
|---|---|---|---|
| 0 | top | cyan | `#00CFFF` |
| 1 | right | magenta | `#FF74E4` |
| 2 | bottom | yellow | `#FFE100` |
| 3 | left | green | `#3FD65C` |

**Do not change the order or the colours.** The model is told to name an edge by its colour in the
`edge` argument of `geom_box`; changing either silently changes the meaning of every call ever made.
A new edge colour is a new vocabulary, and it belongs in a new ADR.

Why these four (ADR_20260914-3 §3.5): the leading subject is a grey-white sack carrying **red** and
blue print, so red is unusable as an edge — it disappears into the content. They are also spread in
**lightness** (yellow brightest, then cyan, then magenta, then green as the darkest), so nothing
rests on colour vision alone — while all four stay light enough to find on a dark photograph. The deep
`#FF2BD6` magenta and `#0E8A26` green this started with failed that on a photograph shot indoors: a
colour you have to hunt for is not a vocabulary. The *names* are frozen; their exact shades are not.

Colours are bound to the **box's own axes**, not the screen's: they turn with the box, so the cyan
edge is the same edge at any angle. That is exactly what side names like `left` could not give.

## The grid is an instrument, and it is adjustable

The grid is **not** debug decoration and not off by default. The evidence: asked "is the green edge
far from the text?" without a grid, the model answered "no, a narrow strip of a few pixels, the box
sits tight". With the grid on it answered "the green side runs from (200, 90) to (170, 260) and the
first digit starts at about x≈340 — a gap of roughly 150–170 px", and proposed the correction itself.
**The model cannot judge a distance by eye, but it reads coordinates off a grid accurately.** Every
earlier "the edges are close to the text" was confabulation.

The view grid, in `GeometrySettings`, all values clamped:

| setting | default | clamp | what it is |
|---|---|---|---|
| `grid` | `true` | — | the view grid, in the view's axes. The `grid` argument of `geom_open`/`geom_view` overrides it for that view; null follows this |
| `grid_step` | `0` | 0, else 4…500 | spacing of the fine lines, in view pixels. `0` derives it from the frame's size instead (see below); a non-zero value pins it |
| `grid_min_step` | `32` | 4…256 | the finest spacing, and the lattice every derived step and every edge ruler is a multiple of |
| `grid_lines` | `24` | 4…100 | how many fine lines the derived step aims to put across the frame's **longer** side |
| `grid_transparency` | `10` | 0…95 | percent: 0 solid, 100 invisible. A token 10 — an instrument you have to hunt for gets guessed past instead of read. The major/minor split then divides what is left |
| `grid_major_every` | `4` | 1…20 | every Nth line drawn stronger and **labelled**; fine lines carry no labels |
| `grid_color` | `#141414` | `#RRGGBB`/`#AARRGGBB`, bad value falls back | near-black: on a grey-white sack a white grid is invisible, and this is clear of the edge palette |
| `edge_rulers` | `true` | — | the labelled scales along the editing box's four edges. The `edge_rulers` argument of `geom_open`/`geom_view` overrides it for that view; null follows this |
| `ruler_transparency` | `10` | 0…95 | percent, same scale as `grid_transparency`, and token for the same reason. The number plates are never faded at all |
| `ruler_labels` | `true` | — | whether the ruler lines carry their numbers. Off gives the picture back clean, and the step is stated in the reply's text anyway — worth trying against a model that cannot read small digits off a blurred photograph |

**The spacing is derived from the frame, and it stands on a lattice.** Two numbers fix it:
`grid_min_step` is the finest spacing anything is allowed to use — the model reads a frame in patches
of roughly that size, so lines closer together than a patch fall inside one token and only cost
legibility — and `grid_lines` says how many fine lines should cross the frame's longer side. The step
is the smallest multiple of the first that keeps the count under the second
(`GeometrySettings.EffectiveGridStep`). What is held constant as the view is cropped and re-cropped is
the **density** of the ruler in the picture the model is looking at, which a fixed spacing cannot do:
a crop of a fingernail and a whole photograph are the same number of view pixels across only by
accident. Pin `grid_step` when you want two renders comparable line for line.

These knobs have a panel of their own in Settings (`web/src/SettingsPanel.vue`, built to
`web/dist/settings.js` and declared as `web_settings_entry` in `meta.yaml`) — the host never imports
it, it is loaded at runtime like every other plugin's. It exists because the loop these numbers live
in is *turn one number, render a real frame, look* — and a loop that costs a text editor and a
restart is a loop nobody runs. Add a field there in the same commit you add it to `GeometrySettings`,
**with its default repeated**: the stored blob is empty until someone saves, so a field the panel
does not list is a field nobody will ever turn, and a default the panel gets wrong is written over
the real one the first time anything on the page is saved. (A key the panel does not know survives a
save — it is read into the form and written back out — so the danger is a wrong default, not a lost
value.)

## The edge rulers measure in the unit the correction is written in

`geom_box` moves a side with `edge` + `by`, and `by` is **pixels from that edge**. So each edge of the
box being edited carries a scale of distance from itself, in view pixels, labelled on every line and
drawn in that edge's own colour: the model reads *"the text sits between the 10 and the 20 from
green"* and calls `{edge:"green", by:-15}` with the number it just read. No conversion, no
subtraction, no coordinates — which is exactly what the proportional box grid it replaced could not
offer, since a cell's size is a function of the box's size and `by` does not take cells.

- **Both directions, told apart by stroke.** Inward into the box is **dashed**, outward past the edge
  is **solid** and runs the full length of its edge, and the tool help says exactly that. Dashed goes
  inside because inside is where the print is — the gaps let the letters through, so the ruler does not
  cost the legibility of the thing being fitted. Outside there is nothing to protect, and a solid line
  is the easier of the two to follow to its end when the overshoot is a long way out. Outward is not decoration: when the print sticks out
  past a side, "how far out" is the same question and nothing else answers it — but a line 32 px inside
  an edge and one 32 px outside it share colour, distance and number, and the only thing left to tell
  them apart is which side of the edge they fall on, which is the judgement by eye this instrument
  exists to remove. Stroke is a property the reader can *name* without measuring.
- **Every line is labelled**, at full `font_size` and in bold, on a plate **centred on the line**. A
  number too small to read is worse than no number: it gets read anyway, wrongly. And a number attached
  to its line by proximity is one more thing to get wrong — sitting on the line is the shortest way to
  say "this line". Switchable off via `ruler_labels`.
- **Its own edge's colour**, so "10 from green" needs no word for "left" and can never be misread
  against the square view grid.
- **Only the editing box**, for the reason the four colours are only there.

**Spacing is computed on the grid's lattice, not configured.** Only on/off and transparency are
settings; step and count come from the box's current size in view pixels and from `grid_min_step`
(`EdgeRuler.InwardScale` / `OutwardScale`, unit-tested). Every step is a multiple of that same finest
step, so a distance read off a ruler lands on a grid line instead of between two:

- *Inward:* **whole inset rectangles**, at most `EdgeRuler.MaxRings` = 2 of them, each inset by
  `grid_min_step` on **all four sides** — one ring at 32, a second at 64, one ring if only one fits and
  none if the box is narrower than that. Two reasons, and both were learnt from a render:
  *(a)* four lines that each stop where their own edge stops are not a ring — each one runs past the
  box at one end and falls short at the other, so its two ends say different things about where the box
  is, which is the very ambiguity the rulers remove;
  *(b)* one inset for every side means "32 in from each side" is **one** fact the reader holds, where a
  per-side step is four facts that each have to be looked up first — by reading a small digit off a
  blurred photograph, the one thing that cannot be relied on here. Past two rings there is nothing left
  to count that the numbers did not already say, and every extra ring covers more of the print being
  fitted. Two is the minimum that still gives a *direction*: "between the first and the second".
- *Outward:* `1, 2, 4, 8` × `grid_min_step`, capped at a quarter of the view's shorter side so it
  never runs to the frame, each line the full length of its own edge. The two directions get different progressions because they do different
  jobs: inward you check a tight fit, outward you measure an overshoot that may be large.

**Every ruler line runs the edge's full span** — inward as well as outward — and pays for the ink it
spends by being faint. Short marks at the ends of an edge were tried and abandoned: a mark measures
only what it runs beside, the thing being fitted sits in the **middle** of the box, and a reader cannot
carry a distance across a gap by eye, which is the whole reason the number is on the picture instead
of in the reply. Density is still the risk — four edges × two directions × several labelled lines land
on a picture that already carries the view grid, the coloured edges, the corner dots and the centre —
but it is answered by `ruler_transparency`, not by shortening the lines. If you change any of these
numbers, render a real frame at several box sizes — including one deliberately narrow — and **look**.

**Every line both measures and obscures.** On blurred small print a dense grid costs more legibility
than it returns — at a 20 px spacing the small blue print on a sack is visibly degraded. That is what
`grid_min_step`, the major/minor split and `grid_transparency` are all for: keep the lines below a
patch out of the picture, keep the fine ones genuinely faint, keep labels on the frame's borders
rather than over the marked object. This trade-off is not settleable by reasoning; the numbers above
are starting values, and they are knobs because the owner turns them against a live model.

## Frozen: what gets colours, and the two glyphs

- Only the **editing** box gets the four colours and the corner dots. `accepted` objects are drawn in
  one muted slate `#7A8CA0`, thinner, with no dots — five accepted objects would otherwise mean
  twenty coloured edges (ADR §3.4; this overrides `PLAN_20260914` §2.1, where colour carried status).
- A box's **centre** is a filled white dot inside a thin white ring, over a dark halo. It is drawn
  because `dx`/`dy` move exactly it.
- A **point object** is a white/muted **X** inside a circle.

Those last two must stay visually distinct. Two meanings on one glyph is the same disease as two
meanings on one word, and it is why the point's cross was turned from `+` to `X` when the centre
arrived. If you touch either, render both on one frame and look.

## The angle convention is not up for revision

`Affine.RotateDeg`, `Obb.Corners` and the `corners` published by `geom_result` agree: a positive
angle turns clockwise on screen, because image `y` grows downward. The live failure was in how it was
*told*, not in the convention, and the fix was to name the tilt in words beside every angle printed.
If you believe the convention is wrong, stop and ask — tests pin it and the published corners depend
on it.

## Descriptions make promises

`geom_box` used to say "two or three corrections are normal". A live run made six. A number in a
description is a promise about the model's own behaviour; when a live run refutes it, the description
is what changes. Prefer telling the model **how to recognise it is done** and **when to switch
tactics** over telling it how many steps it should need.
