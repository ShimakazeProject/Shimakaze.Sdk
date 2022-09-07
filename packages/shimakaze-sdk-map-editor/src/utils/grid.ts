import * as PIXI from 'pixi.js'

const UNIT = 15

const calcPoint = (x: number, y: number, h: number = 0) => ({
  x: x * UNIT * 4 + y % 2 * UNIT * 2,
  y: y * UNIT - h * UNIT
})

export const putTile = (tile: PIXI.Sprite, x: number, y: number, h: number = 0) => {
  const target = calcPoint(x, y, h)
  tile.x = target.x
  tile.y = target.y
  return tile
}

export const put = (sprite: PIXI.Sprite, x: number, y: number, h: number = 0) => {
  putTile(sprite, x, y, h)
  sprite.x += sprite.width / 2
  sprite.y += sprite.height
  return sprite
}
