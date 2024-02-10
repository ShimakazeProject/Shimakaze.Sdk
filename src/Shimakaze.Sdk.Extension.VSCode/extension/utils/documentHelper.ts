import type * as vscode from 'vscode'
import { TextDocumentItem } from 'vscode-languageclient'

export const createDocument = (doc: vscode.TextDocument) => {
  const textDocument = TextDocumentItem.create(
    doc.uri.toString(),
    doc.languageId,
    doc.version,
    doc.getText(),
  )

  return textDocument
}
