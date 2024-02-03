<template>
  <div ref="container" />
</template>

<script setup lang="ts">
import * as jsonrpc from './jsonrpc'
import * as PIXI from 'pixi.js'
import { computed, onMounted, ref } from 'vue'

const container = ref<HTMLDivElement>()
const app = ref<PIXI.Application>()
const shape = ref<jsonrpc.ShpDecodeResponse>()
const frameCount = computed(() => (shape.value?.Frames.length ?? 0) / 2)

let bunny: PIXI.Sprite | undefined = undefined
let shadowBunny: PIXI.Sprite | undefined = undefined
const current = ref(0)
const renderFrame = async () => {
  if (current.value < 0) return
  if (!app.value) return
  if (!shape.value) return
  if (current.value >= frameCount.value) return

  if (bunny) app.value.stage.removeChild(bunny)
  if (shadowBunny) app.value.stage.removeChild(shadowBunny)

  console.log('renderFrame', current.value, current.value + frameCount.value)
  const frame = shape.value.Frames[current.value]
  const shadowFrame = shape.value.Frames[current.value + frameCount.value]

  const texture = await PIXI.Assets.load(frame.Image!)
  const shadowTexture = await PIXI.Assets.load(shadowFrame.Image!)

  bunny = new PIXI.Sprite(texture)
  bunny.x = frame.Metadata.X
  bunny.y = frame.Metadata.Y

  shadowBunny = new PIXI.Sprite(shadowTexture)
  shadowBunny.x = shadowFrame.Metadata.X
  shadowBunny.y = shadowFrame.Metadata.Y

  app.value.stage.addChild(shadowBunny)
  app.value.stage.addChild(bunny)
}
onMounted(async () => {
  const palette = await jsonrpc.methods.pal.choice()
  console.log('收到响应', palette)

  shape.value = await jsonrpc.methods.shp.decode(palette.path)
  console.log('收到响应', shape)
  if (!shape.value) return

  app.value = new PIXI.Application()
  await app.value.init({
    backgroundColor: '#0000fc',
    width: shape.value.Metadata.Width,
    height: shape.value.Metadata.Height,
  })
  container.value?.appendChild(app.value.canvas)
  renderFrame()
})
</script>

<style>
main {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  justify-content: center;
  height: 100%;
}
</style>
