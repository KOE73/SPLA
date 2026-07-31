# skills/

Authored skill procedures that ship with this repository — examples, experiments, and anything not
tied to a single plugin. Plain markdown; see `agents/skills.md` for the file format.

## Layout

Subfolders are free-form grouping. A folder that contains a `SKILL.md` **is** one skill and whatever
sits beside it is that skill's resources; a folder without one is just organisation and the scan
continues into it.

```
skills/
  examples/
    release-notes.md          → id: examples.release-notes (unless the frontmatter says otherwise)
  network/
    host-audit/
      SKILL.md                → id: network.host-audit, with its resources alongside
      references/…
```

A file that declares no `id:` gets one from its path — `network/dns.md` → `network.dns`. Declaring
`id:` explicitly in the frontmatter always wins, and is what the shipped skills do.

## How this folder reaches the agent

It is not magic: the folder is a configured skill source in this project's `spla.spla`.

```yaml
skills:
  sources:
    - type: directory
      path: skills
    - type: directory
      path: .spla/skills
```

`.spla/skills` stays in the list for personal, uncommitted drafts — `.spla/` is local state and is
git-ignored in full. Skills that belong to a *plugin* go in that plugin's own `skills/` subfolder
instead, so they follow its enable switch.
