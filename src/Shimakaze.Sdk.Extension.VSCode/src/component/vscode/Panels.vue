<template>
  <vscode-panels v-bind="$attrs" :activeid="activeId" @change="onChange">
    <div slot="start">
      <slot name="start" />
    </div>
    <slot />
    <div slot="end">
      <slot name="end" />
    </div>
  </vscode-panels>
</template>

<script lang="ts" setup>
import {
  provideVSCodeDesignSystem,
  vsCodePanels,
  Panels,
  PanelTab,
} from '@vscode/webview-ui-toolkit'
import { onMounted } from 'vue'

interface PanelsChangeEvent extends CustomEvent {
  detail: PanelTab
  srcElement: Panels
  target: Panels
}

defineProps<{
  activeId?: string
}>()

const emits = defineEmits<{
  (e: 'update:activeId', ev: string): void
}>()

const onChange = (e: PanelsChangeEvent) => {
  emits('update:activeId', e.detail.id)
  console.log()
}

onMounted(() => provideVSCodeDesignSystem().register(vsCodePanels()))
</script>
