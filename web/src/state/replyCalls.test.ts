/**
 * isReplyCall (replyCalls.ts) — the one bit ChatLog needs to route a tool-call item to
 * ReplyOutLine.vue ("→ to <role>") instead of the generic ToolCard.vue (PLAN_20260902 wave 7).
 * Wrong here means either a real reply rendering as a raw tool card, or an unrelated tool call
 * mistakenly rendered as speech.
 */
import { describe, expect, it } from "vitest";
import { isReplyCall } from "./replyCalls";

describe("isReplyCall", () => {
  it("recognizes a bare role reply tool", () => {
    expect(isReplyCall("reply_architect")).toBe(true);
  });

  it("recognizes a reply tool with a topic suffix", () => {
    expect(isReplyCall("reply_architect_design_review")).toBe(true);
  });

  it("recognizes agent_correspond", () => {
    expect(isReplyCall("agent_correspond")).toBe(true);
  });

  it("rejects an unrelated tool", () => {
    expect(isReplyCall("agent_spawn")).toBe(false);
    expect(isReplyCall("system_run_shell")).toBe(false);
    expect(isReplyCall("fs_read")).toBe(false);
  });

  it("rejects a name that merely contains 'reply' without the prefix", () => {
    expect(isReplyCall("no_reply_here")).toBe(false);
  });

  it("rejects a name that only partially matches agent_correspond", () => {
    expect(isReplyCall("agent_correspondence")).toBe(false);
    expect(isReplyCall("agent_correspond_extra")).toBe(false);
  });

  it("is case sensitive — the wire always sends lowercase tool names", () => {
    expect(isReplyCall("Reply_architect")).toBe(false);
    expect(isReplyCall("AGENT_CORRESPOND")).toBe(false);
  });

  it("rejects the empty string", () => {
    expect(isReplyCall("")).toBe(false);
  });
});
