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
| 1 | right | magenta | `#FF2BD6` |
| 2 | bottom | yellow | `#FFE100` |
| 3 | left | green | `#0E8A26` |

**Do not change the order or the colours.** The model is told to name an edge by its colour in the
`edge` argument of `geom_box`; changing either silently changes the meaning of every call ever made.
A new edge colour is a new vocabulary, and it belongs in a new ADR.

Why these four (ADR_20260914-3 §3.5): the leading subject is a grey-white sack carrying **red** and
blue print, so red is unusable as an edge — it disappears into the content. They are also spread in
**lightness** (yellow brightest, then cyan, then magenta, then a dark green), so nothing rests on
colour vision alone.

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
| `grid_step` | `50` | 10…500 | spacing of the fine lines, in view pixels |
| `grid_major_every` | `4` | 1…20 | every Nth line drawn stronger and **labelled**; fine lines carry no labels |
| `grid_color` | `#141414` | `#RRGGBB`/`#AARRGGBB`, bad value falls back | near-black: on a grey-white sack a white grid is invisible, and this is clear of the edge palette |

**Every line both measures and obscures.** On blurred small print a dense grid costs more legibility
than it returns — at `grid_step=20` the small blue print on a sack is visibly degraded. That is what
the major/minor split is for: keep the fine lines genuinely faint, keep labels on the frame's borders
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
