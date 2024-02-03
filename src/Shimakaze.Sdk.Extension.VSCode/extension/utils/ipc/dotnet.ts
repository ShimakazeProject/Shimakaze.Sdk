import * as vscode from 'vscode'

/**
 * @file .Net Install Tool 调用相关内容
 * @author frg2089
 */

/** .Net 运行时版本 */
const version = '8.0'

/**
 * 获取 .Net 可执行程序路径
 * @param extension 扩展实例
 */
export const getDotnetPath = async (requestingExtensionId: string) => {
  const res = await vscode.commands.executeCommand<{
    dotnetPath?: string
  }>('dotnet.acquire', { version, requestingExtensionId })
  const dotnetPath = res?.dotnetPath
  if (!dotnetPath) {
    throw new Error('Could not resolve the dotnet path!')
  }

  return dotnetPath
}
