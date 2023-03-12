/// <reference path="v1.d.ts"/>
declare namespace Shimakaze.Sdk.Csf.V2 {
  /**
   * Csf多值
   */
  export interface CsfMultiValue {
    values: V1.CsfValue[]
  }

  /**
   * Csf标签的字符串值部分
   */
  export type CsfDataValue = V1.CsfSimpleValue | V1.CsfAdvancedValue | CsfMultiValue | null

  /**
   * Csf文件
   */
  export interface CsfFile {
    protocol: 2
    version?: 2 | 3
    language?: V1.CsfLanguage | number
    data: Record<string, CsfDataValue>
  }
}