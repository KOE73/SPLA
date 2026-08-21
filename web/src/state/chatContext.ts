/**
 * The chat a component belongs to, provided by ChatSurface and injected by everything under it.
 *
 * One owner, one id. Before this, the log, the composer and the status line each read the global
 * "current chat" for themselves and each stamped `chatId` onto every command by hand — a dozen
 * call sites that could disagree, and did: focus could move between reading the id and sending the
 * command, so a rewind could land in a chat the user never looked at.
 *
 * Consumers therefore get no way to name a chat: `send` fills in the ids from the surface that owns
 * them, and `session` is already the right one.
 */
import { inject, provide, type ComputedRef, type InjectionKey } from "vue";
import type { ChatSession } from "./chatSessions";

export interface ChatContext {
  /** The chat this subtree renders, or null when no chat is open. */
  chatId: ComputedRef<string | null>;
  session: ComputedRef<ChatSession | undefined>;
  /** Sends a chat-scoped command for THIS chat. No caller passes a chat id. */
  send(type: string, payload?: Record<string, unknown>): void;
}

const KEY: InjectionKey<ChatContext> = Symbol("spla.chat");

export function provideChat(ctx: ChatContext) {
  provide(KEY, ctx);
}

export function useChat(): ChatContext {
  const ctx = inject(KEY);
  if (!ctx) throw new Error("useChat() outside a chat surface — the component must live under ChatSurface.");
  return ctx;
}
