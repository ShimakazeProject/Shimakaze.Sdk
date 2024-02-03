declare namespace Types.Shp {
  export interface ShpFrame {
    /** 帧的元数据 */
    metadata: {
      bodyLength: number
      color: number
      compressionType: number
      height: number
      offset: number
      padding1: number
      padding2: number
      reserved: number
      width: number
      x: number
      y: number
    }
    /** 图像的base64 */
    image?: string
  }
}
