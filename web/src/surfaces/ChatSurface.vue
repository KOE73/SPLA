<!--
  The chat. This component owns the chat id for everything beneath it: the log, the composer and the
  status line receive it through provide/inject and neither read the global focus nor stamp ids onto
  their own commands. Switching chats is a change of session here, not a teardown down there.
-->
<template>
  <div id="main" class="chat-surface">
    <div id="log"><ChatLog /></div>
    <div id="composer"><Composer /></div>
    <div id="status"><StatusBar /></div>
    <div id="filters"><Filters /></div>
  </div>
</template>

<script setup lang="ts">
import { computed, watch } from "vue";
import ChatLog from "./ChatLog.vue";
import Composer from "./Composer.vue";
import StatusBar from "./StatusBar.vue";
import Filters from "./Filters.vue";
import { store } from "../state/store";
import { focusSession, peekSession } from "../state/chatSessions";
import { provideChat } from "../state/chatContext";
import { client } from "../protocol/SplaClient";

const chatId = computed(() => store.currentChat);
const session = computed(() => peekSession(chatId.value));

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
</style>
