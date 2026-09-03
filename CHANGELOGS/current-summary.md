# Summary — unreleased

<!-- covers: 2026-09-03 -->

The prose account of the current cycle: what changed and why it matters, organised by theme rather
than by date. Rewritten from scratch before each push — never appended to. On release it is frozen
into `CHANGELOGS/<version>.md` and this file starts empty again.

The `covers:` marker on the first line records the latest `current-log.md` date this text accounts
for. CI compares the two: if the log has moved on, this summary is stale and is left out of the
release rather than published as if it were current.

---

## An actor has a type, and the type is a boundary

Until now an agent was one thing configured one way, and a sub-agent was that same thing handed a
task. This cycle gives an actor a **role**: a settings type that lives in `roles/<name>.yaml` beside
the manifest and travels in git like any other project file. The manifest names the roles it accepts,
so a role file that arrives in a pull request and that nobody named does not act — the same rule, for
the same reason, as refusing to walk up the tree looking for configuration. Existing projects mean
exactly what they meant before: `agent:` is read as the default role.

The point of the type is that it **withholds**, not that it instructs. A named role governs the mode
of the run and actually narrows the tools the run is offered — a role a caller could widen by passing
`mode` would not be a boundary at all, and a reviewer told not to fix things needs the tools taken
away rather than the instruction repeated. An unknown role is refused with the available names listed,
because a silent fallback to the default runs the work with the wrong capabilities and looks like
success.

## A spawned run is a session, not a footnote in memory

A sub-agent used to be an in-memory record that vanished on restart and could only be read once it
had finished. It is now an ordinary session with everything a chat already had — an id, a file on
disk, an inbox, a progress tree, token accounting — which is what makes it watchable mid-run rather
than only reportable afterwards. It stays off the human chat list and is bounded by retention instead:
finished sessions ring out oldest-first, and a run still in progress is never touched, however old.

The same change is what makes the rest of this cycle possible at all. Once a spawned run is a session,
two actors can address each other; once they can address each other, their exchange has a cost worth
accounting for.

## Actors write to each other, and the exchange runs down on its own

`agent_correspond` opens an address to another role. Once open, the chat grows its own
`reply_<role>` tool for that one correspondence — the tool name is the address, not a chat id buried
in an argument list and not a line of prompt asking the model to remember one. Calling it returns a
delivery receipt and never the answer: the correspondent's actual words arrive later, as an ordinary
message on this chat's own turn. A correspondent that goes quiet is announced and its reply tool stops
appearing, rather than the next call looking like a hallucinated tool name.

The obvious failure mode is two actors that never run out of things to say on someone else's token
budget. So the exchange **decays by construction**: the wait before a reply wakes a turn doubles with
how many replies have crossed since a human last spoke, up to a ceiling past which a reply no longer
wakes a turn at all — it queues and rides whatever turn happens for another reason. The conversation
slows to the pace of outside events instead of being cut off mid-sentence. A hard cap exists purely as
an emergency stop that should never fire. The four numbers governing this are chosen, not yet
measured, and are overridable per role.

## Seeing who is talking, and who is being ignored

The chat list became a tree: a spawned session hangs under the chat that gave it its errand, tagged
with its role, instead of being hidden or mixed in among human chats. A sessions panel lists what is
running across the whole tree — role, parent, status, model, tokens. An incoming reply renders as
speech, "from `<role>`", the moment it lands; an outgoing one renders "to `<role>`" with its receipt
hidden. Underneath it is still an ordinary turn on the wire; only the rendering tells them apart.

Correspondences now survive a restart, because they live in the chat's session file rather than in a
running runtime, and each edge accumulates how much text actually crossed it. That measurement is
deliberately of the replies themselves and not of the turns they woke — a turn does many things, and
charging all of it to one edge would be a confident lie. The resulting graph is assembled from disk,
so it does not change depending on which windows happen to be open, and it leads with the imbalance
rather than the picture: the role that only ever talks, and the role nobody answers.

## A chat that is archived stays archived

Opening an archived chat used to bring it back to life in everything but name — the list said
archived while the session ran. Opening one is now refused, and refused *legibly*: "Chat is archived",
not the misleading "Chat not found", so a client can tell a deliberate answer from a lost file.

Refusing to open it left nothing to look at, so the same cycle adds the way back in: `chat.read`
hands over an archived chat's history off disk, creating no runtime and registering no watch, and the
window shows it without a composer and without the status bar — which is entirely settings for a next
turn this chat will not take. That is a separate message type rather than a flag on `chat.open`, and
the reason generalises: `chat.opened` is a promise that the session is watchable and takes a message,
and reusing it here would put the burden of remembering the archived exception on every handler built
on that promise, one at a time, forever.

The same shape covers the other surface nobody may write to. A sub-agent's window shows its log and
its progress tree with no composer, because by the time a watcher decided to intervene the sub-agent
has changed its mind several times already. The difference worth stating is that this one is a rule
and not a house style: while its run is going, a message sent to a spawned session is refused by the
server, so a client that has never heard of sub-agents is refused on the same terms. Once the run
ends the session is an ordinary chat again, and the hidden composer is taste rather than prohibition.
