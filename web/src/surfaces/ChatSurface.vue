<!--
  The chat. This component owns the chat id for everything beneath it: the log, the composer and the
  status line receive it through provide/inject and neither read the global focus nor stamp ids onto
  their own commands. Switching chats is a change of session here, not a teardown down there.
-->
<template>
  <div id="main" class="chat-surface">
    <div id="log"><ChatLog /></div>
    <TaskPanel />
    <!-- A read-only surface loses the composer AND the status bar: the latter is entirely settings
         for the next turn (mode, model, temperature, reasoning, skills, tool sets), and there is no
         next turn here. What replaces them is a line saying why, so the missing composer reads as an
         answer rather than as a window that failed to finish loading. -->
    <div v-if="readOnly" id="readonly-note">{{ readOnlyReason }}</div>
    <template v-else>
      <div id="composer"><Composer /></div>
      <div id="status"><StatusBar /></div>
    </template>
    <div id="filters"><Filters /></div>
  </div>
</template>

<script setup lang="ts">
import { computed, watch } from "vue";
import ChatLog from "./ChatLog.vue";
import Composer from "./Composer.vue";
import StatusBar from "./StatusBar.vue";
import Filters from "./Filters.vue";
// Mounted inline, not only registered in registry.ts — registry.ts alone only reaches a tear-off
// window opened at ?surface=taskPanel, and PLAN_20260825 wave E's whole point ("видно, что гасишь")
// needs the panel visible in the ordinary chat window, not behind a URL nobody would guess.
import TaskPanel from "./TaskPanel.vue";
import { store } from "../state/store";
import { focusSession, peekSession } from "../state/chatSessions";
import { findChat } from "../state/chatTree";
import { provideChat } from "../state/chatContext";
import { client } from "../protocol/SplaClient";

const chatId = computed(() => store.currentChat);
const session = computed(() => peekSession(chatId.value));

/**
 * The two surfaces that show a conversation nobody in this window may write to, and they are read-only
 * for different reasons — see PLAN_20260903:
 *
 *  - an archived chat is a frozen snapshot; the session says so because `chat.read` filled it, and
 *    the server refuses `chat.open`/`chat.send` for it outright;
 *  - a spawned session is live, but the writer is the tool that gave the errand, not whoever is
 *    watching. That one is enforced server-side too while its run is in progress (`ChatHandlers.Send`
 *    refuses it) — this is the facade over a real rule, not a facade instead of one.
 */
const spawned = computed(() => {
  const id = chatId.value;
  return !!id && findChat(store.chats, id)?.origin === "spawned";
});
const readOnly = computed(() => !!session.value?.readOnly || spawned.value);
const readOnlyReason = computed(() =>
  spawned.value
    ? "This session is a sub-agent's — it is driven by whoever gave it the errand, not from here."
    : "This chat is archived. Restore it from the chat list to write in it again.");

// Focusing is a state change (it reorders which logs are worth keeping), so it belongs in a watcher
// rather than inside the computed that reads the session.
watch(chatId, id => { if (id) focusSession(id); }, { immediate: true });

provideChat({
  chatId,
  session,
  send(type: string, payload: Record<string, unknown> = {}) {
    const id = chatId.value;
    if (!id) return;
    client.send(type, { chatId: id, ...payload });
  }
});
</script>

<style scoped>
.chat-surface { width: 100%; height: 100%; min-width: 0; min-height: 0; }

#readonly-note {
  padding: 8px var(--pad);
  border-top: 1px solid var(--border);
  color: var(--muted);
  font-size: 12px;
}
</style>
