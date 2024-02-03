<template>
  <vscode-option v-bind="$attrs" :value="value">
    <slot v-if="$slots.default" />
    <span v-else v-text="value" />
  </vscode-option>
</template>

<script lang="ts" setup>
import {
  provideVSCodeDesignSystem,
  vsCodeOption,
} from '@vscode/webview-ui-toolkit'
import { onMounted } from 'vue'

defineProps<{
  value: string | number
}>()

onMounted(() => provideVSCodeDesignSystem().register(vsCodeOption()))
</script>
