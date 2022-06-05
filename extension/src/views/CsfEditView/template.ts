import * as vscode from 'vscode'
import { CsfUnit } from '../../models/csf/CsfUnit'

// Tip: Install the es6-string-html VS Code extension to enable code highlighting below
export const html = (
  csf: CsfUnit,
  toolkitUri: vscode.Uri,
  csfScriptUri: vscode.Uri
) => /* html */`
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Csf Edit Panel</title>
    <script type="module" src="${toolkitUri}"></script>
  </head>
  <body>
    <csf-editor-panel></csf-editor-panel>
    
    <script type="module" defer src="${csfScriptUri}"></script>

    <script>
      document.querySelector('csf-editor-panel').value = ${JSON.stringify(csf)}
    </script>
  </body>
</html>
`
