<!--
  The project manager: everything this machine remembers, running or not, with the actions that
  apply to each.

  Two things make this a different page rather than another panel of the chat client. It is served by
  the HUB, which holds no project and speaks only the registry vocabulary — so it uses RegistryClient,
  not SplaClient. And it is the one place with room to REPORT: the tray can only act, because it has
  nowhere to put a message without stealing focus, which is why a refused Close there looks exactly
  like nothing happening. Here a refusal can simply be shown.
-->
<template>
  <div class="hub">
    <header class="hub-head">
      <h1>Projects</h1>
      <span class="hub-conn" :class="{ off: !connected }">
        {{ connected ? "watching" : "hub unreachable" }}
      </span>
      <span class="hub-head-spacer"></span>
      <!-- The tray's own "Close" opens this window instead of acting whenever anything is still
           working, precisely so this button is here to be reached for — the one place that can both
           show what's still running and let it be discarded anyway. -->
      <button
        v-if="anyRunning"
        class="hub-killall"
        :disabled="killingAll"
        title="Force-closes every running project at once, discarding any work in progress"
        @click="killAll"
      >
        Kill all
      </button>
    </header>

    <div class="hub-scroll">
      <div v-if="projects.length === 0" class="hub-empty">
        Nothing here yet. A project appears once it has been opened or started at least once.
      </div>

      <!-- One list per group. In "name" order there is only ever one, so the markup is the same
           shape either way and the split is a property of the chosen order, not of the template. -->
      <div v-else class="hub-groups">
        <ul v-for="g in groups" :key="g.key" class="hub-list" :class="g.key">
        <li v-for="p in g.items" :key="p.projectId" class="hub-row" :class="{ missing: !p.exists }">
          <!-- Anchored to the row it's about, not a shared banner above the list: a refusal on one
               project must not read as ambiguous, and must not shove every row below it down and up
               again as it appears and times out. -->
          <div v-if="rowMessage?.id === p.projectId" class="hub-balloon" :class="{ bad: rowMessage.bad }">
            {{ rowMessage.text }}
          </div>

          <div class="hub-main">
            <div class="hub-name">
              {{ p.name || p.projectId }}
              <span v-if="!p.exists" class="hub-tag bad" title="The manifest is no longer at this path">
                missing
              </span>
            </div>
            <div class="hub-path" :title="p.projectId">{{ p.projectId }}</div>
          </div>

          <div class="hub-state">
            <span class="dot" :class="p.state ? 'on' : 'off'"></span>
            <span>{{ describe(p) }}</span>
            <!-- Shown whether or not the project is up, because the hub address works either way: a
                 request for a stopped project starts it. The colour still reports live reachability,
                 so the pill answers "is it up" and "here is its address" without becoming two things. -->
            <span
              v-if="p.exists"
              class="mcp-pill"
              :class="[p.mcpAvailable ? 'on' : 'off', { copyable: true }]"
              role="button"
              tabindex="0"
              :title="mcpPillTitle(p)"
              @click="copyMcpAddress(p)"
              @keydown.enter.prevent="copyMcpAddress(p)"
              @keydown.space.prevent="copyMcpAddress(p)"
            >MCP</span>
          </div>

          <!-- Four fixed-width slots, always in the same order, so the buttons line up like a table
               column no matter which two or three of them apply to this row's state. -->
          <div class="hub-actions">
            <button v-if="!p.state" class="act" :disabled="!p.exists || busy === p.projectId" @click="start(p)">
              Start
            </button>
            <span v-else class="act-ph"></span>

            <button class="act" :disabled="!p.exists || busy === p.projectId" @click="open(p)">
              {{ p.windows > 0 ? "Show" : "Open" }}
            </button>

            <button v-if="p.state" class="act" :disabled="busy === p.projectId" @click="close(p, false)">
              Close
            </button>
            <button
              v-else
              class="act quiet"
              :disabled="busy === p.projectId"
              title="Removes it from this list only. The project itself is untouched."
              @click="forget(p)"
            >
              Forget
            </button>

            <button
              v-if="p.state"
              class="act danger"
              :disabled="busy === p.projectId"
              :title="'Closes it even mid-turn, discarding work in progress'"
              @click="close(p, true)"
            >
              Kill
            </button>
            <span v-else class="act-ph"></span>
          </div>
        </li>
        </ul>
      </div>
    </div>

    <footer class="hub-bar">
      <span class="hub-bar-label">Scheme</span>
      <select class="hub-bar-select" v-model="theme" @change="onThemeChange">
        <option v-for="t in themeOptions" :key="t.value" :value="t.value">{{ t.label }}</option>
      </select>
      <span class="hub-bar-label">Order</span>
      <select class="hub-bar-select" v-model="sortMode" @change="onSortChange">
        <option v-for="o in sortOptions" :key="o.value" :value="o.value" :title="o.hint">{{ o.label }}</option>
      </select>
      <span class="hub-bar-spacer"></span>
      <button class="hub-bar-icon" title="Settings" @click="openSettings">
        <Icon name="settings" :size="18" :weight="2" />
      </button>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from "vue";
import { registryClient, type KnownProject } from "../protocol/RegistryClient";
import { applyTheme, SYSTEM_THEME } from "../state/appearance";
import Icon from "../dock/Icon.vue";

const projects = ref<KnownProject[]>([]);
const connected = ref(false);
const busy = ref<string | null>(null);
const rowMessage = ref<{ id: string; text: string; bad: boolean } | null>(null);

// The hub has no project connection to save a scheme through (no "welcome"/"appearance.changed"
// push here — that's SplaClient's job, and the hub only speaks RegistryClient) — so it goes through
// its own file instead (HubAppearanceStore, via the /registry/appearance route). localStorage is
// still applied first, as a flash-of-default guard while that request is in flight — the web view
// backing a tray window isn't guaranteed to keep its own storage across runs the way a normal
// browser profile would, which is exactly why the file exists. "System" tracks the OS live.
const theme = ref(localStorage.getItem("spla.theme") || "dark");
const themeOptions = [
  { value: SYSTEM_THEME, label: "System" },
  { value: "dark", label: "Dark" },
  { value: "light", label: "Light" },
  { value: "cream", label: "Cream" },
  { value: "emerald", label: "Emerald" },
];

/**
 * How the list is ordered.
 *
 * <p>There is a default the server cannot supply: it hands the list back in remembered-projects
 * order, which is recency, so a row moves every time anything is opened or started. That makes the
 * window unusable as a place you reach for by muscle memory — the project you want is somewhere new
 * each time, so you have to read the list instead of pointing at it. Both orders here are therefore
 * built client-side, and both are stable: a name never changes because a project started.</p>
 *
 * <p>Kept in localStorage rather than in HubAppearanceStore, which the theme goes through. The theme
 * has to survive a tray web view losing its storage, because coming back to the wrong colours reads
 * as a bug; landing on the default order does not, and a per-screen habit is not worth a round trip
 * and a file. Worth revisiting if the hub ever grows more of these.</p>
 */
type SortMode = "name" | "active";

const sortOptions: { value: SortMode; label: string; hint: string }[] = [
  {
    value: "name",
    label: "Name",
    hint: "Alphabetical, always. A project keeps its place whatever it happens to be doing.",
  },
  {
    value: "active",
    label: "Active first",
    hint: "Running projects in a block on top, the rest below. Alphabetical inside each.",
  },
];

const sortMode = ref<SortMode>(localStorage.getItem("spla.hub.sort") === "active" ? "active" : "name");

function onSortChange() {
  localStorage.setItem("spla.hub.sort", sortMode.value);
}

// `numeric` so "project 10" lands after "project 9" rather than after "project 1", and `base` so
// case and accents do not split names that read as the same word.
function byName(a: KnownProject, b: KnownProject): number {
  return (a.name || a.projectId).localeCompare(
    b.name || b.projectId,
    undefined,
    { sensitivity: "base", numeric: true },
  );
}

/**
 * The rows to draw, already split into the blocks the chosen order asks for. An empty block is
 * dropped rather than rendered, so "nothing is running" does not leave a gap above the list where a
 * block used to be.
 */
const groups = computed<{ key: string; items: KnownProject[] }[]>(() => {
  const all = [...projects.value].sort(byName);
  if (sortMode.value === "name") return [{ key: "all", items: all }];

  return [
    { key: "running", items: all.filter(p => !!p.state) },
    { key: "idle", items: all.filter(p => !p.state) },
  ].filter(g => g.items.length > 0);
});

function onThemeChange() {
  applyTheme(theme.value);
  registryClient.saveAppearance(theme.value);
}

function openSettings() {
  window.open("/?surface=settings", "spla-settings", "width=640,height=720,resizable=yes");
}

const disposers: Array<() => void> = [];

onMounted(() => {
  disposers.push(registryClient.onProjects(list => { projects.value = list; }));
  disposers.push(registryClient.onConn(up => { connected.value = up; }));
  registryClient.watch();
  void registryClient.refresh();

  void registryClient.getAppearance().then(saved => {
    if (saved && saved !== theme.value) {
      theme.value = saved;
      applyTheme(saved);
    }
  });
});

onUnmounted(() => {
  disposers.forEach(d => d());
  registryClient.dispose();
});

function describe(p: KnownProject): string {
  if (!p.state) return "not running";
  // Zero windows with a live agent is not an oddity to hide — it is the headless case the instance
  // model exists to allow, and worth naming so it does not read as a mistake.
  const windows = p.windows === 0 ? "no window" : p.windows === 1 ? "1 window" : `${p.windows} windows`;
  return `${p.state}, ${windows}`;
}

function say(projectId: string, text: string, bad = false) {
  rowMessage.value = { id: projectId, text, bad };
  window.setTimeout(() => { if (rowMessage.value?.id === projectId && rowMessage.value.text === text) rowMessage.value = null; }, 6000);
}

function clearSay(projectId: string) {
  if (rowMessage.value?.id === projectId) rowMessage.value = null;
}

/**
 * Other listed projects that would answer to the same name. Mirrors what the hub's own resolver does
 * server-side (RegistryEndpoints.ResolveProjectId): match on the displayed name, case-insensitively,
 * and ignore rows whose manifest is gone — those cannot be started, so they must not make a live
 * name look ambiguous here either.
 */
function sharingNameWith(p: KnownProject): KnownProject[] {
  const name = (p.name ?? "").trim().toLowerCase();
  if (!name) return [];
  return projects.value.filter(
    x => x.projectId !== p.projectId && x.exists && (x.name ?? "").trim().toLowerCase() === name,
  );
}

function mcpPillTitle(p: KnownProject): string {
  const live = p.mcpAvailable
    ? "MCP reachable now"
    : "Not running — the address still works, the hub starts it on demand";
  return sharingNameWith(p).length > 0
    ? `${live}. Another project shares this name, so there is no unambiguous address by name — click for details.`
    : `${live}. Click to copy this project's MCP address.`;
}

/**
 * Copies the hub address for this project — the stable one, deliberately: the hub sits on a fixed
 * port and starts the instance on demand, so unlike an instance's own ephemeral endpoint this
 * survives everything being shut down and restarted.
 *
 * <p>On a name collision it copies nothing and says so, per the owner's explicit call: silently
 * copying an address that resolves to the wrong project, or to a 409, is worse than not copying. The
 * message carries the manifest path, which always resolves, so the way out is right there to
 * hand-copy rather than something to go and look up.</p>
 */
async function copyMcpAddress(p: KnownProject) {
  clearSay(p.projectId);

  const clash = sharingNameWith(p);
  if (clash.length > 0) {
    const others = clash.length === 1 ? "another project" : `${clash.length} other projects`;
    say(
      p.projectId,
      `"${p.name}" is also the name of ${others}, so an address by name would be ambiguous. ` +
        `Nothing was copied — address it by its manifest path instead: ${p.projectId}`,
      true,
    );
    return;
  }

  const url = `${window.location.origin}/mcp?project=${encodeURIComponent(p.name || p.projectId)}`;
  try {
    await navigator.clipboard.writeText(url);
    say(p.projectId, `Copied the MCP address: ${url}`);
  } catch {
    // A denied or unavailable clipboard must not swallow the answer — showing the address still
    // leaves the person able to select it by hand.
    say(p.projectId, `The clipboard refused. The address is: ${url}`, true);
  }
}

async function start(p: KnownProject) {
  busy.value = p.projectId;
  clearSay(p.projectId);
  const error = await registryClient.start(p.projectId);
  if (error) say(p.projectId, error, true);
  await registryClient.refresh();
  busy.value = null;
}

const embedded = typeof window !== "undefined" && !!window.chrome?.webview;

/**
 * Open and Show are the same button because they are the same intent — "put this project in front of
 * me" — and only the hub can tell which one that means today. Showing an existing window beats opening
 * a duplicate, and falling through to opening one when the raise fails is right either way: the window
 * may have closed between the listing and the click.
 */
async function open(p: KnownProject) {
  busy.value = p.projectId;
  clearSay(p.projectId);

  const windowOnIt = p.windows > 0 && p.instanceId ? await registryClient.focus(p.instanceId) : false;
  if (!windowOnIt) {
    if (embedded) {
      // This hub surface is running inside the desktop shell, which does have a screen — hand the
      // launch to it instead of only starting the agent headless and pointing the person at the tray.
      try { window.chrome!.webview!.postMessage({ kind: "openProject", projectId: p.projectId }); }
      catch { /* not embedded after all — fall through below */ }
    } else {
      // A plain browser tab (or a hub with no desktop shell at all) cannot open a window itself.
      // Starting the agent is the part it can do; the window follows from wherever the person is.
      const error = await registryClient.start(p.projectId);
      if (error) say(p.projectId, error, true);
      else say(p.projectId, "Started. Open a window on it from the tray or the desktop app.");
    }
  }

  await registryClient.refresh();
  busy.value = null;
}

async function close(p: KnownProject, force: boolean) {
  busy.value = p.projectId;
  clearSay(p.projectId);
  const error = await registryClient.close(p.projectId, force);
  if (error) say(p.projectId, error, true);
  await registryClient.refresh();

  // The agent decides, so "asked" is not "done": a turn in progress is refused, and the row simply
  // stays. Saying so is the whole point of having somewhere to say it — in the tray this looked
  // exactly like a button that did nothing.
  const still = projects.value.find(x => x.projectId === p.projectId);
  if (!error && still?.state && !force) {
    say(p.projectId, "It refused — something is still running. Kill closes it anyway, discarding that work.", true);
  }

  busy.value = null;
}

const anyRunning = computed(() => projects.value.some(p => !!p.state));
const killingAll = ref(false);

/** Force-closes every running project, in parallel — the tray's last resort, reached from here
 * because only here can a refusal (there should not be any, force is true) actually be shown. */
async function killAll() {
  killingAll.value = true;
  await Promise.all(
    projects.value.filter(p => p.state).map(p => registryClient.close(p.projectId, true)),
  );
  await registryClient.refresh();
  killingAll.value = false;
}

async function forget(p: KnownProject) {
  busy.value = p.projectId;
  clearSay(p.projectId);
  if (await registryClient.forget(p.projectId)) say(p.projectId, "Removed from the list. The project is untouched.");
  else say(p.projectId, "It was not in the list.", true);
  await registryClient.refresh();
  busy.value = null;
}
</script>

<style scoped>
/* Header and footer are fixed; only .hub-scroll scrolls — so the bottom bar always stays pinned to
   the window edge instead of drifting into the list's own scroll area. */
.hub { height: 100%; box-sizing: border-box; color: var(--text); display: flex; flex-direction: column; overflow: hidden; }
.hub-scroll { flex: 1 1 auto; overflow: auto; padding: 0 18px 16px; box-sizing: border-box; }

.hub-head { flex-shrink: 0; display: flex; align-items: baseline; gap: 10px; padding: 16px 18px 12px; }
.hub-head h1 { font-size: 1.15rem; margin: 0; }
.hub-conn { font-size: var(--fs-sm); color: var(--muted); }
.hub-conn.off { color: var(--danger); }
.hub-head-spacer { flex: 1 1 auto; }
.hub-killall {
  flex-shrink: 0;
  background: transparent;
  border: 1px solid var(--danger);
  border-radius: var(--radius-sm);
  color: var(--danger);
  font-size: var(--fs-sm);
  padding: 4px 10px;
  cursor: pointer;
}
.hub-killall:hover:not(:disabled) { background: color-mix(in srgb, var(--danger) 15%, transparent); }
.hub-killall:disabled { opacity: 0.45; cursor: default; }

.hub-empty { color: var(--muted); font-size: var(--fs-sm); }

.hub-list { list-style: none; margin: 12px 0 0; padding: 0; display: flex; flex-direction: column; gap: 6px; }
.hub-row {
  position: relative;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 9px 11px;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  background: var(--panel);
}
.hub-row.missing { opacity: 0.65; }

/* Anchored directly onto the row it's about — top edge just above the row's own top, so it reads as
   part of that row rather than a banner floating in the gap above it. Slightly translucent (mixed
   with transparent, not a flat fill) so it still reads as an overlay on that specific row rather than
   a solid card sitting apart from it, and built from theme variables so it stays in step with
   whichever scheme is picked. */
.hub-balloon {
  position: absolute;
  left: 11px;
  right: 11px;
  top: -4px;
  z-index: 1;
  padding: 6px 10px;
  border-radius: var(--radius-sm);
  border-left: 3px solid var(--accent);
  background: color-mix(in srgb, var(--panel) 85%, var(--accent-soft));
  color: var(--text);
  font-size: var(--fs-sm);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.18);
  backdrop-filter: blur(2px);
}
.hub-balloon.bad {
  border-left-color: var(--danger);
  background: color-mix(in srgb, var(--panel) 82%, color-mix(in srgb, var(--danger) 35%, transparent));
}

.hub-main { flex: 1; min-width: 0; }
.hub-name { display: flex; align-items: center; gap: 6px; }
.hub-path {
  font-size: var(--fs-sm);
  color: var(--muted);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.hub-tag { font-size: var(--fs-sm); padding: 1px 5px; border-radius: var(--radius-sm); }
.hub-tag.bad { background: color-mix(in srgb, var(--danger) 20%, transparent); color: var(--danger); }

.hub-state { display: flex; align-items: center; gap: 6px; font-size: var(--fs-sm); color: var(--muted); flex-shrink: 0; }
.dot { width: 8px; height: 8px; border-radius: 50%; background: var(--muted); }
.dot.on { background: var(--ok, #10b981); }

.mcp-pill {
  margin-left: 2px;
  font-size: var(--fs-xs);
  line-height: 1;
  padding: 2px 6px;
  border-radius: 999px;
  border: 1px solid var(--border);
  letter-spacing: 0.02em;
}
.mcp-pill.on { color: var(--accent); border-color: var(--accent); background: var(--accent-soft); }
.mcp-pill.off { color: var(--faint); }
.mcp-pill.copyable { cursor: pointer; user-select: none; }
.mcp-pill.copyable:hover { border-color: var(--accent); }

/* The two blocks are meant to read as one list with a seam, not as two sections: a small gap and a
   hairline, no headings. The split is already obvious from the rows themselves (a running project
   carries a lit dot and different buttons), so labelling it would only spend vertical space
   restating what the reader can see. */
.hub-groups { display: flex; flex-direction: column; }
.hub-list.idle { margin-top: 8px; padding-top: 8px; border-top: 1px solid var(--border); }
/* Just enough to place the block as secondary at a glance, not enough to make it look disabled —
   every button in it still works. */
.hub-list.idle .hub-name { color: var(--faint); }

/* Fixed-width grid, same four slots on every row (Start/blank, Open-or-Show, Close-or-Forget,
   Kill/blank) — buttons of different word lengths never shift the columns out of line. */
.hub-actions { display: grid; grid-template-columns: repeat(4, 62px); gap: 5px; flex-shrink: 0; }
.act-ph { display: block; }
.hub-actions .act {
  width: 100%;
  box-sizing: border-box;
  background: transparent;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  color: var(--text);
  font-size: var(--fs-sm);
  padding: 4px 6px;
  cursor: pointer;
  text-align: center;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.hub-actions .act:hover:not(:disabled) { border-color: var(--accent); color: var(--accent); }
.hub-actions .act:disabled { opacity: 0.45; cursor: default; }
.hub-actions .act.danger:hover:not(:disabled) { border-color: var(--danger); color: var(--danger); }
.hub-actions .act.quiet { color: var(--muted); }

.hub-bar {
  flex-shrink: 0;
  padding: 6px 18px;
  border-top: 1px solid var(--border);
  background: var(--panel);
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: var(--fs-sm);
  color: var(--muted);
}
.hub-bar-label { flex-shrink: 0; }
.hub-bar-select {
  background: transparent;
  color: var(--text);
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  font: inherit;
  font-size: var(--fs-sm);
  padding: 2px 4px;
}
.hub-bar-spacer { flex: 1 1 auto; }
.hub-bar-icon {
  flex-shrink: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  border: none;
  padding: 1px;
  cursor: pointer;
  color: var(--muted);
  border-radius: var(--radius-sm);
}
.hub-bar-icon:hover { color: var(--accent); }
</style>
