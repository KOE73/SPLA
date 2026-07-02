<template>
  <div id="attachments">
    <div v-for="(src, i) in store.attachments" :key="i" class="thumb">
      <img :src="src">
      <button class="rm" @click="store.attachments.splice(i, 1)">✕</button>
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
      @paste="onPaste"
    ></textarea>
    <button v-if="!turnActive" class="btn" :disabled="!ready" @click="send">Send</button>
    <button v-else class="btn danger" @click="stop">Stop</button>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onUnmounted, ref } from "vue";
import { client } from "../protocol/SplaClient";
import { store } from "../state/store";
import { uiBus } from "../state/uiBus";

const text = ref("");
const turnActive = ref(false);
const fileInput = ref<HTMLInputElement>();
const textareaEl = ref<HTMLTextAreaElement>();

const ready = computed(() => store.connected && !!store.currentChat && !turnActive.value);

function addImageFiles(files: FileList | File[]) {
  for (const f of files) {
    if (!f.type.startsWith("image/")) continue;
    const reader = new FileReader();
    reader.onload = () => { store.attachments.push(reader.result as string); };
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
  const t = text.value.trim();
  if ((!t && !store.attachments.length) || !store.currentChat) return;
  const images = store.attachments.slice();
  uiBus.emit("local.userMsg", { text: t, images });
  text.value = "";
  turnActive.value = true;
  client.send("chat.send", { chatId: store.currentChat, text: t, images }, { projectId: store.currentProjectId ?? undefined });
  store.attachments = [];
  nextTick(() => textareaEl.value?.focus());
}
function stop() {
  if (store.currentChat) client.send("cancel", null, { chatId: store.currentChat });
}

const offTurn = client.on("turn.complete", () => {
  turnActive.value = false;
  nextTick(() => textareaEl.value?.focus());
});
onUnmounted(offTurn);
</script>
