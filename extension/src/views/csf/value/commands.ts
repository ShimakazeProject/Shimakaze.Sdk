import * as vscode from 'vscode'
import type { CsfUnit } from '../../../@types/CsfModel'
import { CsfValueViewProvider } from './provider'

/**
 * 推送数据
 * @param provider provider
 * @param log log
 * @returns command
 */
export const put = (provider: CsfValueViewProvider, log: vscode.OutputChannel) => async (unit: CsfUnit) => {
  await provider.pushMessage({ type: 'put', data: unit })
}
