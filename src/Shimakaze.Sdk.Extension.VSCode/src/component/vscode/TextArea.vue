<template>
  <vscode-text-area
    v-bind="$attrs"
    :autofocus="autofocus"
    :cols="cols"
    :form="form"
    :maxlength="maxlength"
    :name="name"
    :placeholder="placeholder"
    :readonly="readonly"
    :resize="resize"
    :rows="rows"
    :value="value"
    @compositionstart="onCompositionStart"
    @compositionend="onCompositionEnd"
    @input="onInput">
    <slot />
  </vscode-text-area>
</template>

<script lang="ts" setup>
import type { TextField } from '@vscode/webview-ui-toolkit'
import {
  provideVSCodeDesignSystem,
  vsCodeTextArea,
} from '@vscode/webview-ui-toolkit'
import { onMounted, ref } from 'vue'

interface TextFieldInputEvent extends InputEvent {
  srcElement: TextField
  target: TextField
}

defineProps<{
  /** Indicates that this component should get focus after the page finishes loading. */
  autofocus?: boolean
  /** Sizes the component horizontally by a number of character columns. Defaults to 20. */
  cols?: number
  /** The id of the form that the component is associated with. */
  form?: string
  /** The maximum number of characters a user can enter. */
  maxlength?: number
  /** The name of the component. */
  name?: string
  /** Sets the placeholder value of the component, generally used to provide a hint to the user. */
  placeholder?: string
  /** When true, the control will be immutable by any user interaction. */
  readonly?: boolean
  /** The resize mode of the component. Options: none, vertical, horizontal, both. */
  resize?: string
  /** Sizes the component vertically by a number of character rows. */
  rows?: number
  /** The value (i.e. content) of the text field. */
  value?: string
}>()

const emits = defineEmits<{
  (e: 'update:value', ev: string): void
}>()

const isComposing = ref(false)
const onCompositionStart = () => {
  isComposing.value = true
}
const onCompositionEnd = (e: TextFieldInputEvent) => {
  isComposing.value = false
  onInput(e)
}

const onInput = (e: TextFieldInputEvent) => {
  if (isComposing.value) return

  emits('update:value', e.target.value)
}

onMounted(() => provideVSCodeDesignSystem().register(vsCodeTextArea()))
</script>
