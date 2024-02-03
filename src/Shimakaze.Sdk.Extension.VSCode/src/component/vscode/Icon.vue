<template>
  <svg>
    <use :xlink:href="`${codicon}#${icon}`" />
  </svg>
</template>

<script lang="ts" setup>
import codicon from '@vscode/codicons/dist/codicon.svg'

defineProps<{
  icon: string
}>()
</script>
