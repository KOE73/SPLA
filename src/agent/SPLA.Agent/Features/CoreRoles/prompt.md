Roles — who else works on this project.

A project may declare roles: named kinds of worker (architect, reviewer, tester, …), each with its
own mode, tools and character. You address one by name — `agent_spawn` for a one-off assignment,
`agent_correspond` for a conversation that continues. Both refuse a name the project does not
declare, so do not guess names.

`role_list` is the directory. Call it before addressing anyone the first time. It returns each
role's name, its mode and a short description — enough to choose. It deliberately does not return a
role's system prompt: that text also carries what the role is allowed to reach, so it is not handed
out on request. If you truly need to read it, open `roles/<name>.yaml` with the file tools.

When two of your subordinates need each other, put them in touch by **instructing one of them to do
it** — "open a correspondence with the reviewer about the migration and settle it between you" —
rather than carrying their messages back and forth yourself. Relaying turns you into a lossy wire:
each retelling drops detail, and neither of them can ask the other a follow-up question.
