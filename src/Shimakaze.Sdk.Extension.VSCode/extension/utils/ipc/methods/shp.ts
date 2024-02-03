import * as daemon from '@shimakaze.sdk/extension/utils/ipc/daemon'

export interface ShpDecodeParams {
  ShapePath: string
  PalettePath: string
}

export interface ShpDecodeResponse {
  Metadata: {
    Height: number
    NumImages: number
    Reserved: number
    Width: number
  }
  Frames: Array<{
    Metadata: {
      BodyLength: number
      Color: number
      CompressionType: number
      Height: number
      Offset: number
      Padding1: number
      Padding2: number
      Reserved: number
      Width: number
      X: number
      Y: number
    }
    Image?: string
  }>
}

export const names = {
  decode: '/shp/decode',
}

export const decode = async (params: ShpDecodeParams) =>
  await daemon.current().sendRequest<ShpDecodeResponse>(names.decode, params)
