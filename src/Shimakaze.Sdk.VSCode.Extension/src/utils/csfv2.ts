import type { CsfUnit, CsfValueUnit } from '../@types/CsfModel'
import type { AdvancedCsfValue, CSFJsonV2Schema, CsfMultiLineValue, CsfValueArray, CsfValueRoot, SimpleOrAdvancedValue, SimpleValue, ValueArrayItem } from '../@types/CsfSchemaExt/csf-v2'

const parseSimpleValue = (data: SimpleValue): string => {
  if (typeof data === 'string') {
    return data
  } else if (typeof data === 'object' && 'length' in data) {
    const arr = data as CsfMultiLineValue
    return arr.join('\n')
  } else {
    throw new Error('Invalid data type. Must be string or array.')
  }
}

const parseAdvancedValue = (data: AdvancedCsfValue): CsfValueUnit => {
  return { value: parseSimpleValue(data.value), extra: data.extra }
}
const parseSimpleOrAdvancedValue = (data: SimpleOrAdvancedValue): CsfValueUnit => {
  if (typeof data === 'string' || (typeof data === 'object' && 'length' in data)) {
    return { value: parseSimpleValue(data as SimpleValue) }
  }

  return parseAdvancedValue(data as AdvancedCsfValue)
}

const parseValues = (data: CsfValueRoot): CsfValueUnit[] => {
  const result: CsfValueUnit[] = []

  if (typeof data === 'object' &&
    'values' in data &&
    typeof data.values !== 'function') {
    const tmp = data as CsfValueArray
    result.push(...[...tmp.values].map(v => parseSimpleOrAdvancedValue(v)))
  } else {
    result.push(parseSimpleOrAdvancedValue(data as SimpleOrAdvancedValue))
  }

  return result
}

export const parse = (label: string, data: CsfValueRoot): CsfUnit | undefined => {
  try {
    return { label, values: [...parseValues(data)] }
  } catch (error) {
    console.error(error)
  }
  return undefined
}

const parseAdvancedCsfValue = (unit: CsfValueUnit): AdvancedCsfValue => {
  const value = unit.value.includes('\n') ? unit.value.split(/\r?\n/) : unit.value
  return { value, extra: unit.extra }
}
const simplifyAdvancedCsfValue = (value: AdvancedCsfValue): ValueArrayItem => {
  if ('extra' in value && value.extra != null) {
    return value
  }

  return value.value
}

const simplifyValueArrayItem = (values: ValueArrayItem[]): CsfValueRoot => {
  if (values.length === 1) {
    return values[0]
  }

  return { values: values }
}

export const format = (csf: CsfUnit[]): CSFJsonV2Schema => {
  const data = Object.assign({}, ...csf.map(unit => {
    return {
      [unit.label]: simplifyValueArrayItem(
        unit.values.map((valueUnit) => simplifyAdvancedCsfValue(
          parseAdvancedCsfValue(valueUnit)
        ))
      )
    }
  }))

  return {
    $schema: 'https://shimakazeproject.github.io/json/csf/v2/schema.json',
    protocol: 2,
    version: 3,
    language: 'en_US',
    data
  }
}
