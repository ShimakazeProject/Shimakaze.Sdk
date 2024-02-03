<template>
  <LabelEditor :label="label" @update:label="$emit('update:label', $event)" />
  <Panels v-model:active-id="activeId">
    <ValuePanel
      v-for="(value, i) in values"
      :key="i"
      :index="i"
      :value-rows="valueRows"
      :value="value.value"
      :extra="value.extra"
      @update:value="onUpdateValue(i, $event)"
      @update:extra="onUpdateExtra(i, $event)"
      @remove="onRemove(i)" />
    <template #end>
      <Button appearance="icon" @click="onAdd">
        <Icon icon="add" />
      </Button>
    </template>
  </Panels>
</template>

<script lang="ts" setup>
import Button from '@shimakaze.sdk/webview/component/vscode/Button.vue'
import Icon from '@shimakaze.sdk/webview/component/vscode/Icon.vue'
import Panels from '@shimakaze.sdk/webview/component/vscode/Panels.vue'
import { ref } from 'vue'
import LabelEditor, {
  type LabelEditorEmits,
  type LabelEditorProps,
} from './LabelEditor.vue'
import { type ValueEditorProps } from './ValueEditor.vue'
import ValuePanel from './ValuePanel.vue'

export type CsfValue = Omit<ValueEditorProps, 'valueRows'>
export interface CsfData extends Required<LabelEditorProps> {
  values: CsfValue[]
}

export interface CsfEditorProps
  extends CsfData,
    Pick<ValueEditorProps, 'valueRows'> {}

export interface CsfEditorEmits extends LabelEditorEmits {
  (e: 'update:values', ev: CsfValue[]): void
}

const props = defineProps<CsfEditorProps>()
const emits = defineEmits<CsfEditorEmits>()

const activeId = ref('')

const onUpdateValue = (index: number, value: string) => {
  props.values[index].value = value
  emits('update:values', props.values)
}
const onUpdateExtra = (index: number, extra: string) => {
  props.values[index].extra = extra
  emits('update:values', props.values)
}
const onAdd = () => {
  const id = `tab-${props.values.length}`
  props.values.push({
    value: undefined,
    extra: undefined,
  })
  emits('update:values', props.values)
  activeId.value = id
}
const onRemove = (index: number) => {
  const id = `tab-${Math.max(index - 1, 0)}`
  props.values.splice(index, 1)
  emits('update:values', props.values)
  activeId.value = id
}
</script>
