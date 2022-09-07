
export class MouseMap {
  moveCamera?: number = 1
  wheelToScale:boolean = true
}

export class KeyboardMap {

}

export class KeyMap {
  mouse: MouseMap = new MouseMap()
  keyboard: KeyboardMap = new KeyboardMap()
}

let _keyMap: KeyMap = new KeyMap()

export const get = () => _keyMap
export const set = (keyMap: KeyMap) => {
  _keyMap = keyMap
}
