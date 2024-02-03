<template>
  <vscode-badge>
    <slot />
  </vscode-badge>
</template>

<script lang="ts" setup>
import {
  provideVSCodeDesignSystem,
  vsCodeBadge,
} from '@vscode/webview-ui-toolkit'
import { onMounted } from 'vue'

onMounted(() => provideVSCodeDesignSystem().register(vsCodeBadge()))
</script>
