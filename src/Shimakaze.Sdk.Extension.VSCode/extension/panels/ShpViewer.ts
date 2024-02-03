import { getHtml } from '@shimakaze.sdk/extension/utils/html'
import * as daemon from '@shimakaze.sdk/extension/utils/ipc/daemon'
import * as jsonrpc from '@shimakaze.sdk/extension/utils/ipc/jsonrpc'
import * as methods from '@shimakaze.sdk/extension/utils/ipc/methods'
import * as vscode from 'vscode'

export class ShpViewer {
  private context: vscode.ExtensionContext
  private file: vscode.Uri
  private extensionUri: vscode.Uri
  private panel: vscode.WebviewPanel
  private frontend: ReturnType<typeof jsonrpc.connect>

  constructor(context: vscode.ExtensionContext, file: vscode.Uri) {
    this.context = context
    this.file = file
    this.extensionUri = this.context.extension.extensionUri

    this.initialize()
    this.initializeFrontEnd()
    this.initializeBackEnd()
  }

  public dispose() {
    this.frontend.dispose()
    this.panel.dispose()
  }

  private async initialize() {
    // 构造面板
    this.panel = vscode.window.createWebviewPanel(
      'Shimakaze.Sdk:ShpViewer',
      'Shp Viewer',
      vscode.ViewColumn.Active,
      {
        enableScripts: true,
        localResourceRoots: [
          vscode.Uri.joinPath(this.extensionUri, 'dist', 'webview'),
        ],
      },
    )
    this.context.subscriptions.push(this.panel)

    this.panel.webview.html = await getHtml({
      enterPoint: 'shp-viewer',
      extensionUri: this.extensionUri,
      cspSource: this.panel.webview.cspSource,
      dependencies: ['pixi'],
    })

    this.panel.onDidDispose(this.dispose, undefined, this.context.subscriptions)
  }

  private initializeFrontEnd() {
    this.frontend = jsonrpc.connect(this.panel.webview)

    this.frontend.onRequest('/pal/choice', this.choicePalette)

    this.frontend.onRequest(
      methods.shp.names.decode,
      async (params: Omit<methods.shp.ShpDecodeParams, 'ShapePath'>) =>
        await methods.shp.decode({
          ...params,
          ShapePath: this.file.fsPath,
        }),
    )

    this.context.subscriptions.push(this.frontend)
  }

  private async initializeBackEnd() {
    daemon.getServer(this.context)
  }

  /** 选择调色板 */
  private choicePalette = async () => {
    const palettes = await vscode.window.showOpenDialog({
      canSelectFiles: true,
      canSelectMany: false,
      filters: {
        [vscode.l10n.t('调色板文件')]: ['pal'],
      },
      title: vscode.l10n.t('请选择一个调色板文件'),
    })

    // 未选择文件
    if (!palettes.length) return

    const palette = palettes[0]

    return {
      path: palette.fsPath,
    }
  }
}
