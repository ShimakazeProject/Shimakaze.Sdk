<template>
  <vscode-dropdown v-bind="$attrs" :position="position">
    <slot />
  </vscode-dropdown>
</template>

<script lang="ts" setup>
import {
  provideVSCodeDesignSystem,
  vsCodeDropdown,
} from '@vscode/webview-ui-toolkit'
import { onMounted } from 'vue'

defineProps<{
  position?: 'above' | 'below'
}>()
onMounted(() => {
  const res = provideVSCodeDesignSystem().register(vsCodeDropdown())
  console.log(res)
})
</script>
