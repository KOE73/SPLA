<template>
  <div class="ask">
    <div class="q">Allow tool: <b>{{ toolName }}</b>?</div>
    <pre v-if="argumentsText">{{ argumentsText }}</pre>
    <div class="opts">
      <button class="btn" @click="$emit('decide', requestId, 'allowOnce')">Allow once</button>
      <button class="btn" @click="$emit('decide', requestId, 'allowRemember')">Allow &amp; remember</button>
      <button class="btn ghost" @click="$emit('decide', requestId, 'deny')">Deny</button>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{ requestId: string; toolName: string; argumentsText?: string }>();
defineEmits<{ decide: [requestId: string, decision: string] }>();
</script>
