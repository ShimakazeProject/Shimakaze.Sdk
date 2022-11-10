import * as vscode from 'vscode'
import { getToolkitUri, getCsfScriptUri } from '../../../CONSTANTS'
import { WebViewMessage } from '../../../@types/WebViewMessage'
import { html } from './template'
import receive from './receive'
import { CsfNode } from '../label/CsfNode'

export class CsfValueViewProvider implements vscode.WebviewViewProvider {
  public static readonly viewType = 'shimakaze-sdk.view.csf.value';

  private _view?: vscode.WebviewView;
  private readonly _extensionUri: vscode.Uri
  unit?: CsfNode

  constructor (extensionUri: vscode.Uri) {
    this._extensionUri = extensionUri
  }

  resolveWebviewView (
    webviewView: vscode.WebviewView,
    context: vscode.WebviewViewResolveContext,
    token: vscode.CancellationToken) {
    this._view = webviewView

    webviewView.webview.options = {
      // Allow scripts in the webview
      enableScripts: true,
      localResourceRoots: [
        this._extensionUri
      ]
    }

    webviewView.webview.html = html(
      getToolkitUri(webviewView.webview, this._extensionUri),
      getCsfScriptUri(webviewView.webview, this._extensionUri)
    )

    webviewView.webview.onDidReceiveMessage(this.receiveMessage)
  }

  pushMessage = async (data: WebViewMessage) => await this._view?.webview.postMessage(data)

  private receiveMessage = async (data: WebViewMessage) => {
    if ('type' in data && data.type in receive) {
      await receive[data.type](this, data)
    }
  }
}
