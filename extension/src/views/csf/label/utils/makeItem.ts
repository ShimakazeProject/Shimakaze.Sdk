import type { CsfUnit } from '../../../../@types/CsfModel'
import { CsfNode } from '../CsfNode'

export const createDataItemTree = (map: Map<string, CsfUnit[]>): CsfNode[] => {
  return [...map.entries()].map((item) => {
    const [type, units] = item
    const children = units.map(unit => CsfNode.create(unit))
    return CsfNode.create(type, children)
  })
}
