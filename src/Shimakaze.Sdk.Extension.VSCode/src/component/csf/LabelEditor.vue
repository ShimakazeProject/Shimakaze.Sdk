<template>
  <TextField
    class="csf-label-editor"
    v-model:value="label"
    placeholder="在此处输入CSF标签">
    CSF标签
  </TextField>
</template>

<script lang="ts" setup>
import TextField from '@shimakaze.sdk/webview/component/vscode/TextField.vue'
import { computed } from 'vue'

export interface LabelEditorProps {
  label?: string
}

export interface LabelEditorEmits {
  (e: 'update:label', ev: string): void
}

const props = defineProps<LabelEditorProps>()
const emits = defineEmits<LabelEditorEmits>()

const label = computed({
  get: () => props.label ?? '',
  set: v => emits('update:label', v),
})
</script>

<style lang="scss" scoped>
.csf-label-editor {
  width: 100%;
}
</style>
