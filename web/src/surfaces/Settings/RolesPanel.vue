<!--
  The role editor. A role is a named actor this project has — the same kind of settings object the
  project's own `agent:` is ("role zero"), which is why the fields here are the Agent panel's fields
  with one difference: every one of them may be left unsaid, and unsaid means "whatever the project
  says". A blank box is an answer.

  Two facts about a role live in two different places, and the panel shows both because hiding
  either one hides a real state:
    - the BODY is `roles/<name>.yaml`, next to the manifest, travelling in git;
    - whether the role ACTS is the manifest's `roles:` list naming it. A body nobody named is inert.
  So a card can exist and be switched off (a file a branch added), and a name can be declared with a
  body that will not load (shown with its fields empty).

  What a role can and cannot do is not a UI choice — the narrowing lists (connections, islands, tool
  sets) are SELECTIONS over what the project already reaches, never grants of anything new, and a
  role carries no permissions block at all: it picks a mode, the way `agent:` does.
-->
<template>
  <div class="s-panel" data-tab="roles">
    <div class="s-head">
      <b>Roles</b>
      <span class="hint">{{ error || hint }}</span>
    </div>

    <p class="expl">
      A role is who a chat can be, spawn (<code>agent_spawn(role:)</code>) or write to
      (<code>agent_correspond</code>). Its body is <code>roles/&lt;name&gt;.yaml</code>; the switch on
      each card is the manifest's <code>roles:</code> list — <b>a role nobody named does not act</b>,
      even with a perfectly good file. Every field left blank inherits from the project's own agent
      settings.
    </p>

    <div v-if="canPersist === false" class="empty">
      No <code>.spla</code> project is open — a role is a file next to the manifest, so there is
      nowhere to put one.
    </div>

    <ListPanel v-else :empty="!roles.length" empty-text="This project has no roles yet."
               add-label="＋ New role" @add="addRole">
      <ListCard v-for="r in roles" :key="r.key" :open="open === r.key" :no-toggle-on-click="true"
                @update:open="toggleOpen(r.key)">
        <template #head>
          <input type="checkbox" :checked="r.active" :title="r.active
                   ? 'Declared in the manifest — this role acts'
                   : 'Body only — the manifest does not name it, so it does not act'"
                 @change="r.active = ($event.target as HTMLInputElement).checked" />
          <b class="r-name" :class="{ off: !r.active }">{{ r.name || "(unnamed)" }}</b>
          <span class="r-badge">{{ r.mode || projectMode + " (project)" }}</span>
          <span v-if="r.modelId" class="r-badge">{{ modelLabel(r.modelId) }}</span>
          <span v-if="narrowings(r)" class="r-badge narrow" :title="narrowings(r) || ''">narrowed</span>
          <span class="r-desc">{{ r.description }}</span>
          <span class="grow"></span>
          <ExpandButton :open="open === r.key" @update:open="toggleOpen(r.key)" />
          <DeleteButton title="Delete this role — its file goes on the next Save" @click="remove(r.key)" />
        </template>

        <template #body>
          <label class="field"><span>Name</span>
            <input v-model="r.name" placeholder="architect" spellcheck="false" />
          </label>
          <p class="sub">
            Also the file name (<code>roles/{{ r.name || "&lt;name&gt;" }}.yaml</code>) and the word an
            agent types into <code>agent_spawn</code>.
          </p>

          <label class="field col"><span>Description</span>
            <input v-model="r.description" placeholder="Reviews changes for correctness and scope." />
          </label>
          <p class="sub">The outward half — one line, shown to any chat choosing whom to task. The prompt below stays in.</p>

          <label class="field"><span>Mode</span>
            <select v-model="r.mode">
              <option value="">inherit — {{ projectMode }} (project)</option>
              <option v-for="m in modes" :key="m" :value="m">{{ m }}</option>
              <!-- A word the file holds that is not a mode any more (renamed, or a typo) keeps its own
                   option: the resolver ignores it and runs the project's mode, and a select that
                   silently showed "inherit" would make an unnoticed save erase the evidence. -->
              <option v-if="r.mode && !modes.includes(r.mode)" :value="r.mode">
                {{ r.mode }} — not a known mode, ignored
              </option>
            </select>
          </label>

          <label class="field"><span>Model</span>
            <select v-model="r.modelId">
              <option value="">inherit — whatever the chat would pick</option>
              <option v-for="m in models" :key="m.id" :value="m.id">{{ m.name || m.id }}</option>
            </select>
          </label>

          <label class="field col"><span>Prompt</span>
            <textarea v-model="r.customPrompt" rows="5"
                      placeholder="Who this role is, written to the model. Replaces the project's custom prompt for this role."></textarea>
          </label>

          <div class="group">
            <div class="group-head">Capabilities</div>
            <p class="sub">
              A role's list <b>replaces</b> the project's rather than narrowing it — a role may have a
              capability <code>agent:</code> never mentioned. What actually runs is still bounded by the
              directory root and the owner's grants, never by this list.
            </p>
            <label class="field"><span>Built-in capabilities</span>
              <select :value="r.capabilities ? 'own' : 'inherit'"
                      @change="setCapMode(r, ($event.target as HTMLSelectElement).value)">
                <option value="inherit">inherit the project's list</option>
                <option value="own">declare this role's own list</option>
              </select>
            </label>
            <div v-if="r.capabilities" class="cap-list">
              <CapabilityRow v-for="c in knownCapabilities" :key="c.id"
                             :item="{ ...c, enabled: r.capabilities.includes(c.id) }"
                             @toggle="on => toggleCap(r, c.id, on)" />
            </div>
          </div>

          <div class="group">
            <div class="group-head">Reach</div>
            <p class="sub">
              Selections over what the project already reaches — narrowing only. Nothing here can make
              something reachable that the project never declared.
            </p>

            <div class="field col"><span>Connections</span>
              <ChipSelect v-model="r.connections" :options="connectionOptions" all-label="every connection" />
            </div>
            <div class="field col"><span>Islands</span>
              <ChipSelect v-if="islands.length" v-model="r.islands"
                          :options="islands.map(i => ({ id: i }))" all-label="every island" />
              <input v-else :value="(r.islands || []).join(', ')" placeholder="every island"
                     spellcheck="false"
                     @input="r.islands = splitList(($event.target as HTMLInputElement).value)" />
            </div>
            <div class="field col"><span>Trusted domains</span>
              <input :value="(r.trustedDomains || []).join(', ')" placeholder="inherit the project's"
                     spellcheck="false"
                     @input="r.trustedDomains = splitList(($event.target as HTMLInputElement).value)" />
            </div>
          </div>

          <div class="group">
            <div class="group-head">Tool sets</div>
            <p class="sub">
              How much of a set reaches the model before it is needed. Merged key by key over the
              project's <code>toolsets:</code> — a set left at "inherit" is not written to the file at all.
            </p>
            <label v-for="id in toolSetIds" :key="id" class="field set"><span class="mono">{{ id }}</span>
              <select :value="r.toolSets?.[id] || ''"
                      @change="setToolSet(r, id, ($event.target as HTMLSelectElement).value)">
                <option value="">inherit</option>
                <option v-for="lv in toolSetLevels" :key="lv" :value="lv">{{ lv }}</option>
              </select>
            </label>
          </div>

          <div class="group">
            <div class="group-head">Behaviour</div>
            <TriStateField v-model="r.loopGuard" label="Tool call loop guard"
                           hint="ask the model if it is stuck, then stop" />
            <InheritNumberField v-model="r.loopGuardRepeats" label="Repeats to trigger" :min="2" :max="20" />
            <InheritNumberField v-model="r.shellTimeoutSeconds" label="Shell command timeout"
                                unit="seconds" hint="0 = wait until the command exits" />
            <InheritNumberField v-model="r.askTimeoutMinutes" label="Question timeout"
                                unit="minutes" hint="0 = wait forever" />
            <TriStateField v-model="r.saveToolCalls" label="Save full tool trace" />
            <TriStateField v-model="r.saveAttempts" label="Save abandoned generations" />
            <TriStateField v-model="r.unifiedResources" label="Resource addresses" />
          </div>

          <div class="group">
            <div class="group-head">Correspondence</div>
            <p class="sub">
              How fast this role may be woken by replies from other actors. The debounce doubles with
              the depth of an exchange (<code>base · 2^depth</code>) up to the ceiling; past the depth
              ceiling a reply no longer raises a turn of its own, it rides the next one.
            </p>
            <InheritNumberField v-model="r.peerDebounceBaseSeconds" label="Reply debounce, base" unit="seconds" />
            <InheritNumberField v-model="r.peerDebounceMaxSeconds" label="Reply debounce, ceiling" unit="seconds" />
            <InheritNumberField v-model="r.peerDepthCeiling" label="Replies that may wake a turn" />
            <InheritNumberField v-model="r.peerHardCap" label="Emergency stop on depth"
                                hint="reaching it is a defect, not a normal outcome" />
          </div>
        </template>
      </ListCard>
    </ListPanel>
  </div>
</template>

<script setup lang="ts">
import { onUnmounted, ref } from "vue";
import { client } from "../../protocol/SplaClient";
import type { CapabilityDto, ConnectionDto, RoleEditDto } from "../../protocol/types";
import ListPanel from "../../components/list/ListPanel.vue";
import ListCard from "../../components/list/ListCard.vue";
import DeleteButton from "../../components/buttons/DeleteButton.vue";
import ExpandButton from "../../components/buttons/ExpandButton.vue";
import ChipSelect from "../../components/fields/ChipSelect.vue";
import TriStateField from "../../components/fields/TriStateField.vue";
import InheritNumberField from "../../components/fields/InheritNumberField.vue";
import CapabilityRow from "./CapabilityRow.vue";

/** A card's identity for the v-for and the open/delete keys. Not the name: the name is editable, and
 *  keying on it would tear the card down mid-rename, losing focus on every keystroke. */
type RoleCard = RoleEditDto & { key: string };

const roles = ref<RoleCard[]>([]);
const modes = ref<string[]>([]);
const projectMode = ref("");
const models = ref<ConnectionDto[]>([]);
const connections = ref<ConnectionDto[]>([]);
const islands = ref<string[]>([]);
const toolSetIds = ref<string[]>([]);
const toolSetLevels = ref<string[]>([]);
const knownCapabilities = ref<CapabilityDto[]>([]);
const canPersist = ref<boolean | undefined>(undefined);
const error = ref("");
const hint = ref("");
const open = ref("");

let seq = 0;
const nextKey = () => `r${++seq}`;

/** Connections offered as narrowing targets: the ids, plus the three whole-layer words. A role that
 *  says "user" means "my own connections, whichever they turn out to be" — a durable answer an id
 *  list cannot express. */
const connectionOptions = ref<{ id: string; label?: string; title?: string }[]>([]);

const off = client.on("roles.result", p => {
  // A refused save answers with what is still on disk. Taking it would throw away the edit that was
  // just refused — including a role typed from scratch — leaving the person with an error message
  // about something they can no longer see. The message lands; the cards stay as they are.
  if (p.error) { error.value = p.error; return; }

  roles.value = (p.roles || []).map(r => ({ ...r, key: nextKey() }));
  modes.value = p.modes || [];
  projectMode.value = p.projectMode || "";
  models.value = p.models || [];
  connections.value = p.connections || [];
  islands.value = p.islands || [];
  toolSetIds.value = p.toolSetIds || [];
  toolSetLevels.value = p.toolSetLevels || [];
  knownCapabilities.value = p.knownCapabilities || [];
  canPersist.value = p.canPersist;
  error.value = p.error || "";
  hint.value = roles.value.length
    ? `${roles.value.filter(r => r.active).length} of ${roles.value.length} named by the manifest`
    : "";
  connectionOptions.value = [
    ...connections.value.map(c => ({ id: c.id, label: c.name || c.id })),
    ...["shared", "user", "project"].map(s => ({
      id: s, label: `all ${s}`, title: `Every connection in the ${s} layer`
    }))
  ];
});
onUnmounted(off);

function toggleOpen(key: string) { open.value = open.value === key ? "" : key; }

function addRole() {
  const card: RoleCard = { key: nextKey(), name: "", active: true };
  roles.value.push(card);
  open.value = card.key;
}

function remove(key: string) {
  roles.value = roles.value.filter(r => r.key !== key);
  if (open.value === key) open.value = "";
}

function modelLabel(id: string) {
  return models.value.find(m => m.id === id)?.name || id;
}

/** The collapsed card's one-word summary of what this role was narrowed to. Narrowing is the half of
 *  a role that its name does not show, so it gets a badge rather than only appearing once open. */
function narrowings(r: RoleCard): string | null {
  const parts: string[] = [];
  if (r.connections?.length) parts.push(`connections: ${r.connections.join(", ")}`);
  if (r.islands?.length) parts.push(`islands: ${r.islands.join(", ")}`);
  if (r.toolSets && Object.keys(r.toolSets).length) parts.push(`tool sets: ${Object.keys(r.toolSets).join(", ")}`);
  if (r.capabilities) parts.push(`own capability list (${r.capabilities.length})`);
  return parts.length ? parts.join("\n") : null;
}

/** Switching to "own list" starts from what the project has — the list this role inherits — so the
 *  switch alone changes nothing, and whatever is unticked afterwards is the actual change. */
function setCapMode(r: RoleCard, mode: string) {
  r.capabilities = mode === "own"
    ? knownCapabilities.value.filter(c => c.enabled).map(c => c.id)
    : null;
}

function toggleCap(r: RoleCard, id: string, on: boolean) {
  const list = r.capabilities || [];
  r.capabilities = on ? [...list, id] : list.filter(x => x !== id);
}

function setToolSet(r: RoleCard, id: string, level: string) {
  const next = { ...(r.toolSets || {}) };
  if (level) next[id] = level; else delete next[id];
  r.toolSets = Object.keys(next).length ? next : null;
}

/** Comma/space separated text → a list, with "nothing typed" staying null (inherit / everything)
 *  rather than becoming an empty list, which would mean "deliberately none". */
function splitList(text: string): string[] | null {
  const parts = text.split(/[,\s]+/).map(x => x.trim()).filter(Boolean);
  return parts.length ? parts : null;
}

function save(): Promise<void> {
  return new Promise((resolve, reject) => {
    const timer = window.setTimeout(() => { offRes(); reject(new Error("save timed out")); }, 8000);
    const offRes = client.on("roles.result", p => {
      clearTimeout(timer); offRes();
      if (p.error) reject(new Error(p.error)); else resolve();
    });
    // `key` is ours and never travels; the rest of the card is the server's own DTO.
    const ok = client.send("roles.save", {
      roles: roles.value.map(({ key, ...dto }) => { void key; return dto; })
    });
    if (!ok) { clearTimeout(timer); offRes(); reject(new Error("socket closed")); }
  });
}

defineExpose({ save });
</script>

<style scoped>
.expl { margin: 4px 0 12px; color: var(--muted); font-size: var(--fs-sm); line-height: 1.5; max-width: 680px; }
.expl code, .sub code { font-family: var(--mono); color: var(--text); }
.empty { color: var(--muted); font-size: var(--fs-sm); padding: 6px 0; }
.grow { flex: 1; }

.r-name { font-size: var(--fs-sm); white-space: nowrap; }
.r-name.off { color: var(--muted); font-weight: 400; }
.r-badge { font-family: var(--mono); font-size: var(--fs-xs); color: var(--muted);
  border: 1px solid var(--border); border-radius: 4px; padding: 0 4px; white-space: nowrap; }
.r-badge.narrow { border-style: dashed; }
.r-desc { font-size: var(--fs-xs); color: var(--muted); overflow: hidden;
  text-overflow: ellipsis; white-space: nowrap; }

.group { margin: 10px 0 4px; padding-top: 8px; border-top: 1px solid var(--border); }
.group-head { font-weight: 600; font-size: var(--fs-sm); margin-bottom: 2px; }
.sub { margin: 0 0 8px; font-size: var(--fs-xs); color: var(--muted); line-height: 1.45; max-width: 680px; }
.cap-list { display: flex; flex-direction: column; gap: var(--gap, 8px); margin-top: 6px; }
/* A tool set's id is the label here, and ids are longer than the 92px .field label column — widen
   the column for these rows rather than truncating an identifier the user has to type elsewhere. */
.group .field.set { grid-template-columns: 160px 1fr; }
.mono { font-family: var(--mono); font-size: var(--fs-xs); }
</style>
