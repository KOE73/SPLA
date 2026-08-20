<!--
  Shown when the agent behind this window has stopped answering for long enough to call it gone.

  This exists because the window used to say nothing at all. The socket dropped, the client retried
  every 1.5s forever, and from the outside the window simply stopped working — that was the "orphan
  window" left behind by closing an agent from the tray. Retrying is right; retrying in silence with
  no way out is not.

  Deliberately does NOT hide the chat behind it: the conversation on screen is still worth reading,
  and covering it would punish the person for something the agent did. It blocks nothing — sending is
  already impossible while the socket is closed, and the composer reports that itself.
-->
<template>
  <div v-if="store.connectionLost" class="lost-banner" role="status">
    <div class="lost-text">
      <b>The agent stopped answering.</b>
      <span>{{ detail }}</span>
    </div>
    <div class="lost-actions">
      <button class="lost-btn" :disabled="busy" @click="retry">Try again</button>
      <button v-if="embedded" class="lost-btn primary" :disabled="busy" @click="restart">
        Restart the agent
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { client } from "../protocol/SplaClient";
import { store } from "../state/store";

// Only the native shell can start a service; a browser tab can only wait for one to come back. So
// the offer to restart appears exactly where it can be honoured, rather than being shown everywhere
// and failing for half the people who press it.
const embedded = typeof window !== "undefined" && !!window.chrome?.webview;

const busy = ref(false);

const detail = computed(() =>
  embedded
    ? "It may have been closed from the tray, or it may have crashed. This window is still here."
    : "It may have been closed or restarted. This window will keep trying on its own.");

function retry() {
  busy.value = true;
  client.reconnectNow();
  // Cleared on a timer rather than on success: "connected" arrives as an event this component does
  // not own, and a button stuck disabled after a failed retry would be worse than one re-enabled a
  // moment early.
  window.setTimeout(() => { busy.value = false; }, 2000);
}

/** Asks the native shell to bring a service back up and re-point this window at it. The shell joins
 *  a live instance through the project's lock file when there is one, so this is "reattach or start"
 *  rather than "always start a second". */
function restart() {
  busy.value = true;
  try { window.chrome?.webview?.postMessage({ kind: "restartService" }); }
  catch { /* not embedded after all — the retry button is still there */ }
  window.setTimeout(() => { busy.value = false; }, 4000);
}
</script>

<style scoped>
.lost-banner {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 12px;
  background: color-mix(in srgb, var(--danger) 14%, var(--panel));
  border-bottom: 1px solid var(--danger);
  font-size: var(--fs-sm);
}
.lost-text { display: flex; flex-direction: column; flex: 1; min-width: 0; }
.lost-text span { color: var(--muted); }
.lost-actions { display: flex; gap: 6px; flex-shrink: 0; }
.lost-btn {
  background: transparent;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  color: var(--text);
  font-size: var(--fs-sm);
  padding: 5px 10px;
  cursor: pointer;
}
.lost-btn:hover:not(:disabled) { border-color: var(--accent); color: var(--accent); }
.lost-btn:disabled { opacity: 0.5; cursor: default; }
.lost-btn.primary { background: var(--accent); color: var(--accent-contrast, #fff); border-color: var(--accent); }
</style>
