<template>
  <div class="control-bar">
    <Button appearance="icon">
      <Icon icon="debug-reverse-continue" />
    </Button>
    <PlayButton
      :is-play="isPlay"
      @update:is-play="$emit('update:isPlay', $event)" />
    <Button appearance="icon">
      <Icon icon="debug-continue" />
    </Button>

    <input
      type="range"
      :max="max"
      :min="min"
      :step="step"
      :value="current ?? 0"
      @input="onInput" />

    <TextField
      class="frame-input"
      type="number"
      :value="current ?? 0"
      @update:value="$emit('update:current', Number($event))" />
  </div>
</template>

<script lang="ts" setup>
import PlayButton from '@shimakaze.sdk/webview/component/common/PlayButton.vue'
import Button from '@shimakaze.sdk/webview/component/vscode/Button.vue'
import Icon from '@shimakaze.sdk/webview/component/vscode/Icon.vue'
import TextField from '../vscode/TextField.vue'

export interface PlayControlProps {
  isPlay: boolean
  current: number
  min?: number
  max?: number
  step?: number
}

export interface PlayControlEmits {
  (e: 'update:isPlay', ev: boolean): void
  (e: 'update:current', ev: number): void
}

withDefaults(defineProps<PlayControlProps>(), {
  max: 10,
  min: 0,
  step: 1,
})
const emits = defineEmits<PlayControlEmits>()

const onInput = (e: Event) => {
  const value = Number((e.target as any)?.value ?? 0)
  emits('update:current', value)
}
</script>

<style lang="scss" scoped>
.control-bar {
  display: flex;
  flex-direction: row;
  gap: 0.5rem;
  .frame-input {
    width: min-content;
  }
}
</style>
