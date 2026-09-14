---
id: geometry.markup
description: Marks objects on an image as boxes and points by looking at each rendered result and correcting it until the outline sits on the thing, then returns the markup in original-image coordinates. Trigger on: outline something in a picture, locate an object on an image, mark up a photo or scan, measure a region in pixels, get bounding boxes for parts of an image.
---

# Image Markup by Correction

The markup is produced by looking, not by calculating. Each call returns a picture; the next call is
a judgement read off that picture. Nothing here needs coordinate arithmetic — the tools do it.

---

## Memory Keys Used by This Skill

- `context:plan` — (object) the step list and progress cursor for this markup, session scope
- `context:step` — (string) the object currently being placed and what is being corrected about it
- `skill:geometry.markup:results` — (object) the markup returned by `geom_result`, session scope
- `task:summary` — (string) the final report handed back to the user, session scope

---

## Step 0 — Initialize the plan

Write the plan to `context:plan` in session scope:

```json
{
  "total_steps": 6,
  "current_step": 0,
  "image": "<address given by the user>",
  "steps": [
    "Step 1: Open the image and look at it",
    "Step 2: List the objects to mark, largest containers first",
    "Step 3: Place and correct each large object in the current view",
    "Step 4: Zoom into each large object and place what is inside it",
    "Step 5: Accept every object",
    "Step 6: Return the result and summarize"
  ],
  "objects": []
}
```

Update `current_step` after each step, and `context:step` after each correction, so position survives
a context reset.

---

## Step 1 — Open and look

`geom_open {image: "<address>"}`. Read the reply: it states the pixel size of the picture that came
back. Every coordinate in every later call is in the size stated by the most recent reply. Describe
what is in the picture before naming any coordinate — a guess made without looking is the one that
takes the most corrections.

---

## Step 2 — List the objects

Name each thing to be marked, and note which things sit inside which. Store the list in
`context:plan.objects`. Large containers come before their contents, because a container can be
zoomed into and its contents then placed at a usable size.

---

## Step 3 — Place and correct one object

`geom_box {name, cx, cy, width, height}` with a rough guess. A first guess that is visibly off is
normal — it is the input to the next judgement, not a failure.

Then look at the returned picture and correct what is visibly wrong:

- **One side is in the wrong place** — name the colour that side is drawn in and move it:
  `geom_box {name, edge: "green", by: 30}`. Positive is outward. "The text runs past the green side"
  is already the correction; write it as an `edge` call rather than resizing the whole box.
- **Both opposite sides are wrong the same way** — `dw` / `dh`.
- **The box sits in the wrong place** — `dx` / `dy`, judged against the drawn centre mark.
- **The box leans the wrong way** — the sign of the angle is wrong. Flip it. Read the tilt words the
  reply prints next to the number and check them against the picture; a negative angle does not mean
  counter-clockwise here.

After each correction write what was corrected to `context:step`.

**When a correction did not improve the picture, do not repeat it smaller.** Repeating the same
reasoning with a decaying delta grows the box past the edge of the view and never converges. Change
approach instead: move one side with `edge`/`by` rather than the whole size, flip the angle's sign,
or `geom_view` onto the box and work larger.

The object is done when the outline sits on the thing and no side can be named as wrong.

---

## Step 4 — Work zoomed for small objects

`geom_view {to: "<name of the enclosing object>"}` before placing anything small. The reply states
the new picture's size, and coordinates from that point on are in it. Small objects placed in the
flat whole-image view come out unusable; the same object placed after zooming comes out accurate.
`geom_view {to: "source"}` steps back out, `geom_view {to: "parent"}` goes back one level.

Repeat Step 3 for each object inside the zoomed view.

---

## Step 5 — Accept

`geom_accept {name}` per object, or `geom_accept {}` once everything sits right. Accepting only
records that placement is finished; an accepted object can be corrected again by calling `geom_box`
on it.

---

## Step 6 — Return the result

`geom_result` returns every object in the original image's coordinates. Store it in
`skill:geometry.markup:results`, write the report to `task:summary` in session scope, and answer the
user from that summary — object names, what each covers, and the source image size.

---

## Finalize

`agent_memory_clear {scope:"session", filter:"context:"}`. Call `skill_deactivate`.
