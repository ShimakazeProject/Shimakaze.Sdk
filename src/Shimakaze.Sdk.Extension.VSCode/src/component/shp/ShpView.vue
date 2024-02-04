<template>
  <div ref="container" />
</template>

<script lang="ts" setup>
import * as PIXI from 'pixi.js'
import { computed, onMounted, ref, watch } from 'vue'

export interface SpritesheetData extends PIXI.SpritesheetData {
  animations: {
    animation: string[]
  }
  meta: {
    image: string
    scale: PIXI.SpritesheetData['meta']['scale']
  }
}

export interface ShpViewProps {
  backgroundColor: string
  width: number
  height: number
  spritesheet: SpritesheetData
  current: number
  hasShadow?: boolean
}

const props = defineProps<ShpViewProps>()

const container = ref<HTMLDivElement>()
const app = new PIXI.Application()
let spritesheet: PIXI.Spritesheet | undefined = undefined
let anim: PIXI.AnimatedSprite | undefined = undefined

const max = computed(() => {
  let value = props.spritesheet.animations.animation.length
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

  spritesheet = new PIXI.Spritesheet(
    PIXI.Texture.from(props.spritesheet.meta.image, true),
    props.spritesheet,
  )

  await spritesheet.parse()

  // spritesheet is ready to use!
  anim = new PIXI.AnimatedSprite(spritesheet.animations['animation'])

  // set the animation speed
  anim.animationSpeed = 0.1

  // add it to the stage to render
  app.stage.addChild(anim)

  await render(props.current)
}

const render = async (current: number) => {
  if (current < 0) return
  if (current >= max.value) return
  if (!spritesheet) await init()

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

  anim?.currentFrame
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
