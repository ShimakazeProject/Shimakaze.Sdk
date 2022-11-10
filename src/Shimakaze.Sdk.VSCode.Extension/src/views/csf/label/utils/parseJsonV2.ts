
import type { CsfUnit } from '../../../../@types/CsfModel'
import type { CSFJsonV2Schema } from '../../../../@types/CsfSchemaExt/csf-v2'
import { parse } from '../../../../utils/csfv2'
import { createDataItemTree } from './makeItem'

export const parseJsonV2 = (csf : CSFJsonV2Schema) => {
  const labels = Object.keys(csf.data)
  const types = new Map<string, CsfUnit[]>()

  labels.forEach(label => {
    const unit = parse(label, csf.data[label])
    const tmp = label.split(/[:_]/)
    const type = tmp.length > 1 ? tmp[0] : '(default)'

    if (!unit) {
      console.error(`${label} is not a valid csf unit`)
    }

    if (types.has(type)) {
            types.get(type)!.push(unit!)
    } else {
      types.set(type, [unit!])
    }
  })

  return createDataItemTree(types)
}
