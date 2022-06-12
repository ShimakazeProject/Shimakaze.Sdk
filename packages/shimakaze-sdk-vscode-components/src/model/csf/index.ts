import { observable } from '@microsoft/fast-element'
import { ICsfUnit, ICsfValueUnit } from '../../@types/csf'

export class CsfValueUnit implements ICsfValueUnit {
  @observable
  value: string;

  @observable
  extra?: string;

  constructor (value: string, extra?: string) {
    this.value = value
    this.extra = extra
  }
}

export class CsfUnit implements ICsfUnit {
  @observable
  label: string;

  @observable
  values: CsfValueUnit[];

  constructor (label: string, values: CsfValueUnit[]) {
    this.label = label
    this.values = values
  }
}
