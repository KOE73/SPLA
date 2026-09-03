/**
 * Small pure helpers over the chat list's tree shape (`ChatSummary.children` — PLAN_20260902 wave 7,
 * ADR_20260827-2 §2.5's "дерево роль → чат"). Shared by ChatList.vue's tree render, its
 * ui.auto_open_subagents watcher, and SessionsPanel.vue's flat listing, so "what counts as a spawned
 * session" and "how to walk the tree" are answered in exactly one place.
 */
import type { ChatSummary } from "../protocol/types";

/** Every spawned session (`origin === "spawned"`) anywhere in the tree, flattened, depth-first. */
export function collectSpawned(list: ChatSummary[], out: ChatSummary[] = []): ChatSummary[] {
  for (const c of list) {
    if (c.origin === "spawned") out.push(c);
    if (c.children?.length) collectSpawned(c.children, out);
  }
  return out;
}

/** The title of one chat anywhere in the tree, or the id itself when the chat is not found (a parent
 *  that has since been deleted — the same "orphan" outcome the flat list always allowed). */
export function titleOf(list: ChatSummary[], id: string): string {
  for (const c of list) {
    if (c.id === id) return c.title || c.id;
    if (c.children?.length) {
      const found = titleOf(c.children, id);
      if (found !== id) return found;
    }
  }
  return id;
}

/** One chat anywhere in the tree by id, or undefined. What a surface needs to ask about a chat it is
 *  showing — is this a spawned session? — without the caller learning the tree's shape. */
export function findChat(list: ChatSummary[], id: string): ChatSummary | undefined {
  for (const c of list) {
    if (c.id === id) return c;
    const found = c.children?.length ? findChat(c.children, id) : undefined;
    if (found) return found;
  }
  return undefined;
}
