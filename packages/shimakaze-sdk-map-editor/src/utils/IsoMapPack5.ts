/**
 * 处理 IsoMapPack5 相关
 *
 * 例子:
 * ```js
 * // Import
 * import { IsoMapPack5 } from './IsoMapPack5'
 *
 * // feature
 * const res = await fetch('IsoMapPack5.ini')
 * const ini = await res.text()
 * const base64 = IsoMapPack5.getBase64FromIni(ini) // 获取base64
 * const lzo = IsoMapPack5.decodeBase64(base64) // 解码base64得到lzo数据
 * const bin = IsoMapPack5.decompress(lzo) // 解压缩lzo得到数据
 * ```
 */
export namespace IsoMapPack5 {
  /**
   * 从ini字符串中拼接base64
   * @param ini ini without section
   * @returns base64
   */
  export const getBase64FromIni = (ini: string) => ini.split('\n').map(s => s.replace(/\d+=/, '')).reduce((a, b) => a + b, '')

  /**
   * 从 base64 字符串中解码为 lzo 数据
   * @param base64 base64
   * @returns lzo
   */
  export const decodeBase64 = (base64: string) => {
    const raw = window.atob(base64)
    const lzo = new Uint8Array(raw.length)
    for (let i = 0; i < raw.length; i++) {
      lzo.set([raw.charCodeAt(i)], i)
    }
    return lzo
  }
}
export default IsoMapPack5
