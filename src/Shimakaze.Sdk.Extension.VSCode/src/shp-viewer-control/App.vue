<template>
  <div>
    <vscode-button :disable="current === 0" @click="previous">
      上一帧
    </vscode-button>

    <vscode-text-field :value="current" @input="input">
      当前帧
    </vscode-text-field>

    <vscode-button
      appearance="primary"
      :disable="current === frameCount - 1"
      @click="next">
      下一帧
    </vscode-button>
  </div>
</template>

<script lang="ts" setup>
import { onMounted, ref } from 'vue'

const current = ref(0)
const frameCount = ref(0)

const next = () => {
  current.value++
}

const input = e => {
  current.value = Number(e.target.value)
}

const previous = () => {
  current.value--
}
</script>
