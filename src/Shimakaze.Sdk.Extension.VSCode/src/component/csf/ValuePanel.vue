<template>
  <PanelTab class="value-panel-tab" :panel-id="`tab-${index}`">
    第 {{ index + 1 }} 个值
    <Button
      class="remove-button"
      appearance="icon"
      @click.stop="$emit('remove')">
      <Icon icon="close" />
    </Button>
  </PanelTab>
  <PanelView :panel-id="`view-${index}`">
    <ValueEditor
      :value="value"
      :extra="extra"
      @update:value="$emit('update:value', $event)"
      @update:extra="$emit('update:extra', $event)"
      v-bind="$props" />
  </PanelView>
</template>

<script lang="ts" setup>
import PanelTab from '@shimakaze.sdk/webview/component/vscode/PanelTab.vue'
import PanelView from '@shimakaze.sdk/webview/component/vscode/PanelView.vue'
import Icon from '@shimakaze.sdk/webview/component/vscode/Icon.vue'
import ValueEditor, {
  type ValueEditorEmits,
  type ValueEditorProps,
} from './ValueEditor.vue'
import Button from '../vscode/Button.vue'

export interface ValuePanelProps extends ValueEditorProps {
  index: number
}

export interface ValuePanelEmits extends ValueEditorEmits {
  (e: 'remove'): void
}

defineProps<ValuePanelProps>()
defineEmits<ValuePanelEmits>()
</script>

<style lang="scss" scoped>
.value-panel-tab {
  .remove-button {
    margin-left: 0.5rem;
    visibility: hidden;
  }
  &:hover {
    .remove-button {
      visibility: unset;
    }
  }
}
</style>
