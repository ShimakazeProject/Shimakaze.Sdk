<template>
  <div class="csf-value-editor">
    <TextArea
      class="csf-value-editor-value"
      :value="value"
      @update:value="$emit('update:value', $event)"
      resize="vertical"
      :rows="valueRows"
      placeholder="在此处输入字符串值">
      字符串值
    </TextArea>
    <TextField
      class="csf-value-editor-extra"
      :value="extra"
      @update:value="$emit('update:extra', $event)"
      placeholder="在此处输入额外值">
      额外值
    </TextField>
  </div>
</template>

<script lang="ts" setup>
import TextArea from '@shimakaze.sdk/webview/component/vscode/TextArea.vue'
import TextField from '@shimakaze.sdk/webview/component/vscode/TextField.vue'

export interface ValueEditorProps {
  value?: string
  extra?: string
  valueRows?: number
}

export interface ValueEditorEmits {
  (e: 'update:value', ev: string): void
  (e: 'update:extra', ev: string): void
}

withDefaults(defineProps<ValueEditorProps>(), {
  valueRows: 3,
})
const emits = defineEmits<ValueEditorEmits>()
</script>

<style lang="scss" scoped>
.csf-value-editor {
  display: flex;
  width: 100%;
  flex-direction: column;
  .csf-value-editor-value {
    flex-grow: 1;
  }
}
</style>
