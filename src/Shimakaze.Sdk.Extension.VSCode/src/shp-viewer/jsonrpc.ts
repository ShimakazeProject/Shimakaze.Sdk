import * as utils from '@shimakaze.sdk/webview/common'

const connection = utils.jsonrpc.connect(window, utils.vscode.vscode)

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

export const methods = {
  pal: {
    choice: () => connection.sendRequest<{ path: string }>('/pal/choice'),
  },
  shp: {
    decode: (palettePath: string) =>
      connection.sendRequest<ShpDecodeResponse>('/shp/decode', { palettePath }),
  },
}
