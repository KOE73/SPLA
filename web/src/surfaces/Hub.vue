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
    </header>

    <p v-if="message" class="hub-message" :class="{ bad: messageIsBad }">{{ message }}</p>

    <div v-if="projects.length === 0" class="hub-empty">
      Nothing here yet. A project appears once it has been opened or started at least once.
    </div>

    <ul v-else class="hub-list">
      <li v-for="p in projects" :key="p.projectId" class="hub-row" :class="{ missing: !p.exists }">
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
        </div>

        <div class="hub-actions">
          <button v-if="!p.state" :disabled="!p.exists || busy === p.projectId" @click="start(p)">
            Start
          </button>
          <button :disabled="!p.exists || busy === p.projectId" @click="open(p)">
            {{ p.windows > 0 ? "Show" : "Open" }}
          </button>
          <button v-if="p.state" :disabled="busy === p.projectId" @click="close(p, false)">Close</button>
          <button
            v-if="p.state"
            class="danger"
            :disabled="busy === p.projectId"
            :title="'Closes it even mid-turn, discarding work in progress'"
            @click="close(p, true)"
          >
            Kill
          </button>
          <button
            v-if="!p.state"
            class="quiet"
            :disabled="busy === p.projectId"
            title="Removes it from this list only. The project itself is untouched."
            @click="forget(p)"
          >
            Forget
          </button>
        </div>
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, ref } from "vue";
import { registryClient, type KnownProject } from "../protocol/RegistryClient";

const projects = ref<KnownProject[]>([]);
const connected = ref(false);
const busy = ref<string | null>(null);
const message = ref("");
const messageIsBad = ref(false);

const disposers: Array<() => void> = [];

onMounted(() => {
  disposers.push(registryClient.onProjects(list => { projects.value = list; }));
  disposers.push(registryClient.onConn(up => { connected.value = up; }));
  registryClient.watch();
  void registryClient.refresh();
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

function say(text: string, bad = false) {
  message.value = text;
  messageIsBad.value = bad;
  if (text) window.setTimeout(() => { if (message.value === text) message.value = ""; }, 6000);
}

async function start(p: KnownProject) {
  busy.value = p.projectId;
  say("");
  const error = await registryClient.start(p.projectId);
  if (error) say(error, true);
  await registryClient.refresh();
  busy.value = null;
}

/**
 * Open and Show are the same button because they are the same intent — "put this project in front of
 * me" — and only the hub can tell which one that means today. Showing an existing window beats opening
 * a duplicate, and falling through to opening one when the raise fails is right either way: the window
 * may have closed between the listing and the click.
 */
async function open(p: KnownProject) {
  busy.value = p.projectId;
  say("");

  const windowOnIt = p.windows > 0 && p.instanceId ? await registryClient.focus(p.instanceId) : false;
  if (!windowOnIt) {
    // The hub cannot open a window itself — it has no desktop and, on a server, no screen to open one
    // onto. Starting the agent is the part it can do; the window follows from wherever the person is.
    const error = await registryClient.start(p.projectId);
    if (error) say(error, true);
    else say("Started. Open a window on it from the tray or the desktop app.");
  }

  await registryClient.refresh();
  busy.value = null;
}

async function close(p: KnownProject, force: boolean) {
  busy.value = p.projectId;
  say("");
  const error = await registryClient.close(p.projectId, force);
  if (error) say(error, true);
  await registryClient.refresh();

  // The agent decides, so "asked" is not "done": a turn in progress is refused, and the row simply
  // stays. Saying so is the whole point of having somewhere to say it — in the tray this looked
  // exactly like a button that did nothing.
  const still = projects.value.find(x => x.projectId === p.projectId);
  if (!error && still?.state && !force) {
    say("It refused — something is still running. Kill closes it anyway, discarding that work.", true);
  }

  busy.value = null;
}

async function forget(p: KnownProject) {
  busy.value = p.projectId;
  say("");
  if (await registryClient.forget(p.projectId)) say("Removed from the list. The project is untouched.");
  else say("It was not in the list.", true);
  await registryClient.refresh();
  busy.value = null;
}
</script>

<style scoped>
.hub { padding: 16px 18px; height: 100%; overflow: auto; box-sizing: border-box; color: var(--text); }

.hub-head { display: flex; align-items: baseline; gap: 10px; margin-bottom: 12px; }
.hub-head h1 { font-size: 1.15rem; margin: 0; }
.hub-conn { font-size: var(--fs-sm); color: var(--muted); }
.hub-conn.off { color: var(--danger); }

.hub-message {
  margin: 0 0 10px;
  padding: 7px 10px;
  border-radius: var(--radius-sm);
  background: var(--accent-soft);
  font-size: var(--fs-sm);
}
.hub-message.bad { background: color-mix(in srgb, var(--danger) 16%, transparent); }

.hub-empty { color: var(--muted); font-size: var(--fs-sm); }

.hub-list { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: 6px; }
.hub-row {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 9px 11px;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  background: var(--panel);
}
.hub-row.missing { opacity: 0.65; }

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

.hub-actions { display: flex; gap: 5px; flex-shrink: 0; }
.hub-actions button {
  background: transparent;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  color: var(--text);
  font-size: var(--fs-sm);
  padding: 4px 9px;
  cursor: pointer;
}
.hub-actions button:hover:not(:disabled) { border-color: var(--accent); color: var(--accent); }
.hub-actions button:disabled { opacity: 0.45; cursor: default; }
.hub-actions button.danger:hover:not(:disabled) { border-color: var(--danger); color: var(--danger); }
.hub-actions button.quiet { color: var(--muted); }
</style>
