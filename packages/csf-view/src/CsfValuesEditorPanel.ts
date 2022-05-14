import { FASTElement, customElement, attr, html, repeat } from '@microsoft/fast-element'
import { CsfValueUnit, I18n } from './models'
import { JSONConverter } from './utils'

const template = html<CsfValuesEditorPanel>`
<vscode-panels>
${repeat(x => x.value, html<CsfValueUnit>`<vscode-panel-tab id="tab-${(_, c) => c.index}">${(x, c) => `${c.parent.i18n.item} ${c.index}`}</vscode-panel-tab>`, { positioning: true })}
${repeat(x => x.value, html<CsfValueUnit>`<vscode-panel-view id="view-${(_, c) => c.index}" style="flex-flow: column;">
  <vscode-text-area value="${x => x.value}" rows="${(_, c) => c.parent.rows}" resize="vertical"  placeholder="${(_, c) => c.parent.i18n.valuePlaceholder}">${(_, c) => c.parent.i18n.value}</vscode-text-area>
  <vscode-text-field value="${x => x.extra ?? ''}" placeholder="${(_, c) => c.parent.i18n.extraPlaceholder}">${(_, c) => c.parent.i18n.extra}</vscode-text-field>
</vscode-panel-view>`, { positioning: true })}
</vscode-panels>
`
@customElement({
  name: 'csf-values-editor-panel',
  template
})
export class CsfValuesEditorPanel extends FASTElement {
  @attr({ converter: JSONConverter }) value: CsfValueUnit[] = []
  @attr rows: string = '10'
  @attr({ converter: JSONConverter }) i18n: I18n = {
    item: 'Item',
    value: 'Value',
    valuePlaceholder: 'Type a Value',
    extra: 'Extra',
    extraPlaceholder: 'Empty Extra'
  }
}
