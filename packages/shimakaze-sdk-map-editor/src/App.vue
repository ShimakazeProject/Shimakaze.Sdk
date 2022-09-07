<template>
  <div
    ref="root"
    class="h-screen"
  />
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import * as PIXI from 'pixi.js'
import Kernel from '@/utils/kernel'
import * as Grid from '@/utils/grid'
import * as KeyBind from '@/utils/key-bind'

const root = ref<HTMLDivElement>()

const init = async () => {
  if (!root.value) return

  const kernel = new Kernel({
    width: root.value.clientWidth,
    height: root.value.clientHeight
  })
  root.value.appendChild(
    KeyBind.init(
      kernel.dom
    )
  )

  const texture = PIXI.Texture.from('clear01.png')
  for (let y = 0; y < 100; y++) {
    for (let x = 0; x < 100; x++) {
      kernel.container.addChild(
        Grid.putTile(
          new PIXI.Sprite(texture),
          x,
          y
        )
      )
    }
  }
}

onMounted(init)
</script>
