<template>
  <div class="ask">
    <div class="q">{{ t('Allow tool:') }} <b>{{ toolName }}</b>?</div>
    <pre v-if="argumentsText">{{ argumentsText }}</pre>
    <div class="opts">
      <button class="btn" @click="$emit('decide', requestId, 'allowOnce')">{{ t('Allow once') }}</button>
      <button class="btn" @click="$emit('decide', requestId, 'allowRemember')">{{ t('Allow & remember') }}</button>
      <button class="btn ghost" @click="$emit('decide', requestId, 'deny')">{{ t('Deny') }}</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { t } from "../i18n";
defineProps<{ requestId: string; toolName: string; argumentsText?: string }>();
defineEmits<{ decide: [requestId: string, decision: string] }>();
</script>
