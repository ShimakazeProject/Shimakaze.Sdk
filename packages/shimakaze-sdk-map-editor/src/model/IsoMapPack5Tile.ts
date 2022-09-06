import { Uint8ArrayToUint16, Uint8ArrayToUint32 } from '../utils/converts'

export interface IsoMapPack5Tile {
  x: number // uint16
  y: number // uint16
  tileIndex: number // uint32
  tileSubIndex: number // uint8
  level: number // uint8
  iceGrowth: number // uint8
}

// eslint-disable-next-line no-redeclare
export namespace IsoMapPack5Tile{
  export const readAll = (data: Uint8Array) => {
    let p = 0
    const count = data.length / 11
    const result = new Array<IsoMapPack5Tile>(count)
    for (let i = 0; i < count; i++) {
      result[i] = read(data, p)
      p += 11
    }
    return result
  }
  export const read = (data: Uint8Array, index: number = 0): IsoMapPack5Tile => {
    return {
      x: Uint8ArrayToUint16(data.slice(index, index + 2)),
      y: Uint8ArrayToUint16(data.slice(index + 2, index + 4)),
      tileIndex: Uint8ArrayToUint32(data.slice(index + 4, index + 8)),
      tileSubIndex: data[index + 8],
      level: data[index + 9],
      iceGrowth: data[index + 10]
    }
  }
}
