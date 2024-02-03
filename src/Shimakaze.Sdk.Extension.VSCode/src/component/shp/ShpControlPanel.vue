<template>
  <div class="control-bar">
    <Dropdown id="pal-dropdown">
      <Option value="unittem.pal" />
    </Dropdown>
    <PlayControl v-model:current.number="current" v-model:is-play="isPlay" />
  </div>
</template>

<script lang="ts" setup>
import PlayControl from '@shimakaze.sdk/webview/component/common/PlayControl.vue'
import Dropdown from '@shimakaze.sdk/webview/component/vscode/Dropdown.vue'
import Option from '@shimakaze.sdk/webview/component/vscode/Option.vue'
import { ref } from 'vue'

const current = ref(0)
const isPlay = ref(false)
</script>

<style lang="scss" scoped>
.control-bar {
  display: flex;
  flex-direction: row;
  gap: 0.5rem;
}
</style>
