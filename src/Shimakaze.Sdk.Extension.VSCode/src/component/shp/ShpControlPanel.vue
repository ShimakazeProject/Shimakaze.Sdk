<template>
  <div class="control-bar">
    <Dropdown id="pal-dropdown">
      <Option value="unittem.pal" />
    </Dropdown>
    <PlayControl
      :current="current"
      :is-play="isPlay"
      :min="min"
      :max="max"
      :step="step"
      @update:current="$emit('update:current', $event)"
      @update:is-play="$emit('update:isPlay', $event)" />
  </div>
</template>

<script lang="ts" setup>
import PlayControl, {
  type PlayControlProps,
} from '@shimakaze.sdk/webview/component/common/PlayControl.vue'
import Dropdown from '@shimakaze.sdk/webview/component/vscode/Dropdown.vue'
import Option from '@shimakaze.sdk/webview/component/vscode/Option.vue'

export interface ShpControlPanelProps extends PlayControlProps {
  current: number
  isPlay: boolean
}
export interface ShpControlPanelEmits {
  (e: 'update:current', ev: number): void
  (e: 'update:isPlay', ev: boolean): void
}

defineProps<ShpControlPanelProps>()
defineEmits<ShpControlPanelEmits>()
</script>

<style lang="scss" scoped>
.control-bar {
  display: flex;
  flex-direction: row;
  gap: 0.5rem;
}
</style>
