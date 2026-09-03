/**
 * Recognizing a virtual reply/correspondence tool call by name — the one bit ChatLog needs to route a
 * "toolcall" item to ReplyOutLine.vue instead of the generic ToolCard.vue (PLAN_20260902 wave 7).
 *
 * A plain function rather than something exported from ReplyOutLine.vue's own <script setup>: keeping
 * it here means ChatLog's routing decision and the component that acts on it share one definition
 * without either importing the other's SFC internals.
 */

/** True for the virtual reply_<role>[_<topic>] tools (ChatRuntime.SendReply) and agent_correspond
 *  (its first message on a fresh address) — both ultimately the same edge, source→sink
 *  (ADR_20260827-2 §2.2), and both meant to render as speech ("→ to <role>"), not a generic tool
 *  card, with the delivery receipt kept out of view as content (plan trap 10). */
export function isReplyCall(name: string): boolean {
  return name === "agent_correspond" || name.startsWith("reply_");
}
