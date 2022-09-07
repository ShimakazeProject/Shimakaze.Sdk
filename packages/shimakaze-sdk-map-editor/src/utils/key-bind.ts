import * as camera from './camera'
import * as KeyMap from './key-map'

// See https://developer.mozilla.org/docs/Web/API/MouseEvent/button
const _isMouseDown = [false, false, false, false, false]

const mousedown = (e: MouseEvent) => {
  _isMouseDown[e.button] = true
}
const mouseup = (e: MouseEvent) => {
  _isMouseDown[e.button] = false
}
const mousemove = (e: MouseEvent) => {
  const keymap = KeyMap.get()
  if (keymap.mouse.moveCamera && _isMouseDown[keymap.mouse.moveCamera]) {
    camera.move(e.movementX, e.movementY)
  }
}
const wheel = (e: WheelEvent) => {
  const keymap = KeyMap.get()
  if (keymap.mouse.wheelToScale) {
    camera.scale(e.deltaY / 1000)
  }
}

export const init = (dom: HTMLCanvasElement) => {
  dom.addEventListener('mousedown', mousedown)
  dom.addEventListener('mouseup', mouseup)
  dom.addEventListener('mousemove', mousemove)
  dom.addEventListener('wheel', wheel)
  return dom
}
