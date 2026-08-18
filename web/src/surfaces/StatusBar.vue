<!--
  The CHAT's status line. Everything here is a property of the open chat and dies with it; connection,
  project, user, connection health totals, usage and settings live in ProjectBar (sidebar footer).

  The one deliberate crossover is the health dot next to the model picker: health is a property of the
  connection, but the model is chosen PER CHAT, so the warning has to sit where that choice is made.
-->
<template>
  <!-- Invisible when there is nothing to say. Raised, it is a mark you cannot miss rather than a
       tidy dot: the whole value of the thing is being noticed the one time it matters. -->
  <button v-if="doubt?.raised" class="doubt" :title="doubtTitle" @click="clearDoubt">
    <span class="doubt-mark">●</span> untrusted content
  </button>
  <label>mode
    <select v-model="mode" :disabled="!session" @change="onModeChange">
      <option v-for="m in modes" :key="m" :value="m">{{ m }}</option>
    </select>
  </label>
  <label>model
    <select v-model="modelId" :disabled="!session" @change="onModelChange">
      <optgroup v-for="g in groups" :key="g.connectionId" :label="connEmoji(g.connectionId) + g.connectionName">
        <option v-for="m in g.models" :key="m.id" :value="m.id">{{ modelLabel(m) }}</option>
      </optgroup>
    </select>
    <span id="connHealthDot" class="conn-status" :class="healthClass" :title="healthTitle"></span>
    <button
      ref="infoBtn"
      class="pi-btn"
      :class="alertClass"
      :disabled="!modelId"
      title="Provider and model details"
      @click.stop="toggleInfo"
    >i</button>
  </label>
  <label>temp
    <input
      class="temp"
      type="number"
      min="0"
      max="2"
      step="0.05"
      :value="temperature"
      :disabled="!session"
      title="Sampling temperature for this chat. Applies to the next turn."
      @change="onTempChange"
    />
  </label>
  <!-- The reasoning lever is drawn from what the PROVIDER says this model takes, never from a fixed
       list: the words differ per model (Qwen3.8 has "xhigh" and no "high"). Three states, and the
       middle one is the point — offered when the model was described as having a channel, DISABLED
       (not hidden) when nobody described the model at all, and gone only when a provider positively
       said this model does not reason. "We were not told" is information the person needs. -->
  <label v-if="!reasoningCaps || !reasoningCaps.known || reasoningCaps.supported">think
    <select
      v-model="reasoning"
      class="reasoning"
      :disabled="!session || !reasoningKnown"
      :title="reasoningTitle"
      @change="onReasoningChange"
    >
      <option v-for="o in reasoningOptions" :key="o.value" :value="o.value">{{ o.label }}</option>
    </select>
    <input
      v-if="isBudget"
      class="temp budget"
      type="number"
      min="1"
      step="256"
      :value="budgetTokens"
      title="Thinking budget in tokens"
      @change="onBudgetChange"
    />
  </label>
  <span v-if="activeSkill" id="activeSkill" :title="`Skill '${activeSkill}' is running in this chat`">
    <span class="skill-label">skill: {{ activeSkill }}</span>
    <button class="skill-unload" title="End this skill" @click="unloadSkill">✕</button>
  </span>
  <!-- Hand a skill to this chat yourself. Offered only when none is running, because a second
       activation is refused anyway — and hidden without a chat, since there is nothing to hand it to. -->
  <button
    v-else-if="session"
    ref="skillBtn"
    class="filter"
    title="Give this chat a skill — costs no catalog in the prompt"
    @click.stop="skillPickerOpen = !skillPickerOpen"
  >+ skill</button>
  <span
    v-if="ctxUsed != null"
    id="ctxBudget"
    :class="{ warn: ctxPercent != null && ctxPercent >= 80, crit: ctxPercent != null && ctxPercent >= 95 }"
    :title="ctxTitle"
  >
    <span v-if="ctxPercent != null" class="ctx-bar"><span class="ctx-bar-fill" :style="{ width: ctxPercent + '%' }"></span></span>
    <span class="ctx-label">{{ ctxLabel }}</span>
  </span>

  <span v-if="toolSets.length" id="toolSets">
    <button
      v-for="t in toolSets"
      :key="t.setId"
      class="toolset"
      :class="{ disclosed: t.disclosed, lowerable: t.by === 'agent' || t.by === 'user' }"
      :title="toolSetTitle(t)"
      @click="lowerToolSet(t)"
    >{{ t.setId }}</button>
  </span>

  <ProviderInfoPopup
    v-if="infoOpen && infoBtn && modelId"
    :model-id="modelId"
    :anchor="infoBtn"
    @close="infoOpen = false"
  />
  <SkillPickerPopup
    v-if="skillPickerOpen && skillBtn"
    :anchor="skillBtn"
    @close="skillPickerOpen = false"
  />
</template>

<script setup lang="ts">
import { computed, onUnmounted, ref, watch } from "vue";
import { client } from "../protocol/SplaClient";
import { formatCompact } from "../util/format";
import { useChat } from "../state/chatContext";
import type { ConnHealth, ModelPickDto, ReasoningCapabilityDto, ToolSetState } from "../protocol/types";
import ProviderInfoPopup from "./ProviderInfoPopup.vue";
import SkillPickerPopup from "./SkillPickerPopup.vue";

const chat = useChat();

// Everything chat-scoped is READ from this chat's session — the session is where the demultiplexer
// puts it, so no event handler in this file can be aimed at the wrong chat any more.
const session = computed(() => chat.session.value);
const doubt = computed(() => session.value?.doubt ?? null);
const activeSkill = computed(() => session.value?.activeSkill ?? null);
const toolSets = computed<ToolSetState[]>(() => session.value?.toolSets ?? []);
const lastPrompt = computed(() => session.value?.lastPrompt ?? null);
const lastCompletion = computed(() => session.value?.lastCompletion ?? null);
const ctxUsed = computed(() => session.value?.ctxUsed ?? null);
const ctxWindow = computed(() => session.value?.ctxWindow ?? null);

// The two selects write through to the session so the pickers show this chat's own choice the moment
// it is switched to, without waiting for the server's echo.
const mode = computed({
  get: () => session.value?.mode ?? "",
  set: v => { if (session.value) session.value.mode = v; }
});
const modelId = computed({
  get: () => session.value?.modelId ?? "",
  set: v => { if (session.value) session.value.modelId = v; }
});

const temperature = computed(() => session.value?.temperature ?? null);
const reasoning = computed({
  get: () => session.value?.reasoning ?? "",
  set: v => { if (session.value) session.value.reasoning = v; }
});

// ── The reasoning lever ──────────────────────────────────────────────────────
// Asked for per chat, because the answer belongs to the model AND the connection it runs on. Null
// while unanswered; an answer with known=false means nobody described the model, and the control
// then says so instead of offering words that may corrupt the next turn.
const reasoningCaps = ref<ReasoningCapabilityDto | null>(null);
const reasoningKnown = computed(() => !!reasoningCaps.value?.known && !!reasoningCaps.value?.supported);

/** The words this model actually takes, in the provider's own order, plus the two we always mean:
 *  "leave it alone" and, where it is on offer, "off". */
const reasoningOptions = computed(() => {
  const caps = reasoningCaps.value;
  const out = [{ value: "", label: caps && caps.known ? "default" : "—" }];
  if (!caps || !caps.known || !caps.supported) return out;

  if (!caps.mandatory) out.push({ value: "off", label: "off" });
  out.push({ value: "on", label: "on" });
  for (const e of caps.efforts) out.push({ value: e, label: e });
  // The budget option carries the CURRENT budget as its value once one is set — otherwise the select
  // would find no option matching "budget:4096" and silently fall back to showing "default" while
  // the chat is in fact on a budget.
  if (caps.supportsTokenBudget)
    out.push({ value: isBudgetValue(reasoning.value) ? reasoning.value : BUDGET, label: "budget…" });

  // A saved choice this model has never heard of (the chat moved to another model) still has to be
  // visible — silently showing "default" would misreport what the next turn will send.
  const cur = reasoning.value;
  if (cur && !isBudgetValue(cur) && !out.some(o => o.value === cur)) out.push({ value: cur, label: `${cur} (?)` });
  return out;
});

const BUDGET = "budget:";
const isBudgetValue = (v: string) => v.startsWith(BUDGET);
const isBudget = computed(() => isBudgetValue(reasoning.value));
const budgetTokens = computed(() => {
  const n = parseInt(reasoning.value.slice(BUDGET.length), 10);
  return Number.isFinite(n) && n > 0 ? n : 4096;
});

const reasoningTitle = computed(() => {
  const caps = reasoningCaps.value;
  if (!caps) return "Asking the provider what this model takes…";
  if (!caps.known)
    return "This provider describes no reasoning options for the model, and a lever nobody advertised "
      + "is not safe to pull — an endpoint that accepts a field it does not implement can answer with "
      + "garbage.\nDeclare reasoning_options on the model entry in .spla if you know what your server takes.";
  if (!caps.supported) return "This model has no reasoning channel.";

  const parts = [`Options come from the provider: ${caps.efforts.length ? caps.efforts.join(", ") : "on/off only"}.`];
  if (caps.mandatory) parts.push("This model always reasons — it cannot be switched off.");
  if (caps.defaultEffort) parts.push(`Its own default is "${caps.defaultEffort}".`);
  return parts.join("\n");
});

function requestReasoning() {
  reasoningCaps.value = null;
  if (session.value) chat.send("chat.reasoning.get");
}

// Both the chat and the model within it change what the lever should offer.
watch(() => [chat.chatId.value, modelId.value], requestReasoning, { immediate: true });

function onTempChange(e: Event) {
  const v = parseFloat((e.target as HTMLInputElement).value);
  if (!Number.isFinite(v)) return;
  if (session.value) session.value.temperature = v;
  chat.send("chat.settings", { temperature: v });
}

function onReasoningChange() {
  // "budget…" is a mode, not a value — give it a starting number so what gets sent is well-formed.
  if (reasoning.value === BUDGET) reasoning.value = BUDGET + (reasoningCaps.value?.maxTokenBudget ?? 4096);
  chat.send("chat.settings", { reasoning: reasoning.value });
}

function onBudgetChange(e: Event) {
  const n = parseInt((e.target as HTMLInputElement).value, 10);
  if (!Number.isFinite(n) || n <= 0) return;
  reasoning.value = BUDGET + n;
  chat.send("chat.settings", { reasoning: reasoning.value });
}

// App-level data the chat's pickers need: the catalogue of connections and their health.
const modes = ref<string[]>([]);
const picks = ref<ModelPickDto[]>([]);
const connHealth = ref<Record<string, ConnHealth>>({});
const skillBtn = ref<HTMLElement>();
const skillPickerOpen = ref(false);

/**
 * Ends the running skill from the UI.
 *
 * The model is supposed to call skill_deactivate as its last step; when it does not, the chat cannot
 * recover on its own — the skills index is suppressed while a skill is active, so it cannot be told
 * about another one, and a second activation is refused. This is the way out.
 */
function unloadSkill() {
  chat.send("chat.skill.deactivate");
}

/**
 * Lowers a raised tool set from the UI.
 *
 * This is the person's half of the design: the model MAY release a set it no longer needs, but is
 * never told it must — remembering to hand tools back is exactly the bookkeeping decision tool sets
 * exist to remove. So the reliable control lives here.
 */
function lowerToolSet(t: ToolSetState) {
  // An always-on set is a project setting, and a set a skill is running on belongs to that run —
  // neither is this control's to touch.
  if (t.by !== "agent" && t.by !== "user") return;
  chat.send("chat.toolset.deactivate", { setId: t.setId });
}

/** Why this set is in the line, and what it costs — the whole point of showing it. */
function toolSetTitle(t: ToolSetState): string {
  if (!t.disclosed) return `${t.setId} — announced only; the agent can load it when needed`;

  const who = t.by === "skill" ? "loaded by the active skill"
    : t.by === "agent" ? "loaded by the agent"
    : t.by === "user" ? "loaded by you"
    : "always on for this project";
  const canLower = t.by === "agent" || t.by === "user";
  return `${t.setId} — ${who}${t.reason ? ` (${t.reason})` : ""}`
    + (canLower ? ". Click to unload." : "");
}

const ctxPercent = computed(() => {
  if (ctxUsed.value == null || !ctxWindow.value) return null;
  return Math.min(100, Math.round((ctxUsed.value / ctxWindow.value) * 100));
});

/** "18.5k / 1.0M (2%)" when the window is known, else just the raw count — degrades gracefully
 *  for providers/runtimes GetContextLengthAsync can't resolve a window for. */
const ctxLabel = computed(() => {
  if (ctxUsed.value == null) return "";
  const used = formatCompact(ctxUsed.value);
  return ctxWindow.value
    ? `${used} / ${formatCompact(ctxWindow.value)} (${ctxPercent.value}%)`
    : `${used} tokens`;
});

/** This is the LAST LLM call, not a running total for the conversation — the agent loop makes one
 *  call per tool round-trip, and each resends the full history, so the latest call's token counts
 *  ARE the current context occupancy. The exact in/out split lives here, in the tooltip, since the
 *  pill only has room for the compact size. */
const ctxTitle = computed(() => {
  if (ctxUsed.value == null) return "";
  const parts: string[] = [];
  if (lastPrompt.value != null) parts.push(`in: ${lastPrompt.value.toLocaleString()}`);
  if (lastCompletion.value != null) parts.push(`out: ${lastCompletion.value.toLocaleString()}`);
  let title = parts.length ? `last request — ${parts.join(", ")}` : "";
  if (ctxWindow.value) {
    title += (title ? "\n" : "") + `context: ${ctxUsed.value.toLocaleString()} of ${ctxWindow.value.toLocaleString()} tokens`;
    if ((ctxPercent.value ?? 0) >= 95) title += " — almost full: start a new chat or the next request may fail";
    else if ((ctxPercent.value ?? 0) >= 80) title += " — getting full: consider a new chat soon";
  }
  return title;
});

/** Model entries grouped by their owning connection — the picker's two levels. Grouped by connection
 *  NAME, never by provider: two connections to the same provider must stay distinguishable. */
const groups = computed(() => {
  const out: { connectionId: string; connectionName: string; models: ModelPickDto[] }[] = [];
  for (const p of picks.value) {
    let g = out.find(x => x.connectionId === p.connectionId);
    if (!g) out.push(g = { connectionId: p.connectionId, connectionName: p.connectionName || p.connectionId, models: [] });
    g.models.push(p);
  }
  return out;
});

/** Health is a property of the connection (endpoint + key), so the dot rides on the group. */
function connEmoji(connectionId: string): string {
  const h = connHealth.value[connectionId];
  return !h || h.ok == null ? "" : h.ok ? "🟢 " : "🔴 ";
}

/** Name plus the provider's model string, when they differ — the name alone hides which model it is. */
function modelLabel(m: ModelPickDto): string {
  return m.model && m.model !== m.name ? `${m.name} · ${m.model}` : m.name;
}

/** The health of the selected model's connection, not of the model itself. */
const currentConnectionId = computed(() =>
  picks.value.find(p => p.id === modelId.value)?.connectionId ?? "");

const healthClass = computed(() => {
  const h = connHealth.value[currentConnectionId.value];
  if (!h || h.ok == null) return "";
  return h.ok ? "ok" : "err";
});
const healthTitle = computed(() => {
  const h = connHealth.value[currentConnectionId.value];
  if (!h || h.ok == null) return "";
  return h.ok ? "Reachable" : (h.error || "Unreachable");
});

// ── Provider info ("i") ──────────────────────────────────────────────────────
// Passive by design: it never polls. It reads on open, and afterwards reflects whatever the last
// read reported — a badge that quietly hammered the provider on a timer would cost money on some of
// them and lie on the rest.
const infoBtn = ref<HTMLElement>();
const infoOpen = ref(false);
const worstSeverity = ref<"normal" | "warn" | "critical">("normal");

function toggleInfo() { infoOpen.value = !infoOpen.value; }

const alertClass = computed(() =>
  worstSeverity.value === "critical" ? "crit" : worstSeverity.value === "warn" ? "warn" : "");

function onModeChange() {
  chat.send("chat.settings", { mode: mode.value });
}
function onModelChange() {
  // A different model may sit behind a different key, so the previous badge says nothing about it.
  worstSeverity.value = "normal";
  infoOpen.value = false;
  chat.send("chat.settings", { modelId: modelId.value });
}
/** What raised it, oldest first — the person clearing the flag is entitled to see what they are
 *  clearing, and the count on its own would just be a number to dismiss. */
const doubtTitle = computed(() => {
  const causes = doubt.value?.causes ?? [];
  if (!causes.length) return "";
  const lines = causes.map(c => `· ${c.what}  (${c.zone})`);
  return [
    "Took in content from a source nobody named:",
    ...lines,
    "",
    "Clear it once you have looked."
  ].join("\n");
});

function clearDoubt() {
  if (!doubt.value?.raised) return;
  if (!confirm(doubtTitle.value)) return;
  chat.send("chat.doubt.clear");
}

// Only app-level traffic is subscribed to here. Everything chat-scoped — mode, model, skill, tool
// sets, doubt, token usage — arrives through state/chatSessions and is read off the session above.
const offWelcome = client.on("welcome", p => {
  modes.value = p.modes || [];
  picks.value = p.connections || [];
});
// Switching projects replaces the catalogue wholesale. Without this the picker kept the previous
// project's entries and showed a blank selection for a chat whose model it had never heard of —
// welcome only ever describes the project the window started on.
const offContext = client.on("project.context", p => {
  picks.value = p.connections || [];
  if (p.modes?.length) modes.value = p.modes;
  requestReasoning();
});
// The editor broadcasts the connection TREE; the picker needs it flattened. Done here rather than
// asking the server for a second shape — the tree already carries everything the two levels need.
const offResult = client.on("connections.result", p => {
  picks.value = (p.connections || []).flatMap(c =>
    (c.models || []).map(m => ({
      id: m.id,
      name: m.name || m.model || m.id,
      model: m.model,
      connectionId: c.id,
      connectionName: c.name || c.id,
      provider: c.provider
    })));
});
// The badge follows whatever the last read reported — including a read triggered by another window.
const offInfo = client.on("provider.info.result", p => {
  if (p.modelId !== modelId.value) return;
  const all = (p.sections || []).flatMap(s => s.facts || []);
  worstSeverity.value = all.some(f => f.severity === "critical") ? "critical"
    : all.some(f => f.severity === "warn") ? "warn"
    : "normal";
});
const offHealth = client.on("connections.health", p => {
  connHealth.value = {};
  for (const s of p.statuses || []) connHealth.value[s.id] = { ok: s.ok, error: s.error };
});
// Chat-scoped, but it answers a question this component asked and nothing else keeps it — so it is
// read here, guarded by the chat id like every other reply that can outlive its question.
const offReasoning = client.on("chat.reasoning.result", (p, env) => {
  if ((p.chatId || env.chatId) !== chat.chatId.value) return;
  reasoningCaps.value = p.reasoning;
});
onUnmounted(() => { offWelcome(); offContext(); offResult(); offHealth(); offInfo(); offReasoning(); });
</script>
