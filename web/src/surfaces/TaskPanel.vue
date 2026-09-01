<!--
  Live background task panel. Shows tasks currently running in this chat, updates as they start
  and finish via task.state.changed push events. Does not persist state across chat switches —
  each chat has its own independent task view.
-->
<template>
  <div v-if="tasks.length > 0" class="task-panel">
    <div class="task-row" v-for="task in tasks" :key="task.taskId">
      <div class="task-name">{{ task.toolName }}</div>
      <div class="task-state" :class="task.state.toLowerCase()">{{ task.state }}</div>
      <div class="task-time">{{ formatTime(task.startedAt) }}</div>
      <button
        v-if="task.state === 'Running'"
        class="task-cancel"
        title="Cancel this task"
        @click="cancelTask(task.taskId)"
      >✕</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, ref, watch } from "vue";
import { client } from "../protocol/SplaClient";
import { useChat } from "../state/chatContext";
import type { TaskSummaryDto } from "../protocol/types";

const chat = useChat();
const tasks = ref<TaskSummaryDto[]>([]);

/** Load the current server state when mounting or chat changes. */
async function loadTasks(chatId: string) {
  if (!chatId) return;
  try {
    const result = await client.invoke<{ chatId: string; tasks: TaskSummaryDto[] }>(
      "task.list",
      { chatId },
      { chatId }
    );
    tasks.value = result.tasks || [];
  } catch (e) {
    console.error("Failed to load task list:", e);
    tasks.value = [];
  }
}

/** Subscribe to task state changes; fires whenever a task starts or finishes. */
const offStateChanged = client.on("task.state.changed", (payload) => {
  // Ignore tasks from other chats — this panel is chat-scoped.
  if (payload.chatId !== chat.chatId.value) return;

  // Upsert: replace existing by taskId, or push a new row.
  const idx = tasks.value.findIndex(t => t.taskId === payload.task.taskId);
  if (idx >= 0) {
    tasks.value[idx] = payload.task;
  } else {
    tasks.value.push(payload.task);
  }
});

/** Request cancellation. The outcome comes via task.state.changed, not a direct reply. */
function cancelTask(taskId: string) {
  const chatId = chat.chatId.value;
  if (chatId) {
    client.send("task.cancel", { chatId, taskId }, { chatId });
  }
}

/**
 * Format an ISO 8601 timestamp as a human-readable string.
 * Shows relative time for recent tasks, absolute time for older ones.
 */
function formatTime(isoString: string): string {
  try {
    const date = new Date(isoString);
    const now = Date.now();
    const diff = now - date.getTime();

    // Within the last minute: show as "just now"
    if (diff < 60000) return "just now";

    // Within the last hour: show minutes ago
    if (diff < 3600000) {
      const mins = Math.floor(diff / 60000);
      return `${mins} minute${mins !== 1 ? 's' : ''} ago`;
    }

    // Within the last day: show hours ago
    if (diff < 86400000) {
      const hours = Math.floor(diff / 3600000);
      return `${hours} hour${hours !== 1 ? 's' : ''} ago`;
    }

    // Older: show absolute time
    return date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
  } catch {
    return isoString;
  }
}

// Load initial task list and refresh when chat changes.
watch(() => chat.chatId.value, (chatId) => {
  tasks.value = [];
  if (chatId) loadTasks(chatId);
}, { immediate: true });

onUnmounted(offStateChanged);
</script>

<style scoped>
.task-panel {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 8px 10px;
  border-top: 1px solid var(--border);
  background: var(--bg-2, color-mix(in srgb, var(--text) 2%, transparent));
}

.task-row {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: var(--fs-xs);
  padding: 6px 8px;
  background: var(--bg-1, transparent);
  border: 1px solid var(--border);
  border-radius: var(--radius-sm, 4px);
}

.task-name {
  font-weight: 600;
  color: var(--text);
  flex: 0 1 auto;
  min-width: 10ch;
}

.task-state {
  padding: 1px 6px;
  border-radius: 2px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  background: var(--border);
  color: var(--muted);
}

.task-state.running {
  background: var(--accent);
  color: var(--bg-1, white);
}

.task-state.completed {
  background: color-mix(in srgb, var(--accent) 20%, transparent);
  color: var(--accent);
}

.task-state.failed {
  background: color-mix(in srgb, red 20%, transparent);
  color: red;
}

.task-state.cancelled {
  background: color-mix(in srgb, var(--muted) 20%, transparent);
  color: var(--muted);
}

.task-time {
  color: var(--muted);
  margin-left: auto;
  white-space: nowrap;
}

.task-cancel {
  background: none;
  border: none;
  color: var(--muted);
  cursor: pointer;
  padding: 2px 4px;
  font-size: inherit;
  display: flex;
  align-items: center;
  justify-content: center;
}

.task-cancel:hover {
  color: var(--accent);
}
</style>
