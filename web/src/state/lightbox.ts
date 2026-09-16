/**
 * The image viewer's state — one viewer for the whole window, opened by whoever shows a picture.
 *
 * A module-level state rather than a component each surface mounts: a chat bubble, a tool card and
 * the debug panel all open the SAME viewer, so there is exactly one set of keyboard handlers and one
 * layer on top, and no surface has to know how the viewer is built. `ImageLightbox.vue` (mounted once
 * in LayoutHost) renders whatever is here.
 */
import { reactive } from "vue";
import type { ImageRef } from "../protocol/types";
import type { ChatSession } from "./chatSessions";

export const lightbox = reactive({
  /** The pictures the viewer steps through, in order. Empty = closed. */
  images: [] as ImageRef[],
  index: 0
});

/** Opens the viewer on `images[index]`, with the rest reachable by the arrow keys. */
export function openLightbox(images: ImageRef[], index = 0) {
  if (!images.length) return;
  lightbox.images = images.slice();
  lightbox.index = Math.min(Math.max(index, 0), images.length - 1);
}

export function closeLightbox() {
  lightbox.images = [];
  lightbox.index = 0;
}

/** Every picture of a chat's log in reading order: attached to a message, or returned by a tool. */
export function chatImages(s: ChatSession | undefined): ImageRef[] {
  const out: ImageRef[] = [];
  for (const it of s?.items ?? []) {
    if (it.kind === "user" && it.images) out.push(...it.images);
    else if (it.kind === "toolcall" && it.call.images) out.push(...it.call.images);
  }
  return out;
}

/** Opens one picture of a chat, stepping through all of the chat's pictures from there. Found by
 *  identity first and by address second — a reactive proxy and its raw object are different
 *  references — and shown alone if the log does not hold it at all. */
export function openChatImage(s: ChatSession | undefined, image: ImageRef) {
  const all = chatImages(s);
  let at = all.indexOf(image);
  if (at < 0) at = all.findIndex(i => i.url === image.url && i.label === image.label);
  if (at < 0) openLightbox([image], 0);
  else openLightbox(all, at);
}
