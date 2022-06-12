import * as vscode from 'vscode'

// Tip: Install the es6-string-html VS Code extension to enable code highlighting below
export const html = (
  toolkitUri: vscode.Uri,
  csfScriptUri: vscode.Uri
) => /* html */`
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Csf Edit Panel</title>
    <script type="module" async src="${toolkitUri}"></script>
    <script type="module" async src="${csfScriptUri}"></script>
    <script type="module" async>
      const vscode = acquireVsCodeApi()
      const element = document.querySelector('csf-editor-panel')
      const pushCsf = () => {
        vscode.postMessage({
          type: 'update',
          data: element.value
        })
      }

      window.addEventListener('message', (e) => {
        if (!e.data.type) {
          return
        }
        switch (e.data.type) {
          case "put":
            element.value = e.data.data
            break
        }
      })

      element.onchange = () => {
        pushCsf()
      }
    </script>
  </head>
  <body>
    <csf-editor-panel></csf-editor-panel>
  </body>
</html>
`
