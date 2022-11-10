import { TextField } from '@vscode/webview-ui-toolkit/dist/toolkit'
import { FASTElement, customElement, attr, observable } from '@microsoft/fast-element'
import type { ICsfUnit } from '../../@types/csf'
import { template } from './template'
import { styles } from './styles'

// eslint-disable-next-line no-unused-expressions
TextField

@customElement({
  name: 'csf-editor-panel',
  template,
  styles
})
export class CsfEditorPanel extends FASTElement {
  @attr labelLabel = 'Label'
  @attr tabLabel = 'Item %d'
  @attr valueLabel = 'Value'
  @attr extraLabel = 'Extra'
  @attr labelPlaceholder= 'Type a Label'
  @attr valuePlaceholder= 'Type a Value'
  @attr extraPlaceholder= 'Empty Extra'
  @attr rows: number = 5
  @observable value?: ICsfUnit = undefined

  onChange = () => this.$emit('change', this.value)

  labelChange (event: Event) {
    const newLabel = (event.target as HTMLInputElement).value
    if (!this.value) {
      this.value = {
        label: newLabel,
        values: []
      }
    } else {
      this.value.label = newLabel
    }
  }
}
