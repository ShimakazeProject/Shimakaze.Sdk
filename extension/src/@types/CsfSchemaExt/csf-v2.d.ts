import type { AdvancedCsfValue, CsfMultiLineValue, CsfSingleLineValue } from '../../@types/generate/csf-v2'
export type * from '../../@types/generate/csf-v2'
export type SimpleValue = CsfSingleLineValue | CsfMultiLineValue
export type SimpleOrAdvancedValue = SimpleValue | AdvancedCsfValue
export type ValueArrayItem = CsfSingleLineValue | CsfMultiLineValue | AdvancedCsfValue
