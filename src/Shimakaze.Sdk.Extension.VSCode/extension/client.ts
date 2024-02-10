import * as vscode from 'vscode'
import {
  LanguageClient,
  RevealOutputChannelOn,
  Trace,
  createServerSocketTransport,
  type LanguageClientOptions,
  type ServerOptions,
} from 'vscode-languageclient/node'

class ShimakazeClient {
  private readonly _client: LanguageClient

  get instance() {
    return this._client
  }

  constructor() {
    const clientOptions: LanguageClientOptions = {
      documentSelector: [
        {
          language: 'ini',
        },
      ],
      outputChannelName: 'Shimakaze.Sdk 语言服务器日志',
      revealOutputChannelOn: RevealOutputChannelOn.Debug,
      progressOnInitialization: true,
    }

    this._client = new LanguageClient(
      'Shimakaze.Sdk.LanguageServer',
      'Shimakaze.Sdk: 语言服务器',
      this.getServerOptions(),
      clientOptions,
      import.meta.env.DEV,
    )
  }

  async start(logger: vscode.LogOutputChannel) {
    logger.trace('正在启动语言服务器...')

    const config = vscode.workspace.getConfiguration('shimakaze.sdk')
    const trace = config.get<Trace>('log.server') ?? Trace.Messages

    if (this._client.isInDebugMode) {
      logger.trace('当前处于Debug模式')
    }

    logger.trace('获取到的用户配置的语言服务器日志等级为', trace)
    if (import.meta.env.DEV) {
      logger.debug('当前在开发模式，将强制日志等级为', Trace.Verbose)
      await this._client.setTrace(Trace.Verbose)
    } else {
      await this._client.setTrace(trace)
    }

    await this._client.start()

    logger.trace('已连接到Shimakaze.Sdk语言服务器')
    return this
  }

  async dispose() {
    await this._client.stop()
    await this._client.dispose()
  }

  private getServerOptions(): ServerOptions {
    if (import.meta.env.DEV) {
      // TODO: 开发时调试使用的 实际使用时需要按需修改
      return async () => {
        // Socket 测试
        const [reader, writer] = createServerSocketTransport(12345)
        // Pipe 测试
        // const [reader, writer] = createServerPipeTransport(
        //   'Shimakaze.Sdk.LanguageServer',
        // )
        return {
          reader,
          writer,
        }
      }
    } else {
      throw new Error('NotImplemented')
    }
  }
}

export const client = new ShimakazeClient()
