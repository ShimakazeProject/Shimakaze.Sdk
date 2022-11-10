import * as vscode from 'vscode'
import type { CsfNode } from '../label/CsfNode'
import type { CsfValueViewProvider } from './provider'

/**
 * 推送数据
 * @param provider provider
 * @param log log
 * @returns command
 */
export const put = (provider: CsfValueViewProvider, log: vscode.OutputChannel) => async (unit: CsfNode) => {
  provider.unit = unit
  await provider.pushMessage({ type: 'put', data: unit.data })
}
