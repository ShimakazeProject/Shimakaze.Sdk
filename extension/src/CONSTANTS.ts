import * as vscode from 'vscode'
import { getUri } from './utils/getUri'

/**
 * 获取 @vscode/webview-ui-toolkit 的Uri
 * @param webview webview 对象
 * @param extensionUri VSCode扩展当前Uri
 * @returns webview-ui-toolkit 的Uri
 */
export const getToolkitUri = (webview: vscode.Webview, extensionUri: vscode.Uri) => getUri(webview, extensionUri, [
  'dist',
  'deps',
  '@vscode',
  'webview-ui-toolkit.js'
])

/**
 * 获取 shimakaze-sdk-vscode-components 的Uri
 * @param webview webview 对象
 * @param extensionUri VSCode扩展当前Uri
 * @returns shimakaze-sdk-vscode-components 的Uri
 */
export const getCsfScriptUri = (webview: vscode.Webview, extensionUri: vscode.Uri) => getUri(webview, extensionUri, [
  'dist',
  'components-csf.js'
])
