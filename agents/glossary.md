# Russian Terminology for SPLA Documents

The repository writes its code in English and its documents in Russian. That seam is where terms
get damaged: an English name that is a **term of art** gets rendered into Russian by dictionary,
the idiom does not survive the trip, and the Russian text ends up naming a mechanism from a
different industry.

This file is the record of those decisions, so the same word is not re-litigated every few months.

**Concrete cost of not having it:** `ChatPump` — a good name, because *message pump* is an English
idiom that carries "pulls from the queue and pushes on" — was rendered as «насос ходов». The word
survived in an accepted ADR, three plans, a diagram model and the memory for six days before the
owner said it caused rejection on sight. Twenty-six occurrences, four files, one sweep.

## Scope

Applies to **every Russian text produced in this repository**: `docs/adr/`, `docs/plans/`,
`docs/ideas/`, `readme_*_ru.md`, the diagram text catalogues
(`docs/diagrams/projects/*/text.ru.json`), UI strings, prompts, and the conversation with the owner.

Does **not** apply to identifiers, commit messages, branch names or PR titles — those are English,
and deliberately keep the English idiom (see the Git section of the root `AGENTS.md`). `ChatPump`
stays `ChatPump`.

## The rule that generates every row below

**Translate the role, not the metaphor.**

An English technical name is often an idiom: the picture it draws is dead, only the role is read.
Translating the picture imports a live metaphor from another domain, and the reader trips over a
word that was invisible in the original. So ask what the thing *decides* or *does*, and name that.

Two checks, both cheap:

- **Read it aloud.** If the reaction is "странно как-то" — the word came from a dictionary, not from
  the meaning.
- **Say the role in one clause without the word.** If that clause is clearer than the word, use the
  clause's noun.

A term earns a row here only when the naive answer is actually **wrong** — a word was written and
rejected, or two mechanisms started competing for one Russian word. This is not a dictionary: a
glossary padded with obvious words stops being read, and then it stops being true.

## Table

| English (code / concept) | In Russian prose | Never | Why |
|---|---|---|---|
| pump (`ChatPump`, turn start) | **диспетчер ходов** | насос | *message pump* is an English idiom; the translation keeps only the hydraulics. The thing decides *when the next turn starts* — that is a dispatcher. |
| pump (stdout/stderr readers) | **читатель** | насос | Same English word, a different mechanism. Calling both «диспетчер» would invent a link between the turn loop and a stream reader that does not exist. |
| dispatch (a tool call) | **вызов**, «точка входа вызова» | диспетчеризация; **«диспетчер»** | The noun is spent on the turn loop. One Russian word must not name two mechanisms — that is exactly how a reader is made to guess. |
| inbox (`ChatInbox`) | **ящик** | входящие, почтовый ящик | It holds every item addressed to the chat — a human reply, a background result, later a timer — not mail. |
| turn | **ход** | шаг, поворот, итерация | Established: ~145 occurrences across ADRs and plans. «Шаг» is already used for a stage *inside* a turn. |
| middleware | **middleware** (цепочка middleware) | промежуточный слой, посредник | Borrowed as is, established. Where the role needs spelling out, say what it wraps, not what it is called. |
| pipeline | **конвейер** | трубопровод, поток | Established in ~19 documents. |
| gateway (`ILlmGateway`) | **шлюз** | ворота, шлагбаум | Keep «ворота» free — see `gate`. |
| gate (`ICapabilityGate`) | **допуск**, «точка допуска» | шлюз, ворота | «Шлюз» is taken by `gateway`, and the thing does not transport anything: it answers "можно ли". |
| capability | **возможность (хоста)** | способность, компетенция | It is a thing the host *can do* and hands out, not a property of a subject. |
| guard (`RepetitionGuard…`) | **защита (от вырождения)** | охранник, страж, стражник | It cancels and retries; it does not stand at a door. |
| zone / island | **зона / остров** | район, участок | Established: the zone is the area of uniform trust, the island is how it is spoken about on a diagram. |
| lease | **аренда** | лизинг, аренда прав | Established for entering a project: taken, held, released — never owned. |
| stale | **протух** | устарел, неактуален | «Устарел» is passive and quiet; «протух» carries the point that the value must now be redone. |
| provenance | **происхождение** | родословная, источник | «Источник» is the `from` field of a translation — a different thing in the same format. |
| blob | **блоб** | капля, двоичный объект | Established, transliterated. |
| runtime (the component) | **рантайм** | время выполнения | «Время выполнения» is the *moment*; the component is a thing. Both meanings occur here, so they get different words. |
| sandbox | **песочница** | изолятор, карантин | Established. |
| hook | **хук** | крючок, зацепка | Established, transliterated. |
| bus (`ServiceEvents`) | **шина** | автобус | Established. |
| spawn (`agent_spawn`) | **породить**, «порождённый подагент» | заспавнить, нерест | Verb by role; the transliterated slang does not belong in a document. |
| drain (`DrainInbox`) | **разобрать ящик**, «опустошить» | дренаж, слив | The point is that the items are *taken and handled*, not that liquid leaves. |
| poll | **опрос** | поллинг, опрашивание | Established Russian, no reason to borrow. |

## Adding a row

1. **Only after a collision.** Either a translation was written and rejected, or two mechanisms
   reached for one Russian word. A row nobody argued about is noise.
2. **Write the "Why" clause.** Without it the next person re-opens the question and answers it
   differently — which is how the table becomes a second source of confusion.
3. **Sweep in the same commit.** `grep -rn` the rejected word across `docs/`, `agents/`,
   `docs/diagrams/projects/*/text.ru.json` and fix every hit. A glossary that does not match the
   repository documents an intention, not a convention.
   `ADR_` files are the one place where editing is normally forbidden — a terminology sweep is the
   exception, because it changes the word, never the decision.
4. **Do not rename the identifier** to match the Russian choice. The English name is chosen by the
   English idiom and is usually right precisely because of it.
