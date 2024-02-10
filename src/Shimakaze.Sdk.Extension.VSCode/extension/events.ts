import { client } from '@shimakaze.sdk/extension/client'
import { createDocument } from '@shimakaze.sdk/extension/utils'
import * as vscode from 'vscode'
import {
  DidChangeTextDocumentNotification,
  DidCloseTextDocumentNotification,
  DidOpenTextDocumentNotification,
  DidSaveTextDocumentNotification,
} from 'vscode-languageclient'

export const init = (logger: vscode.LogOutputChannel, extensionId: string) =>
  vscode.Disposable.from(
    // 打开文档
    vscode.workspace.onDidOpenTextDocument(async doc => {
      logger.trace('打开文档', doc)

      await client.instance.sendNotification(
        DidOpenTextDocumentNotification.type,
        {
          textDocument: createDocument(doc),
        },
      )
    }),
    // 修改文档
    vscode.workspace.onDidChangeTextDocument(async ev => {
      if (
        ev.document.uri.scheme === 'output' &&
        ev.document.uri.path.includes(extensionId)
      ) {
        // 避免这玩意儿自己无限循环
        return
      }

      logger.trace('修改文档', ev)

      await client.instance.sendNotification(
        DidChangeTextDocumentNotification.type,
        {
          textDocument: createDocument(ev.document),
          contentChanges: ev.contentChanges.map(i => i),
        },
      )
    }),
    // 保存文档
    vscode.workspace.onDidSaveTextDocument(async doc => {
      logger.trace('保存文档', doc)

      await client.instance.sendNotification(
        DidSaveTextDocumentNotification.type,
        {
          textDocument: createDocument(doc),
        },
      )
    }),
    // 关闭文档
    vscode.workspace.onDidCloseTextDocument(async doc => {
      logger.trace('关闭文档', doc)

      await client.instance.sendNotification(
        DidCloseTextDocumentNotification.type,
        {
          textDocument: createDocument(doc),
        },
      )
    }),
  )
