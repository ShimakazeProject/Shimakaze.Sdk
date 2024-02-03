<template>
  <div ref="container" />
</template>

<script lang="ts" setup>
import * as PIXI from 'pixi.js'
import { computed, onMounted, ref, watch } from 'vue'

export interface ShpViewProps {
  backgroundColor: string
  width: number
  height: number
  image: Types.Shp.ShpImage
  current: number
  hasShadow?: boolean
}

const props = defineProps<ShpViewProps>()

const container = ref<HTMLDivElement>()
const app = new PIXI.Application()

const max = computed(() => {
  let value = props.image.frames.length
  if (props.hasShadow) value /= 2
  return value
})
const bunnies: {
  object?: PIXI.Sprite
  shadow?: PIXI.Sprite
} = {}

const init = async () => {
  await app.init({
    hello: true,
    backgroundColor: props.backgroundColor,
    width: props.width,
    height: props.height,
  })
  container.value?.appendChild(app.canvas)
  await render(props.current)
}

const render = async (current: number) => {
  if (current < 0) return
  if (current >= max.value) return

  // 渲染影子
  if (props.hasShadow) {
    const newShadow = await renderFrame(props.image.frames[current + max.value])
    if (newShadow) app.stage.addChild(newShadow)
    if (bunnies.shadow) {
      app.stage.removeChild(bunnies.shadow)
      bunnies.shadow.destroy()
    }
    bunnies.shadow = newShadow
  }

  const newObject = await renderFrame(props.image.frames[current])
  if (newObject) app.stage.addChild(newObject)
  if (bunnies.object) {
    app.stage.removeChild(bunnies.object)
    bunnies.object.destroy()
  }
  bunnies.object = newObject
}

const renderFrame = async (frame: Types.Shp.ShpFrame) => {
  if (!frame.image) return
  const texture = await PIXI.Assets.load(frame.image)

  const bunny = new PIXI.Sprite(texture)
  bunny.x = frame.metadata.x
  bunny.y = frame.metadata.y

  return bunny
}

watch(() => props.current, render)
onMounted(init)
</script>
