import { CsfValuesEditorPanel } from '.'
import { html, repeat, ViewTemplate } from '@microsoft/fast-element'
import { CsfValueUnit } from '../../@types/csf'

const getValues = (view: CsfValuesEditorPanel): CsfValueUnit[] => {
  if (!view.values) {
    return []
  }

  return view.values
}

const eachValue = (template: ViewTemplate<CsfValueUnit, CsfValuesEditorPanel>) => repeat(x => getValues(x), template, { positioning: true })

export const template = html<CsfValuesEditorPanel>`
<vscode-panels :activeid="tab-${(x) => x.current}">
  <vscode-button slot="start" appearance="icon" @click="${x => x.addValue()}">+</vscode-button>
${
  eachValue(html<CsfValueUnit, CsfValuesEditorPanel>`
  <vscode-panel-tab :id="tab-${(_, c) => c.index}">
    <span>${(_, c) => c.parent.tabLabel.replace('%d', c.index.toString())}</span>
    <vscode-button appearance="icon" @click="${(_, x) => x.parent.removeValue(x.index)}">&#x00d7;</vscode-button>
  </vscode-panel-tab>
`)
  }
${
  eachValue(html<CsfValueUnit, CsfValuesEditorPanel>`
  <vscode-panel-view :id="view-${(_, c) => c.index}">
    <vscode-text-area
        resize="vertical"
        :rows="${(_, c) => c.parent.rows}"
        :value="${x => x.value}"
        :placeholder="${(_, c) => c.parent.valuePlaceholder}"
        @input="${(_, c) => c.parent.onChange()}">
      ${(_, c) => c.parent.valueLabel}
    </vscode-text-area>
    <vscode-text-field
        :value="${x => x.extra ?? ''}"
        :placeholder="${(_, c) => c.parent.extraPlaceholder}"
        @input="${(_, c) => c.parent.onChange()}">
      ${(_, c) => c.parent.extraLabel}
    </vscode-text-field>
  </vscode-panel-view>
`)
}
</vscode-panels>
`
