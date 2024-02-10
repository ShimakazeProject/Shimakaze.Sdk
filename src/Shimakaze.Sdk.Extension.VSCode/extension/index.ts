import { client } from '@shimakaze.sdk/extension/client'
import { init } from '@shimakaze.sdk/extension/events'
import {
  iniFoldingRangeProvider,
  iniSemanticTokensProvider,
} from '@shimakaze.sdk/extension/providers/iniServicesProvider'
import * as vscode from 'vscode'

export type MyExtension = vscode.Extension<ReturnType<typeof activate>>

export async function activate(context: vscode.ExtensionContext) {
  const logger = vscode.window.createOutputChannel(
    'Shimakaze.Sdk for VSCode 工作日志',
    { log: true },
  )

  if (import.meta.env.DEV) {
    logger.clear()
  }

  logger.debug(
    'Shimakaze.Sdk for VSCode 已被激活，扩展ID：',
    context.extension.id,
  )

  if (import.meta.env.DEV) {
    logger.show()
  }

  context.subscriptions.push(await client.start(logger))
  context.subscriptions.push(init(logger, context.extension.id))

  // semanticTokensProvider
  context.subscriptions.push(iniSemanticTokensProvider())
  context.subscriptions.push(iniFoldingRangeProvider())
}

export function deactivate() {}
