import * as vscode from 'vscode'
import type { CSFJsonV2Schema } from '../../../../@types/CsfSchemaExt/csf-v2'
import { CsfNode } from '../CsfNode'
import { parseJsonV2 } from './parseJsonV2'

export const parseNodes = async (textEditor: vscode.TextEditor | undefined, log: vscode.OutputChannel): Promise<CsfNode[] | undefined> => {
  if (!textEditor) {
    return
  }

  const content = textEditor.document.getText()

  try {
    const json = JSON.parse(content)
    if (!('protocol' in json) || typeof json.protocol !== 'number') {
      return
    }
    switch (json.protocol) {
      case 1:
        log.appendLine('Not Impliment Protocol')

        break
      case 2:
        return parseJsonV2(json as CSFJsonV2Schema)
      default:
        log.appendLine('Not Supported Protocol')
        break
    }
  } catch (e: any) {
    log.appendLine('Invalid JSON file')
    if ('message' in e) {
      log.appendLine(e.message)
    }
  }
}
