export interface CsfValueUnit{
  value: string,
  extra?: string
}

export interface CsfUnit {
  label: string,
  values: CsfValueUnit[]
}
export interface I18n{
  label?: string
  labelPlaceholder?: string
  item?: string
  value?: string
  valuePlaceholder?: string
  extra?: string
  extraPlaceholder?: string
}
