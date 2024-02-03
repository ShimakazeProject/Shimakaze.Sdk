import * as logger from '@shimakaze.sdk/extension/utils/logger'
import type * as vscode from 'vscode'
import * as jsonrpc from 'vscode-jsonrpc/node'

/**
 * 通过两个流启动JsonRPC服务
 * @param input 标准输入
 * @param output 标准输出
 */
export const connect: {
  (
    input: NodeJS.WritableStream,
    output: NodeJS.ReadableStream,
  ): jsonrpc.MessageConnection
  (webview: vscode.Webview): jsonrpc.MessageConnection
} = (
  arg1: NodeJS.WritableStream | vscode.Webview,
  arg2?: NodeJS.ReadableStream,
) => {
  const { reader, writer } = (() => {
    if (arg2) {
      return {
        reader: new jsonrpc.StreamMessageReader(arg2 as NodeJS.ReadableStream),
        writer: new jsonrpc.StreamMessageWriter(arg1 as NodeJS.WritableStream),
      }
    } else {
      return {
        reader: new VSCodeMessageReader(arg1 as vscode.Webview),
        writer: new VSCodeMessageWriter(arg1 as vscode.Webview),
      }
    }
  })()

  const connection = jsonrpc.createMessageConnection(reader, writer, {
    error(message) {
      logger
        .getLogChannel()
        .appendLine(`${new Date().toISOString()} [error]: ${message}`)
    },
    warn(message) {
      logger
        .getLogChannel()
        .appendLine(`${new Date().toISOString()} [warn]: ${message}`)
    },
    info(message) {
      logger
        .getLogChannel()
        .appendLine(`${new Date().toISOString()} [info]: ${message}`)
    },
    log(message) {
      logger
        .getLogChannel()
        .appendLine(`${new Date().toISOString()} [log]: ${message}`)
    },
  })
  connection.listen()
  return connection
}

class VSCodeMessageReader extends jsonrpc.AbstractMessageReader {
  private webview: vscode.Webview

  constructor(webview: vscode.Webview) {
    super()
    this.webview = webview
  }

  override listen(callback: jsonrpc.DataCallback): jsonrpc.Disposable {
    return this.webview.onDidReceiveMessage(callback)
  }
}

class VSCodeMessageWriter
  extends jsonrpc.AbstractMessageWriter
  implements jsonrpc.MessageWriter
{
  private webview: vscode.Webview

  constructor(webview: vscode.Webview) {
    super()
    this.webview = webview
  }

  async write(msg: jsonrpc.Message): Promise<void> {
    await this.webview.postMessage(msg)
  }

  end(): void {}
}
