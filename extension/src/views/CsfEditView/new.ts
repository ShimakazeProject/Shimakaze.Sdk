import * as vscode from 'vscode'
import { getToolkitUri, getCsfScriptUri } from '../../CONSTANTS'
import { html } from './template'

export class CsfEditViewProvider implements vscode.WebviewViewProvider {
  public static readonly viewType = 'shimakaze-sdk.csf-edit-panel';

  private _view?: vscode.WebviewView;
  private readonly _extensionUri: vscode.Uri

  constructor (extensionUri: vscode.Uri) {
    this._extensionUri = extensionUri
  }

  public resolveWebviewView (
    webviewView: vscode.WebviewView,
    context: vscode.WebviewViewResolveContext,
    _token: vscode.CancellationToken
  ) {
    vscode.window.showInformationMessage('resolveWebviewView')
    this._view = webviewView

    webviewView.webview.options = {
      // Allow scripts in the webview
      enableScripts: true,

      localResourceRoots: [
        this._extensionUri
      ]
    }

    webviewView.webview.html = this._getHtmlForWebview(webviewView.webview)

    webviewView.webview.onDidReceiveMessage(data => {
      switch (data.type) {
        case 'colorSelected':
        {
          vscode.window.activeTextEditor?.insertSnippet(new vscode.SnippetString(`#${data.value}`))
          break
        }
      }
    })
  }

  private _getHtmlForWebview (webview: vscode.Webview) {
    vscode.window.showInformationMessage('_getHtmlForWebview')
    return html(
      {
        label: 'THEME:Intro',
        values: [
          { value: '开场', extra: 'extra' },
          { value: '新字符串' }
        ]
      },
      getToolkitUri(webview, this._extensionUri),
      getCsfScriptUri(webview, this._extensionUri)
    )
  }
}
