declare namespace Types.Shp {
  export interface ShpImage {
    /** 帧的元数据 */
    metadata: {
      height: number
      numImages: number
      reserved: number
      width: number
    }
    /** 图像的所有的帧 */
    frames: ShpFrame[]
  }
}
