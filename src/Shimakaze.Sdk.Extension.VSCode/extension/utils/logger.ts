import * as vscode from 'vscode'

/** 日志通道名 */
const LogChannelName = 'Shimakaze.Sdk.Extension.Server.Status'

let _logChannel: vscode.OutputChannel | undefined = undefined
export const getLogChannel = () =>
  (_logChannel ??= vscode.window.createOutputChannel(LogChannelName, 'log'))

export const dispose = () => {
  _logChannel?.dispose()
}
