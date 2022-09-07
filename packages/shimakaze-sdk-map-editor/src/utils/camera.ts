import Kernel from './kernel'

const xMin = -50
const yMin = -100
const scaleMin = 0.5
const scaleMax = 5

export const move = (offsetX: number, offsetY: number) => {
  Kernel.instance.container.pivot.x -= offsetX
  Kernel.instance.container.pivot.y -= offsetY
  if (Kernel.instance.container.pivot.x < xMin) {
    Kernel.instance.container.pivot.x = xMin
  }
  if (Kernel.instance.container.pivot.y < yMin) {
    Kernel.instance.container.pivot.y = yMin
  }
}

export const moveTo = (x: number, y: number) => {
  Kernel.instance.container.pivot.x = x
  Kernel.instance.container.pivot.y = y
}

export const scale = (scale: number = 1) => {
  const size = Kernel.instance.container.scale
  size.x += scale
  size.y += scale
  if (size.x < scaleMin) {
    size.x = scaleMin
    size.y = scaleMin
  }
  if (size.x > scaleMax) {
    size.x = scaleMax
    size.y = scaleMax
  }
}
