# Surveillance task

Detect mining equipment in ONE camera frame. Reply with a single raw JSON object —
nothing else: no thinking, no explanations, no markdown fences.

Schema:

{"equipment": [{"type": "...", "unit_number": "... or null", "load": "... or null"}], "action": "..."}

- type: haul_truck | excavator | train (rail wagons / состав) | dozer | front_loader
- unit_number: only if clearly readable, else null
- load: for haul_truck only, judged by the visible body: empty | partial | full;
  body not visible or not a haul_truck → null
- action — ONE value for the whole scene, first that clearly matches, else "none":
  truck_unloading | excavator_loading_truck | excavator_loading_train | none
  (do not guess motion from a still frame — when unsure, "none")
- no machinery → "equipment": [], "action": "none"
- never invent what is not visible
