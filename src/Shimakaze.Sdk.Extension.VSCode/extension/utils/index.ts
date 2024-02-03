import * as vscode from 'vscode'

export const getCacheFolder = (base: vscode.Uri, ...paths: string[]) =>
  vscode.Uri.joinPath(base, '.shimakaze', ...paths)
