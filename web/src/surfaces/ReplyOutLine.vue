<!--
  One outgoing reply/correspondence call, rendered as speech ("→ to <role>") instead of a generic
  ToolCard (PLAN_20260902 wave 7; ADR_20260827-2 §2.5; PLAN_20260906 wave 0). ChatLog routes a
  toolcall item here instead of to ToolCard.vue when the call's name is a virtual
  reply_<role>[_<n>] tool or agent_correspond — see isReplyCall() below, which ChatLog imports for
  that decision.

  The delivery receipt (the tool's actual result text — "delivered: ..." or "error: ...") is
  deliberately never shown as content: plan trap 10 is exactly a model mistaking a receipt for the
  correspondent's answer, and showing the raw string here would teach the READER the same confusion.
  Only a terse ✓/✗ status derived from it is shown — status, not content.
-->
<template>
  <div class="msg reply-out">
    <div class="reply-out-label">
      → to {{ target.role }}<span v-if="target.instance">&nbsp;#{{ target.instance }}</span><span v-if="target.purpose">&nbsp;({{ target.purpose }})</span>
      <span v-if="call.status === 'running'" class="reply-out-status">{{ t('sending…') }}</span>
      <span v-else class="reply-out-status" :class="delivered ? 'ok' : 'failed'">
        {{ delivered ? "✓ delivered" : "✗ " + failureReason }}
      </span>
    </div>
    <div class="body plain">{{ text }}</div>
  </div>
</template>

<script setup lang="ts">
import { t } from "../i18n";
import { computed } from "vue";
import type { ToolCallState } from "./ToolCard.vue";

const props = defineProps<{ call: ToolCallState }>();

const args = computed<Record<string, unknown>>(() => {
  try { return JSON.parse(props.call.argumentsText || "{}"); } catch { return {}; }
});

/** Where this call is speaking to. agent_correspond carries role/purpose explicitly in its arguments
 *  ('topic' is accepted server-side as an older name for the same field — read as a fallback here
 *  too); a reply_<role>[_<n>] tool bakes only the system-issued instance number into its own name
 *  (PLAN_20260906 wave 0 §2.1/2.3 — purpose never joins the name any more) — see ReplyToolNaming.cs. */
const target = computed<{ role: string; instance?: number; purpose?: string }>(() => {
  if (props.call.name === "agent_correspond") {
    return {
      role: String(args.value.role ?? "?"),
      purpose: args.value.purpose ? String(args.value.purpose) : args.value.topic ? String(args.value.topic) : undefined
    };
  }
  const rest = props.call.name.replace(/^reply_/, "");
  const m = /^(.*)_(\d+)$/.exec(rest);
  return m ? { role: m[1].replace(/_/g, " "), instance: Number(m[2]) } : { role: rest.replace(/_/g, " ") || "?" };
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
