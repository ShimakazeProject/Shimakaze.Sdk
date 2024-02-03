<template>
  <Button v-if="isPlay" appearance="icon" @click="onPause">
    <Icon icon="debug-pause" />
  </Button>
  <Button v-else appearance="icon" @click="onPlay">
    <Icon icon="debug-start" />
  </Button>
</template>

<script lang="ts" setup>
import Button from '@shimakaze.sdk/webview/component/vscode/Button.vue'
import Icon from '@shimakaze.sdk/webview/component/vscode/Icon.vue'

export interface PlayButtonProps {
  isPlay: boolean
}

export interface PlayButtonEmits {
  (e: 'update:isPlay', ev: boolean): void
  (e: 'play'): void
  (e: 'pause'): void
}

defineProps<PlayButtonProps>()
const emits = defineEmits<PlayButtonEmits>()

const onPlay = () => {
  emits('update:isPlay', true)
  emits('play')
}
const onPause = () => {
  emits('update:isPlay', false)
  emits('pause')
}
</script>
