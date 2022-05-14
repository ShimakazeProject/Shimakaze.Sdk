import { ValueConverter } from '@microsoft/fast-element'

export const stringify = (value: any): string => {
  if (value === 'undefined') {
    return ''
  }
  return encodeURIComponent(JSON.stringify(value))
}
export const parse = (value: string): any => {
  if (value === 'undefined') {
    return {}
  }
  return JSON.parse(decodeURIComponent(value))
}

export const JSONConverter : ValueConverter = {
  toView (value: any) {
    try {
      if (value === 'undefined') {
        return
      }
      return typeof value === 'string'
        ? JSON.parse(decodeURIComponent(value))
        : encodeURIComponent(JSON.stringify(value))
    } catch (error) {
      console.error(error, `Value is "${value}"(${typeof value})`)
    }
  },
  fromView (value: any) {
    return this.toView(value)
  }
}
