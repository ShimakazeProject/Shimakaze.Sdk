declare namespace Types.Shp {
  export interface ShpViewerStatus {
    /** 总帧数 */
    total: number
    /** 当前帧 */
    current: number
    /** 包含影子帧 */
    hasShadow?: boolean
  }
}
