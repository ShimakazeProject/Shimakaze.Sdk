<template>
  <vscode-button
    v-bind="$attrs"
    :appearance="appearance"
    :aria-label="ariaLabel"
    :autofocus="autofocus"
    :form="form"
    :formaction="formaction"
    :formenctype="formenctype"
    :formmethod="formmethod"
    :formnovalidate="formnovalidate"
    :formtarget="formtarget"
    :name="name"
    :type="type"
    :value="value"
    @click="$emit('click', $event)">
    <slot />
  </vscode-button>
</template>

<script lang="ts" setup>
import {
  provideVSCodeDesignSystem,
  vsCodeButton,
} from '@vscode/webview-ui-toolkit'
import { onMounted } from 'vue'

defineProps<{
  appearance?: 'primary' | 'secondary' | 'icon'
  ariaLabel?: string
  autofocus?: boolean
  form?: string
  formaction?: string
  formenctype?: string
  formmethod?: string
  formnovalidate?: string
  formtarget?: string
  name?: string
  type?: string
  value?: string
}>()
defineEmits<{
  (e: 'click', ev: Event): void
}>()
onMounted(() => provideVSCodeDesignSystem().register(vsCodeButton()))
</script>
