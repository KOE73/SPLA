<!--
  A hole in this panel that the HOST fills with its credential control (pick an existing secret-store
  entry or create one). This plugin deliberately knows nothing about the secret protocol: it hands
  over an element and receives back a `secret:<scope>:<key>` reference. Everything that used to live
  here — the entry list, the inline "new credential" form, the scope picker, the error handling —
  is the host's single implementation now.

  The fallback input matters: the module is shipped inside the plugin and may run against an older
  host that has no such API. Then the reference is still editable by hand rather than unreachable.
-->
<template>
  <div ref="el" class="cred-slot">
    <input v-if="!mounted" :value="modelValue" class="w-260" spellcheck="false"
           placeholder="secret:<scope>:<entry>"
           @input="emit('update:modelValue', ($event.target as HTMLInputElement).value)">
  </div>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref, watch } from "vue";
import type { CredentialFieldHandle, MountApi } from "./mount";

const props = defineProps<{ api: MountApi; modelValue: string }>();
const emit = defineEmits<{ (e: "update:modelValue", reference: string): void }>();

const el = ref<HTMLElement | null>(null);
const mounted = ref(false);
let handle: CredentialFieldHandle | null = null;

onMounted(() => {
  if (!el.value || !props.api.mountCredentialField) return;
  handle = props.api.mountCredentialField(el.value, {
    value: props.modelValue,
    noneLabel: "(none — use fields below)",
    onChange: reference => emit("update:modelValue", reference)
  });
  mounted.value = true;
});

watch(() => props.modelValue, v => handle?.setValue(v));
onBeforeUnmount(() => handle?.destroy());
</script>

<style scoped>
.cred-slot { min-width: 0; flex: 1; }
.w-260 { width: 260px; }
</style>
