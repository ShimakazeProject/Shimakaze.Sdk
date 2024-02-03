<template>
  <div class="control-bar">
    <Button
      appearance="icon"
      :disabled="current <= min"
      @click="$emit('update:current', current - 1)">
      <Icon icon="debug-reverse-continue" />
    </Button>
    <PlayButton
      :is-play="isPlay"
      @update:is-play="$emit('update:isPlay', $event)" />
    <Button
      appearance="icon"
      :disabled="current >= maxValue"
      @click="$emit('update:current', current + 1)">
      <Icon icon="debug-continue" />
    </Button>

    <input
      type="range"
      :max="maxValue"
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
import { computed, watch } from 'vue'

export interface PlayControlProps {
  isPlay: boolean
  current: number
  min?: number
  max?: number
  step?: number
  hasShadow?: boolean
}

export interface PlayControlEmits {
  (e: 'update:isPlay', ev: boolean): void
  (e: 'update:current', ev: number): void
}

const props = withDefaults(defineProps<PlayControlProps>(), {
  max: 10,
  min: 0,
  step: 1,
})
const emits = defineEmits<PlayControlEmits>()

const maxValue = computed(() => {
  let value = props.max
  if (props.hasShadow) value /= 2
  return value
})

let taskId: number
watch(
  () => props.isPlay,
  v => {
    if (v) {
      taskId = setInterval(() => {
        const value = props.current + 1
        if (value >= maxValue.value) emits('update:current', 0)
        emits('update:current', value)
      }, 100)
    } else {
      clearInterval(taskId)
    }
  },
)

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
