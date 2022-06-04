import { CsfValuesEditorPanel } from '.'
import { html, repeat } from '@microsoft/fast-element'

export const template = html<CsfValuesEditorPanel>`
<vscode-panels>
${
  repeat(
    x => {
      if (typeof x.values === 'object') {
        return x.values
      }

      try {
        return JSON.parse(x.values ?? '')
      } catch (e) {
        console.error(`JSON: ${x.values}`)
        console.error(e)
        return []
      }
    },
    html`
  <vscode-panel-tab id="tab-${(_, c) => c.index}">
    <slot name="i18n-item-start">Item&nbsp;</slot>
    <span>${(_, c) => c.index}</span>
    <slot name="i18n-item-end"></slot>
  </vscode-panel-tab>
    `,
    { positioning: true }
  )
}
${
  repeat(
    x => {
      if (typeof x.values === 'object') {
        return x.values
      }

      try {
        return JSON.parse(x.values ?? '')
      } catch (e) {
        console.error(`JSON: ${x.values}`)
        console.error(e)
        return []
      }
    },
    html`
  <vscode-panel-view id="view-${(_, c) => c.index}" style="flex-flow: column;">
    <vscode-text-area
        resize="vertical"
        rows="${(_, c) => c.parent.rows}"
        value="${x => x.value}"
        placeholder="${(_, c) => c.parent.valuePlaceholder}">
      <slot name="i18n-value">Value</slot>
    </vscode-text-area>
    <vscode-text-field
        value="${x => x.extra ?? ''}"
        placeholder="${(_, c) => c.parent.extraPlaceholder}">
      <slot name="i18n-extra">Extra</slot>
    </vscode-text-field>
  </vscode-panel-view>
    `,
    { positioning: true }
  )
}
</vscode-panels>
`
