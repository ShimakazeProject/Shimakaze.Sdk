import '@vscode/webview-ui-toolkit/dist/toolkit'

import './CsfEditorPanel'
import './CsfValuesEditorPanel'
import { stringify } from './utils'

const e = document.querySelector('csf-editor-panel')

e?.setAttribute('value', stringify(JSON.parse(e.textContent!)))
