import { html } from '@microsoft/fast-element'
import { CsfEditorPanel } from '.'
import { ICsfUnit } from '../../@types/csf'
import { CsfValuesEditorPanel } from '../CsfValuesEditorPanel'

// eslint-disable-next-line no-unused-expressions
CsfValuesEditorPanel

const getValue = (view: CsfEditorPanel): ICsfUnit | undefined => {
  if (!view.value) {
    return
  }

  return view.value
}

export const template = html<CsfEditorPanel, CsfEditorPanel>`
<vscode-text-field
  :value="${x => getValue(x)?.label}"
  :placeholder="${x => x.labelPlaceholder}"
  @change="${(x, c) => x.labelChange(c.event)}">
  ${x => x.labelLabel}
</vscode-text-field>
<csf-values-editor-panel
  :rows=${x => x.rows}
  :tabLabel="${x => x.tabLabel}"
  :valueLabel="${x => x.valueLabel}"
  :extraLabel="${x => x.extraLabel}"
  :valuePlaceholder="${x => x.valuePlaceholder}"
  :extraPlaceholder="${x => x.extraPlaceholder}"
  :values="${x => getValue(x)?.values}"
  @change="${x => x.onChange()}">
</csf-values-editor-panel>
`
