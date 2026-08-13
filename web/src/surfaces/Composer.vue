<!--
  The composer. Its text, its attachments and its Send/Stop state all belong to the chat it is
  editing — kept in that chat's session, so switching away and back finds the draft where it was, and
  an image attached in one chat can never be sent to another.
-->
<template>
  <div id="attachments">
    <div v-for="(src, i) in attachments" :key="i" class="thumb">
      <img :src="src">
      <button class="rm" @click="attachments.splice(i, 1)">✕</button>
    </div>
  </div>
  <div class="row">
    <button class="icon-btn" title="Attach image" @click="fileInput?.click()">+</button>
    <input ref="fileInput" type="file" accept="image/*" multiple hidden @change="onFileInput">
    <textarea
      id="input"
      ref="textareaEl"
      v-model="text"
      rows="2"
      placeholder="Message…  (Enter to send, Shift+Enter for newline, paste images)"
      :disabled="!ready"
      @keydown.enter.exact.prevent="send"
      @input="autosize"
      @paste="onPaste"
    ></textarea>
    <button v-if="!turnActive" class="btn" :disabled="!ready" @click="send">Send</button>
    <button v-else class="btn danger" @click="stop">Stop</button>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onUnmounted, ref, watch } from "vue";
import { client } from "../protocol/SplaClient";
import { store } from "../state/store";
import { uiBus } from "../state/uiBus";
import { useChat } from "../state/chatContext";
import { addLocalUserMessage } from "../state/chatSessions";

const chat = useChat();
const fileInput = ref<HTMLInputElement>();
const textareaEl = ref<HTMLTextAreaElement>();

/** Draft and attachments live in the session; an absent session simply has nothing to edit. */
const text = computed({
  get: () => chat.session.value?.draft ?? "",
  set: v => { if (chat.session.value) chat.session.value.draft = v; }
});
const attachments = computed(() => chat.session.value?.attachments ?? []);

// The server is the source of truth for "a turn is running": chat.opened carries it, so a window
// attaching mid-turn shows Stop, and turns started elsewhere still disable this input.
const turnActive = computed(() => !!chat.session.value?.turnActive);
const ready = computed(() => store.connected && !!chat.session.value && !turnActive.value);

// ── Growing input ────────────────────────────────────────────────────────────
// Two rows for a one-liner, up to 16, then it scrolls. Chromium does this from CSS alone
// (`field-sizing: content` + max-height in #input), so the manual path below only runs where that
// property is missing — and only on input/paste/programmatic set, never on a timer.
const MAX_ROWS = 16;
const cssHandlesIt = typeof CSS !== "undefined" && CSS.supports?.("field-sizing", "content");

function autosize() {
  if (cssHandlesIt) return;
  const el = textareaEl.value;
  if (!el) return;
  const cs = getComputedStyle(el);
  const line = parseFloat(cs.lineHeight) || 20;
  const chrome = el.offsetHeight - el.clientHeight            // borders
    + parseFloat(cs.paddingTop) + parseFloat(cs.paddingBottom);
  el.style.height = "auto";                                   // shrink first, or it only ever grows
  el.style.height = Math.min(el.scrollHeight, line * MAX_ROWS + chrome) + "px";
}

/** Back to two rows — after a send, and whenever the text is replaced from outside. */
function resetSize() {
  const el = textareaEl.value;
  if (el && !cssHandlesIt) el.style.height = "";
}

// A different chat means a different draft, and therefore a different height.
watch(() => chat.chatId.value, () => nextTick(() => { resetSize(); autosize(); }));

function addImageFiles(files: FileList | File[]) {
  for (const f of files) {
    if (!f.type.startsWith("image/")) continue;
    const reader = new FileReader();
    // Resolve the target chat now, not in the callback: the read is async and the user may well have
    // switched chats before it finishes.
    const target = chat.session.value;
    reader.onload = () => { target?.attachments.push(reader.result as string); };
    reader.readAsDataURL(f);
  }
}
function onFileInput(e: Event) {
  const input = e.target as HTMLInputElement;
  if (input.files) addImageFiles(input.files);
  input.value = "";
}
function onPaste(e: ClipboardEvent) {
  const imgs = [...(e.clipboardData?.items || [])]
    .filter(i => i.type.startsWith("image/"))
    .map(i => i.getAsFile())
    .filter((f): f is File => !!f);
  if (imgs.length) { e.preventDefault(); addImageFiles(imgs); }
}

function send() {
  const s = chat.session.value;
  if (!s) return;
  const t = s.draft.trim();
  if (!t && !s.attachments.length) return;

  const images = s.attachments.slice();
  addLocalUserMessage(s, t, images);
  s.draft = "";
  s.attachments = [];
  s.turnActive = true;
  chat.send("chat.send", { text: t, images });
  nextTick(() => { resetSize(); textareaEl.value?.focus(); });
}

function stop() {
  const id = chat.chatId.value;
  if (id) client.send("cancel", null, { chatId: id });
}

// Rewind pulls the removed user message back into the composer for editing.
const offComposerSet = uiBus.on("composer.set", p => {
  if (chat.session.value) chat.session.value.draft = (p as { text: string }).text;
  nextTick(() => { resetSize(); autosize(); textareaEl.value?.focus(); });
});

// The input becoming usable again is the moment to put the caret back in it.
watch(ready, on => { if (on) nextTick(() => textareaEl.value?.focus()); });

onUnmounted(offComposerSet);
</script>
