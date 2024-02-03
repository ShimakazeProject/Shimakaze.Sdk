<template>
  <vscode-panel-view v-bind="$attrs" :id="panelId">
    <slot />
  </vscode-panel-view>
</template>

<script lang="ts" setup>
import {
  provideVSCodeDesignSystem,
  vsCodePanelView,
} from '@vscode/webview-ui-toolkit'
import { onMounted } from 'vue'

defineProps<{
  panelId: string
}>()
onMounted(() => provideVSCodeDesignSystem().register(vsCodePanelView()))
</script>
