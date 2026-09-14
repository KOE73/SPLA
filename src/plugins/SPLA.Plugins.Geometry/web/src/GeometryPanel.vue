<!--
  Geometry panel (built and shipped INSIDE the plugin, mounted by the host at runtime).

  What it is for: while the model places a box, looks at the picture and corrects it, a human reading
  tool replies sees only coordinates and cannot tell whether the box is converging on the object or
  wandering. So this shows the current render large AND the last few renders as a strip — the strip
  IS the point: side by side they show the box walking towards the object.

  READ-ONLY, deliberately (ADR_20260914-2 §3.4). The model drives the geometry through its tools;
  a second source of edits would make the session editable from two places at once and race the
  model's turn against a human hand. Picking which stored render to look at is not an edit.

  The panel holds no state the plugin would have to free: the C# side answers from the chat's markup
  session and its blob ring, and what lives here is a picture cache keyed by ring slot.
-->
<template>
  <div class="geom">
    <div class="head">
      <span class="title">{{ t('Geometry') }}</span>
      <span v-if="state.hasSession && state.view" class="muted">
        {{ state.view.id }} — {{ state.view.width }}×{{ state.view.height }} px
        <template v-if="state.view.fromBox">, {{ t('from box') }} “{{ state.view.fromBox }}”</template>
        <template v-if="state.view.deskewed">, {{ t('deskewed') }}</template>
      </span>
    </div>

    <div v-if="!state.hasSession" class="empty">
      {{ t('No image is open in this chat. The panel fills in as soon as the agent opens one.') }}
    </div>

    <template v-else>
      <div class="stage">
        <img v-if="shownSrc" :src="shownSrc" :alt="t('Marked-up frame')" draggable="false">
        <div v-else class="muted">{{ t('Waiting for the first render…') }}</div>
      </div>

      <div v-if="renders.length" class="strip">
        <button v-for="(item, i) in renders" :key="item.name + item.at" type="button"
                class="thumb" :class="{ sel: item.name === shownName, live: item.name === state.current }"
                :title="thumbTitle(item, i)" @click="pick(item.name)">
          <img v-if="pictures[key(item)]" :src="pictures[key(item)]" alt="">
          <span v-else class="ph">…</span>
          <span class="no">{{ i + 1 }}</span>
        </button>
        <button v-if="shownName !== state.current" type="button" class="latest" @click="follow()">
          {{ t('Latest') }}
        </button>
      </div>

      <div class="facts">
        <div class="row">
          <span class="muted">{{ t('Source') }}</span><code>{{ state.source }}</code>
          <span v-if="state.sourceWidth" class="muted">{{ state.sourceWidth }}×{{ state.sourceHeight }} px</span>
        </div>
        <table v-if="state.objects?.length" class="objects">
          <tbody>
            <tr v-for="o in state.objects" :key="o.name" :class="{ away: !o.visible }">
              <td class="name">{{ o.name }}</td>
              <td class="muted">{{ o.kind === 'box' ? t('box') : t('point') }}</td>
              <td><code>{{ o.where }}</code></td>
              <td :class="o.status === 'accepted' ? 'ok' : 'edit'">
                {{ o.status === 'accepted' ? t('accepted') : t('editing') }}
              </td>
              <td class="muted">{{ o.visible ? '' : t('outside this view') }}</td>
            </tr>
          </tbody>
        </table>
        <div v-else class="muted">{{ t('Nothing marked yet.') }}</div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref } from "vue";
import { t } from "./i18n";
import type { PanelApi } from "./panel";
import type { GeometryState, RenderEvent, RenderRef } from "./types";

const props = defineProps<{ api: PanelApi }>();

// One panel per connection is enough: the geometry session belongs to the chat, and the panel is a
// window onto whichever chat is current.
const panelId = "geometry-session-main";

const state = reactive<GeometryState>({ chatId: null, hasSession: false });
/** Ring slot ("name@at") → data URL. A picture is fetched once and kept until its slot is rewritten. */
const pictures = reactive<Record<string, string>>({});
/** Which render the big view shows; null follows the head of the ring. */
const pinned = ref<string | null>(null);
const disposers: Array<() => void> = [];
let opened = false;

const key = (item: RenderRef) => `${item.name}@${item.at}`;

const renders = computed<RenderRef[]>(() => state.renders ?? []);
const shownName = computed(() => pinned.value ?? state.current ?? renders.value.at(-1)?.name ?? null);
const shownSrc = computed(() => {
  const item = renders.value.find(r => r.name === shownName.value);
  return item ? pictures[key(item)] ?? "" : "";
});

const pick = (name: string) => { pinned.value = name; };
const follow = () => { pinned.value = null; };
const thumbTitle = (item: RenderRef, i: number) =>
  `${i + 1}. ${item.name}${item.name === state.current ? ` — ${t('latest')}` : ""}`;

function send(inputType: string, data: object = {}) {
  props.api.send("plugin.panel.input", { panelId, inputType, data });
}

/** Ask for every picture the strip needs and does not have. Keyed by slot+time, so a rewritten ring
 *  slot is refetched and an unchanged one never is — the ring is megabytes, and re-sending all of it
 *  on every step of the loop would put the whole history on the wire once per correction. */
function fetchMissing() {
  for (const item of renders.value) if (!pictures[key(item)]) send("render", { name: item.name });
}

function open() {
  if (opened) return;
  opened = props.api.send("plugin.panel.open", {
    panelId, panelType: "geometry.session", parameters: { chatId: props.api.currentChatId() ?? "" }
  });
}

onMounted(() => {
  open();
  disposers.push(props.api.on("conn", ((event: { on?: boolean }) => {
    // The plugin-side session dies with the connection it was opened on.
    if (event?.on) { opened = false; open(); }
  }) as never));

  disposers.push(props.api.on("plugin.panel.event", ((event: {
    panelId: string; eventType: string; data?: GeometryState & RenderEvent;
  }) => {
    if (event.panelId !== panelId || !event.data) return;
    if (event.eventType === "state") {
      const next = event.data as GeometryState;
      Object.assign(
        state,
        { source: undefined, view: undefined, objects: [], renders: [], current: null },
        next);
      // The pin names a slot in a ring that keeps turning: once it is gone, follow the head again.
      if (pinned.value && !renders.value.some(r => r.name === pinned.value)) pinned.value = null;
      fetchMissing();
    } else if (event.eventType === "render") {
      const render = event.data as RenderEvent;
      if (render.missing || !render.base64) return;
      pictures[`${render.name}@${render.at}`] =
        `data:${render.mimeType || "image/png"};base64,${render.base64}`;
    }
  }) as never));

  // Mounted is open (no visibility flag — that guard is what froze the debug panel on chat one), so
  // the chat is followed unconditionally. Pictures are dropped: they belong to the chat we left.
  disposers.push(props.api.onChatChange(chatId => {
    for (const cached of Object.keys(pictures)) delete pictures[cached];
    pinned.value = null;
    send("bind", { chatId: chatId ?? "" });
  }));
});

onBeforeUnmount(() => {
  props.api.send("plugin.panel.close", { panelId });
  disposers.forEach(dispose => dispose());
});
</script>

<style scoped>
.geom { display: flex; flex-direction: column; gap: 8px; height: 100%; min-height: 0; padding: 8px; }
.head { display: flex; align-items: baseline; gap: 8px; }
.title { font-weight: 600; }
.muted { color: var(--muted); font-size: var(--fs-xs); }
.empty { padding: 16px 4px; color: var(--muted); }
.stage { flex: 1; min-height: 120px; display: grid; place-items: center; overflow: hidden;
         background: var(--bg); border: 1px solid var(--border); border-radius: 4px; }
.stage img { display: block; max-width: 100%; max-height: 100%; object-fit: contain; }
.strip { display: flex; align-items: center; gap: 6px; overflow-x: auto; padding-bottom: 2px; }
.thumb { position: relative; flex: 0 0 auto; width: 86px; height: 64px; padding: 0; overflow: hidden;
         background: var(--bg); border: 1px solid var(--border); border-radius: 4px; cursor: pointer; }
.thumb img { width: 100%; height: 100%; object-fit: contain; }
.thumb.sel { outline: 2px solid var(--accent, #4a9); outline-offset: -2px; }
.thumb.live { border-color: var(--accent, #4a9); }
.thumb .ph { color: var(--muted); }
.thumb .no { position: absolute; right: 2px; bottom: 1px; padding: 0 3px; color: var(--text);
             background: var(--panel); border-radius: 3px; font-size: var(--fs-xs); }
.latest { flex: 0 0 auto; height: 24px; padding: 0 8px; color: var(--text); background: transparent;
          border: 1px solid var(--border); border-radius: 4px; cursor: pointer; }
.facts { display: flex; flex-direction: column; gap: 4px; font-size: var(--fs-sm); }
.row { display: flex; align-items: baseline; gap: 8px; flex-wrap: wrap; }
.objects { width: 100%; border-collapse: collapse; }
.objects td { padding: 1px 6px 1px 0; vertical-align: baseline; }
.objects tr.away { opacity: 0.55; }
.name { font-weight: 600; }
.ok { color: var(--muted); }
.edit { color: var(--accent, #4a9); }
code { font-size: var(--fs-xs); }
</style>
