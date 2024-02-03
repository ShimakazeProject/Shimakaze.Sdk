<template>
  <vscode-text-field
    v-bind="$attrs"
    :autofocus="autofocus"
    :maxlength="maxlength"
    :name="name"
    :placeholder="placeholder"
    :readonly="readonly"
    :type="type"
    :value="value"
    @compositionstart="onCompositionStart"
    @compositionend="onCompositionEnd"
    @input="onInput">
    <slot />
  </vscode-text-field>
</template>

<script lang="ts" setup>
import type { TextField } from '@vscode/webview-ui-toolkit'
import {
  provideVSCodeDesignSystem,
  vsCodeTextField,
} from '@vscode/webview-ui-toolkit'
import { onMounted, ref } from 'vue'

interface TextFieldInputEvent extends InputEvent {
  srcElement: TextField
  target: TextField
}

const props = defineProps<{
  /** Indicates that this component should get focus after the page finishes loading. */
  autofocus?: boolean
  /** The maximum number of characters a user can enter. */
  maxlength?: number
  /** The name of the component. */
  name?: string
  /** Sets the placeholder value of the component, generally used to provide a hint to the user. */
  placeholder?: string
  /** When true, the control will be immutable by any user interaction. */
  readonly?: boolean
  /** Sets the text field type. */
  type?: string
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

const value = ref(props.value ?? '')
const onInput = (e: TextFieldInputEvent) => {
  if (isComposing.value) return

  value.value = e.target.value
  emits('update:value', value.value)
}

onMounted(() => provideVSCodeDesignSystem().register(vsCodeTextField()))
</script>
