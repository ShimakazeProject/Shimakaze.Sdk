import { FASTElement, customElement, attr, html } from '@microsoft/fast-element'
import { CsfUnit, I18n } from './models'
import { JSONConverter, stringify } from './utils'

const template = html<CsfEditorPanel>`
<div style="display: flex; flex-flow: column;">
  <vscode-text-field value="${x => x.value.label}" placeholder="${x => x.i18n.labelPlaceholder}">${x => x.i18n.label}</vscode-text-field>
  <csf-values-editor-panel value="${x => stringify(x.value.values)}" rows=${x => x.rows} i18n="${x => stringify(x.i18n)}"></csf-values-editor-panel>
</div>
`

@customElement({
  name: 'csf-editor-panel',
  template
})
export class CsfEditorPanel extends FASTElement {
  @attr({ converter: JSONConverter }) value: CsfUnit = {} as any
  @attr rows: string = '10'
  @attr({ converter: JSONConverter }) i18n: I18n = stringify({
    label: 'Label',
    labelPlaceholder: 'Type a Label',
    item: 'Item',
    value: 'Value',
    valuePlaceholder: 'Type a Value',
    extra: 'Extra',
    extraPlaceholder: 'Empty Extra'
  }) as any
}
