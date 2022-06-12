import * as vscode from 'vscode'
import type { CsfValueUnit } from '../../../@types/CsfModel'
import type { Receives } from './type'
import type { WebViewMessage, WebViewMessageWithData } from '../../../@types/WebViewMessage'
import { _viewCommand } from '../../../utils/command'
import { CsfValueViewProvider } from './provider'
import { CsfLabelViewProvider } from '../label/provider'

const receives: Receives = {
  update: async (sender: CsfValueViewProvider, data: WebViewMessage) => {
    await vscode.commands.executeCommand(
      _viewCommand(CsfLabelViewProvider.viewType, 'update'),
      (data as WebViewMessageWithData<CsfValueUnit>).data)
  }
}
export default receives
