import __getWebviewHtml__ from '@tomjs/vscode-extension-webview'
import * as crypto from 'node:crypto'
import * as fs from 'node:fs/promises'
import * as vscode from 'vscode'
import * as htmlparser2 from 'htmlparser2'
import render from 'dom-serializer'
import * as selector from 'css-select'

/**
 *
 * @param option 设置
 * @returns
 */
export const getHtml = async (option: {
  enterPoint: string
  extensionUri: vscode.Uri
  cspSource: string
  dependencies?: string[]
}) => {
  if (process.env.VITE_DEV_SERVER_URL) {
    console.log(
      'VITE_DEV_SERVER_URL:',
      process.env.VITE_DEV_SERVER_URL + option.enterPoint,
    )

    return __getWebviewHtml__(
      process.env.VITE_DEV_SERVER_URL + option.enterPoint,
    )
  } else {
    const { enterPoint, extensionUri, cspSource, dependencies = [] } = option
    const nonce = crypto.randomUUID()
    const base = vscode.Uri.joinPath(extensionUri, 'dist', 'webview')
    const assets = vscode.Uri.joinPath(base, 'assets')
    const csp =
      [
        `default-src ${cspSource} data: blob:`,
        `style-src ${cspSource} 'unsafe-inline'`,
        `script-src ${cspSource} blob: 'unsafe-eval' 'nonce-${nonce}'`,
      ].join('; ') + ';'

    const page = vscode.Uri.joinPath(base, enterPoint + '.html')

    const css = vscode.Uri.joinPath(assets, enterPoint + '.css')
    const jss = dependencies.map(i => vscode.Uri.joinPath(assets, i + '.js'))
    jss.push(vscode.Uri.joinPath(assets, enterPoint + '.js'))

    const content = await fs.readFile(page.fsPath, { encoding: 'utf-8' })

    const doc = htmlparser2.parseDocument(content)

    const head = selector.selectOne('head', doc)
    ;[
      `<meta http-equiv="Content-Security-Policy" content="${csp}" />`,
      `<link rel="stylesheet" crossorigin href="${css}" />`,
    ].forEach(i => head.childNodes.push(htmlparser2.parseDocument(i)))

    const body = selector.selectOne('body', doc)
    // 清除 script
    body.childNodes = body.childNodes.filter(
      i => i.type != htmlparser2.ElementType.Script,
    )

    jss.map(i =>
      body.childNodes.push(
        htmlparser2.parseDocument(
          `<script type="module" crossorigin nonce="${nonce}" src="${i}"></script>`,
        ),
      ),
    )

    return render(doc)
  }
}
