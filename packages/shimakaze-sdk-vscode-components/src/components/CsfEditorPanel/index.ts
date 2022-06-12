import { FASTElement, customElement, attr, observable } from '@microsoft/fast-element'
import type { CsfUnit } from '../../@types/csf'
import { template } from './template'
import { styles } from './styles'

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
  @observable value?: CsfUnit = undefined

  onChange = () => this.$emit('change', this.value)
}
