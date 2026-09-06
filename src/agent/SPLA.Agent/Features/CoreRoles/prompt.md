Roles — who else works on this project.

A project may declare roles: named kinds of worker (architect, reviewer, tester, …), each with its
own mode, tools and character. You address one by name — `agent_spawn` for a one-off assignment,
`agent_correspond` for a conversation that continues. `agent_spawn` refuses a name the project does
not declare, so do not guess names.

A role is a **kind** of worker, not a person. `agent_correspond("architect")` therefore means "give
me an architect" and hands you a new one; the particular architect you already have is called
something like `architect_2` — his **public name** — and `agent_correspond("architect_2")` reaches
him instead of making a third. That number belongs to him, not to your list of contacts: everyone
who talks to that architect calls him `architect_2`, so the name is worth saying out loud to a
third party.

Public names are shown to you — in the reply tool that appears once a correspondence is open
(`reply_architect_2`), and in the delivery receipt. Read one, never build one: a role plus a guessed
number addresses somebody else, or nobody.

`role_list` is the directory. Call it before addressing anyone the first time. It returns each
role's name, its mode and a short description — enough to choose. It deliberately does not return a
role's system prompt: that text also carries what the role is allowed to reach, so it is not handed
out on request. If you truly need to read it, open `roles/<name>.yaml` with the file tools.

When two chats need each other, **introduce them**: `agent_introduce(first, second, text)` opens the
correspondence between those two and leaves you out of it. Name both by their public names, and give
the text you want the **first** one to receive — it is the one that will speak first, and without a
message nothing would start. The second needs nothing from you: the next thing it hears is the
first one's reply.

An introduction is a receipt, not a conversation. You are not on that edge: you will not see what
they say to each other, no answer is coming back to you, and there is nothing to wait for. Say what
they need to know and carry on with your own work. Never relay their messages by hand instead —
retelling turns you into a lossy wire, and neither of them can ask the other a follow-up question.

`agent_introduce` refuses to name **you** as one of the two: that is ordinary correspondence, and
`agent_correspond` is the tool for it. It also refuses two chats that already correspond — they have
each other's address already, so there is nothing left to introduce.
