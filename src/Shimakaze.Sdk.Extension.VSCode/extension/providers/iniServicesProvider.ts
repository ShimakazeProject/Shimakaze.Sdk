import { client } from '@shimakaze.sdk/extension/client'
import { createDocument } from '@shimakaze.sdk/extension/utils'
import * as vscode from 'vscode'
import {
  FoldingRangeRequest,
  SemanticTokensRequest,
} from 'vscode-languageclient'

const selector: vscode.DocumentSelector = {
  language: 'ini',
}

export const iniSemanticTokensProvider = (): vscode.Disposable => {
  return vscode.languages.registerDocumentSemanticTokensProvider(
    selector,
    {
      async provideDocumentSemanticTokens(document, token) {
        const tokens = await client.instance.sendRequest(
          SemanticTokensRequest.type,
          {
            textDocument: createDocument(document),
          },
        )

        if (tokens != null) {
          return new vscode.SemanticTokens(
            new Uint32Array(tokens.data),
            tokens.resultId,
          )
        }
      },
    },
    new vscode.SemanticTokensLegend([
      'type',
      'property',
      'operator',
      'string',
      'number',
      'keyword',
      'comment',
    ]),
  )
}

export const iniFoldingRangeProvider = (): vscode.Disposable => {
  return vscode.languages.registerFoldingRangeProvider(selector, {
    async provideFoldingRanges(document, context, token) {
      const ranges = await client.instance.sendRequest(
        FoldingRangeRequest.type,
        {
          textDocument: createDocument(document),
        },
      )

      if (ranges != null) {
        return ranges.map(i => ({
          start: i.startLine,
          end: i.endLine,
          kind:
            i.kind === 'Comment'
              ? vscode.FoldingRangeKind.Comment
              : vscode.FoldingRangeKind.Region,
        }))
      }
    },
  })
}
