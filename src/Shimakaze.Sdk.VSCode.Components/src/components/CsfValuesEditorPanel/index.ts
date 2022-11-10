import { TextField, TextArea, Button, Panels, PanelTab, PanelView } from '@vscode/webview-ui-toolkit/dist/toolkit'
import { FASTElement, customElement, attr, observable } from '@microsoft/fast-element'
import type { ICsfValueUnit } from '../../@types/csf'
import { template } from './template'
import { styles } from './styles'

/* eslint-disable no-unused-expressions */
TextField
TextArea
Button
Panels
PanelTab
PanelView
/* eslint-enable no-unused-expressions */

@customElement({
  name: 'csf-values-editor-panel',
  template,
  styles
})
export class CsfValuesEditorPanel extends FASTElement {
  @attr rows: number = 5

  @attr tabLabel = 'Item %d'
  @attr valueLabel = 'Value'
  @attr extraLabel = 'Extra'

  @attr valuePlaceholder = 'Type a Value'
  @attr extraPlaceholder = 'Empty Extra'
  @observable values?: ICsfValueUnit[] = undefined
  @observable current: number = 0

  removeValue = (index: number) => {
    if (!this.values) {
      return
    }

    this.values?.splice(index, 1)

    this.onChange()
  }

  addValue = () => {
    if (!this.values) {
      return
    }

    this.values.push({
      value: 'New Value'
    })
    this.current = this.values.length - 1

    this.onChange()
  }

  onChange = () => this.$emit('change', this.values)

  valueChange (event: Event, index: number) {
    const newValue = (event.target as HTMLInputElement).value
    if (this.values) {
      this.values[index].value = newValue
    }
  }

  extraChange (event: Event, index: number) {
    const newValue = (event.target as HTMLInputElement).value
    if (this.values) {
      this.values[index].extra = newValue
    }
  }
}
