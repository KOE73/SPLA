<template>
  <div class="android-settings">
    <section v-for="component in components" :key="component">
      <h3>{{ component === 'adb' ? 'ADB' : 'scrcpy' }}</h3>
      <label>{{ t('Component folder') }}
        <input v-model="form[component + '_path']" :placeholder="status?.[component].defaultFolder" :disabled="busy(component)" @change="refresh" spellcheck="false">
      </label>
      <p v-if="status">{{ t(status[component].installed ? 'Installed' : 'Not installed') }}
        <template v-if="status[component].installed"> · {{ status[component].version }} · {{ status[component].channel }} · {{ t(status[component].verified ? 'Verified' : 'Not verified') }}</template>
        <span v-if="status[component].experimental"> · {{ t('Experimental version') }}</span>
      </p>
      <p v-if="status?.[component].folder" class="folder">{{ status[component].folder }}</p>
      <p v-if="status?.[component].error" role="alert">{{ status[component].error }}</p>
      <div class="actions">
        <button :disabled="!status || busy(component) || !!status[component].error" @click="install(component, 'pinned')">{{ t('Install') }} {{ status?.pinned[component] }}</button>
        <button :disabled="!status || busy(component) || !!status[component].error" @click="install(component, 'latest')">{{ t('Install latest (experimental)') }}</button>
      </div>
      <p v-if="status?.[component].job" aria-live="polite">
        {{ t(status[component].job!.stage) }} · {{ megabytes(status[component].job!.bytesDone) }} MB
        <template v-if="status[component].job!.bytesTotal"> / {{ megabytes(status[component].job!.bytesTotal!) }} MB</template>
        {{ status[component].job!.error }}
      </p>
    </section>
    <p v-if="error" role="alert">{{ error }}</p>
    <button @click="refresh" :disabled="refreshing">{{ t('Refresh') }}</button>
    <section>
      <h3>{{ t('Stream') }}</h3>
      <label v-for="field in numbers" :key="field.key">{{ t(field.label) }}
        <input type="number" v-model.number="form[field.key]" :min="field.min" :max="field.max">
      </label>
      <label><input type="checkbox" v-model="form.stay_awake"> {{ t('Keep screen awake') }}</label>
      <label><input type="checkbox" v-model="form.screenshot_after_action"> {{ t('Screenshot after action') }}</label>
    </section>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, reactive, ref } from 'vue';
import type { MountApi } from './mount';
import { t } from './i18n';
const props = defineProps<{ api: MountApi }>();
type Component = 'adb' | 'scrcpy';
interface Job { stage: string; bytesDone: number; bytesTotal?: number; error?: string }
interface ComponentStatus { folder?: string; defaultFolder: string; installed: boolean; version: string; channel: string; verified: boolean; experimental: boolean; error?: string; job?: Job }
type Status = Record<Component, ComponentStatus> & { pinned: Record<Component, string> };
const components: Component[] = ['adb', 'scrcpy'];
const form = reactive<Record<string, string | number | boolean>>({ adb_path: '', scrcpy_path: '', adb_server_port: 0, max_size: 1280, max_fps: 30, video_bit_rate: 8000000, stay_awake: true, screenshot_after_action: true, settle_ms: 300, settle_timeout_ms: 3000, idle_disconnect_minutes: 15 });
const error = ref('');
try { Object.assign(form, JSON.parse(props.api.getJson() || '{}')); } catch (e) { error.value = String(e); }
const numbers = [
  { key: 'max_size', label: 'Maximum size (0 = native)', min: 0, max: 4096 },
  { key: 'max_fps', label: 'Maximum FPS', min: 1, max: 120 },
  { key: 'video_bit_rate', label: 'Bit rate', min: 500000, max: 100000000 },
  { key: 'settle_ms', label: 'Settle time (ms)', min: 0, max: 5000 },
  { key: 'settle_timeout_ms', label: 'Settle timeout (ms)', min: 0, max: 30000 },
  { key: 'idle_disconnect_minutes', label: 'Idle disconnect (minutes)', min: 1, max: 1440 },
  { key: 'adb_server_port', label: 'ADB server port (0 = default)', min: 0, max: 65535 }
];
const status = ref<Status>(); const refreshing = ref(false); const starting = reactive<Record<Component, boolean>>({ adb: false, scrcpy: false });
let timer: ReturnType<typeof setTimeout> | undefined; let disposed = false;
const megabytes = (bytes: number) => (bytes / 1048576).toFixed(1);
function busy(component: Component) { const stage = status.value?.[component].job?.stage; return starting[component] || (!!stage && stage !== 'done' && stage !== 'failed'); }
async function invoke<R>(action: string, extra = {}): Promise<R> {
  const result = await props.api.invoke<{ ok: boolean; resultJson?: string; error?: string }>('plugin.action', { pluginId: 'android', action, valueJson: JSON.stringify({ ...form, ...extra }) });
  if (!result.ok || !result.resultJson) throw new Error(result.error || 'Android action failed');
  return JSON.parse(result.resultJson) as R;
}
async function refresh() {
  if (refreshing.value || disposed) return;
  clearTimeout(timer); refreshing.value = true;
  try { status.value = await invoke<Status>('runtimeStatus'); error.value = ''; }
  catch (e) { error.value = e instanceof Error ? e.message : String(e); }
  finally {
    refreshing.value = false;
    if (!disposed && components.some(busy)) timer = setTimeout(refresh, 1000);
  }
}
async function install(component: Component, channel: string) {
  starting[component] = true;
  try { const job = await invoke<Job>('install', { component, channel }); if (status.value) status.value[component].job = job; error.value = ''; }
  catch (e) { error.value = e instanceof Error ? e.message : String(e); }
  finally { starting[component] = false; if (!disposed) timer = setTimeout(refresh, 1000); }
}
function toJson() { return JSON.stringify(form); }
defineExpose({ toJson });
onMounted(refresh);
onUnmounted(() => { disposed = true; clearTimeout(timer); });
</script>

<style scoped>
.android-settings { display: grid; gap: 12px; }
section { display: grid; gap: 8px; }
h3, p { margin: 0; }
label { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
input:not([type=checkbox]) { flex: 1; min-width: 120px; }
.actions { display: flex; gap: 8px; flex-wrap: wrap; }
.folder { overflow-wrap: anywhere; opacity: .7; }
</style>
