declare namespace Shimakaze.Sdk.Csf.V1 {
  /**
   * Csf支持的语言
   */
  export type CsfLanguage = "en_US" | "en_UK" | "de" | "fr" | "es" | "it" | "jp" | "Jabberwockie" | "kr" | "zh"

  /**
   * Csf元数据
   */
  export interface CsfMetadata {
    version: 2 | 3
    language?: CsfLanguage | number
  }

  /**
   * Csf简单值
   */
  export type CsfSimpleValue = string | string[]

  /**
   * Csf复杂值
   */
  export interface CsfAdvancedValue {
    value: CsfSimpleValue
    extra?: string
  }

  /**
   * Csf值
   */
  export type CsfValue = CsfSimpleValue | CsfAdvancedValue

  /**
   * Csf简单标签
   */
  export interface CsfSingleData {
    label: string
    value: CsfSimpleValue
    extra?: string
  }

  /**
   * Csf复杂标签
   */
  export interface CsfAdvancedData {
    label: string
    value?: CsfAdvancedValue
  }

  /**
   * Csf多值标签
   */
  export interface CsfMultiData {
    label: string
    values: CsfValue[]
  }

  /**
   * Csf标签
   */
  export type CsfData = CsfSingleData | CsfAdvancedData | CsfMultiData

  /**
   * Csf文件
   */
  export interface CsfFile {
    protocol: 1
    head: CsfMetadata
    data: CsfData[]
  }
}