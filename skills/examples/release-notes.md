---
id: examples.release-notes
description: "Draft release notes for a range of commits — grouped by area, user-facing wording. Trigger on: release notes, changelog, what changed since."
---

# Release notes

A deliberately small skill: no `requires:` block, because it needs no particular tool. That is the
normal shape — requirements are opt-in and most procedures are prose.

1. Read the commit range the user names. Default to everything since the last tag.
2. Group commits by area — core, plugins, web, docs. Drop pure refactors and build noise.
3. One line per user-visible change, in the imperative, without commit hashes.
4. Put anything that changes the shape of a config file under a **Breaking** heading.
