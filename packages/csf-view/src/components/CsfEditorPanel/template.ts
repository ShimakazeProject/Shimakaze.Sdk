import { html } from '@microsoft/fast-element'
import { CsfEditorPanel } from '.'
import { CsfValuesEditorPanel } from '../CsfValuesEditorPanel'

// eslint-disable-next-line no-unused-expressions
CsfValuesEditorPanel

export const template = html<CsfEditorPanel>`
<div style="display: flex; flex-flow: column;">
  <vscode-text-field
      value="${x => {
        if (!x.value) {
          return
        }

        console.log(x.value)

        if (typeof x.value === 'object') {
          return x.value.label
        }

        try {
          const json = JSON.parse(x.value)
          return json.label
        } catch (e) {
        console.error(`JSON: ${x.value}`)
          console.error(e)
        }
      }}"
      placeholder="${x => x.labelPlaceholder}">
    <slot name="i18n-label">Label</slot>
  </vscode-text-field>
  <csf-values-editor-panel
      rows=${x => x.rows}
      valuePlaceholder="${x => x.valuePlaceholder}"
      extraPlaceholder="${x => x.extraPlaceholder}"
      :values="${x => {
      if (!x.value) {
        return
      }

      if (typeof x.value === 'object') {
        return x.value.values
      }

      try {
        const json = JSON.parse(x.value)
        return JSON.stringify(json.values)
      } catch (e) {
        console.error(`JSON: ${x.value}`)
        console.error(e)
      }
    }}">
    <slot name="i18n-item-start" slot="i18n-item-start">Item </slot>
    <slot name="i18n-item-end" slot="i18n-item-end"></slot>
    <slot name="i18n-value" slot="i18n-value">Value</slot>
    <slot name="i18n-extra" slot="i18n-extra">Extra</slot>
  </csf-values-editor-panel>
</div>
`
