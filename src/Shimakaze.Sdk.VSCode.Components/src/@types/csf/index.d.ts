export interface ICsfValueUnit{
  value: string,
  extra?: string
}

export interface ICsfUnit {
  label: string,
  values: ICsfValueUnit[]
}
