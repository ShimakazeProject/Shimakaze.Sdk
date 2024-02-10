import * as vscode from 'vscode'
import {
  LanguageClient,
  type LanguageClientOptions,
  type ServerOptions,
  Trace,
  createServerSocketTransport,
} from 'vscode-languageclient/node'

class ShimakazeClient {
  client: LanguageClient

  constructor() {
    const clientOptions: LanguageClientOptions = {
      documentSelector: [],
      progressOnInitialization: true,
    }
    this.client = new LanguageClient(
      'Shimakaze.Sdk.LanguageServer',
      'Shimakaze.Sdk: 语言服务器',
      this.getServerOptions(),
      clientOptions,
    )
  }

  async start() {
    // this.client.registerProposedFeatures()
    // this.client.registerFeature()

    const config = vscode.workspace.getConfiguration('shimakaze.sdk')

    await this.client.setTrace(
      config.get('languageClient.LogLevel') ?? Trace.Messages,
    )

    await this.client.start()
  }

  async dispose() {
    await this.client.stop()
    await this.client.dispose()
  }

  private getServerOptions(): ServerOptions {
    // TODO: 开发时调试使用的 实际使用时需要按需修改
    return async () => {
      // Socket测试
      const [reader, writer] = createServerSocketTransport(12345)
      return {
        reader,
        writer,
      }
    }
  }
}

export const client = new ShimakazeClient()
