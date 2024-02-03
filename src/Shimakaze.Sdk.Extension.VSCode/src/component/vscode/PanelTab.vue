<template>
  <vscode-panel-tab v-bind="$attrs" :id="panelId">
    <slot />
  </vscode-panel-tab>
</template>

<script lang="ts" setup>
import {
  provideVSCodeDesignSystem,
  vsCodePanelTab,
} from '@vscode/webview-ui-toolkit'
import { onMounted } from 'vue'

defineProps<{
  panelId: string
}>()
onMounted(() => provideVSCodeDesignSystem().register(vsCodePanelTab()))
</script>
