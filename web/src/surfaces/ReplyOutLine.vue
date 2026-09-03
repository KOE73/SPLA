<!--
  One outgoing reply/correspondence call, rendered as speech ("→ to <role>") instead of a generic
  ToolCard (PLAN_20260902 wave 7; ADR_20260827-2 §2.5). ChatLog routes a toolcall item here instead
  of to ToolCard.vue when the call's name is a virtual reply_<role>[_<topic>] tool or agent_correspond
  — see isReplyCall() below, which ChatLog imports for that decision.

  The delivery receipt (the tool's actual result text — "delivered: ..." or "error: ...") is
  deliberately never shown as content: plan trap 10 is exactly a model mistaking a receipt for the
  correspondent's answer, and showing the raw string here would teach the READER the same confusion.
  Only a terse ✓/✗ status derived from it is shown — status, not content.
-->
<template>
  <div class="msg reply-out">
    <div class="reply-out-label">
      → to {{ target.role }}<span v-if="target.topic">&nbsp;({{ target.topic }})</span>
      <span v-if="call.status === 'running'" class="reply-out-status">sending…</span>
      <span v-else class="reply-out-status" :class="delivered ? 'ok' : 'failed'">
        {{ delivered ? "✓ delivered" : "✗ " + failureReason }}
      </span>
    </div>
    <div class="body plain">{{ text }}</div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { ToolCallState } from "./ToolCard.vue";

const props = defineProps<{ call: ToolCallState }>();

const args = computed<Record<string, unknown>>(() => {
  try { return JSON.parse(props.call.argumentsText || "{}"); } catch { return {}; }
});

/** Where this call is speaking to. agent_correspond carries role/topic explicitly in its arguments;
 *  a reply_<role>[_<topic>] tool bakes them into its own (normalized, lossy) name instead — see
 *  ReplyToolNaming.cs — so the name is all there is to read for that case. */
const target = computed<{ role: string; topic?: string }>(() => {
  if (props.call.name === "agent_correspond") {
    return { role: String(args.value.role ?? "?"), topic: args.value.topic ? String(args.value.topic) : undefined };
  }
  const rest = props.call.name.replace(/^reply_/, "");
  return { role: rest.replace(/_/g, " ") || "?" };
});

const text = computed(() => String(args.value.text ?? ""));

/** The receipt's own outcome word (ChatToolHost.ExecuteReply / ChatRuntime.Correspond): "delivered: "
 *  on success, "error: <reason>" otherwise. Read only for this ✓/✗ — never displayed verbatim. */
const delivered = computed(() => (props.call.result ?? "").trimStart().toLowerCase().startsWith("delivered"));
const failureReason = computed(() => {
  const r = (props.call.result ?? "").trim();
  const m = /^error:\s*(.*)$/i.exec(r);
  return m?.[1] || "not delivered";
});
</script>
