import * as vscode from 'vscode'
import { getCsfScriptUri, getToolkitUri } from '../../CONSTANTS'
import { html } from './template'

export class CsfEditViewPanel {
  public static currentPanel: CsfEditViewPanel | undefined;
  private readonly _panel: vscode.WebviewPanel;
  private _disposables: vscode.Disposable[] = [];

  private constructor (panel: vscode.WebviewPanel, extensionUri: vscode.Uri) {
    this._panel = panel
    this._panel.webview.html = this._getWebviewContent(this._panel.webview, extensionUri)
  }

  private _getWebviewContent (webview: vscode.Webview, extensionUri: vscode.Uri) {
    const toolkitUri = getToolkitUri(webview, extensionUri)
    const csfScriptUri = getCsfScriptUri(webview, extensionUri)
    return html({
      label: 'THEME:Intro',
      values: [
        { value: '开场', extra: 'extra' },
        { value: '新字符串' }
      ]
    }, toolkitUri, csfScriptUri)
  }

  public static render (extensionUri: vscode.Uri) {
    if (CsfEditViewPanel.currentPanel) {
      CsfEditViewPanel.currentPanel._panel.reveal(vscode.ViewColumn.One)
      return
    }
    const panel = vscode.window.createWebviewPanel('shimakaze-sdk-vscode.csf.editor', 'Csf Editor', vscode.ViewColumn.Active, {
      enableScripts: true
    })

    CsfEditViewPanel.currentPanel = new CsfEditViewPanel(panel, extensionUri)
  }

  public dispose () {
    CsfEditViewPanel.currentPanel = undefined

    this._panel.dispose()

    while (this._disposables.length) {
      const disposable = this._disposables.pop()
      if (disposable) {
        disposable.dispose()
      }
    }
  }
}
