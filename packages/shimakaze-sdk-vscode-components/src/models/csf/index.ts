export interface CsfValueUnit{
  value: string,
  extra?: string
}

export interface CsfUnit {
  label: string,
  values: CsfValueUnit[]
}
