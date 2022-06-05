import { getUri } from './utils/getUri'
import * as vscode from 'vscode'

/**
 * 获取 @vscode/webview-ui-toolkit 的Uri
 * @param webview webview 对象
 * @param extensionUri VSCode扩展当前Uri
 * @returns webview-ui-toolkit 的Uri
 */
export const getToolkitUri = (webview: vscode.Webview, extensionUri: vscode.Uri) => getUri(webview, extensionUri, [
  '.yarn',
  'unplugged',
  '@vscode-webview-ui-toolkit-virtual-ed0c2df74e',
  'node_modules',
  '@vscode',
  'webview-ui-toolkit',
  'dist',
  'toolkit.min.js'
])

/**
 * 获取 shimakaze-sdk-vscode-components 的Uri
 * @param webview webview 对象
 * @param extensionUri VSCode扩展当前Uri
 * @returns shimakaze-sdk-vscode-components 的Uri
 */
export const getCsfScriptUri = (webview: vscode.Webview, extensionUri: vscode.Uri) => getUri(webview, extensionUri, [
  'dist',
  'shimakaze-sdk-vscode-components.js'
])
