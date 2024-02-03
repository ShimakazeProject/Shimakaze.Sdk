import type { WebviewApi } from 'vscode-webview'
import * as jsonrpc from 'vscode-jsonrpc/browser'

class VSCodeMessageReader extends jsonrpc.AbstractMessageReader {
  private window: Window

  constructor(window: Window) {
    super()
    this.window = window
  }

  override listen(callback: jsonrpc.DataCallback): jsonrpc.Disposable {
    const listener = (ev: MessageEvent<any>) => {
      if (ev.data.jsonrpc !== '2.0') return

      return callback(ev.data)
    }
    this.window.addEventListener('message', listener)

    return jsonrpc.Disposable.create(() => {
      this.window.removeEventListener('message', listener)
    })
  }
}

class VSCodeMessageWriter
  extends jsonrpc.AbstractMessageWriter
  implements jsonrpc.MessageWriter
{
  private vscode: WebviewApi<unknown>

  constructor(vscode: WebviewApi<unknown>) {
    super()
    this.vscode = vscode
  }

  async write(msg: jsonrpc.Message): Promise<void> {
    this.vscode.postMessage(msg)
  }

  end(): void {}
}

let _connection: jsonrpc.MessageConnection | undefined
export const connect = (window: Window, vscode: WebviewApi<unknown>) => {
  if (_connection) return _connection

  _connection = jsonrpc.createMessageConnection(
    new VSCodeMessageReader(window),
    new VSCodeMessageWriter(vscode),
  )
  _connection.listen()
  return _connection
}
