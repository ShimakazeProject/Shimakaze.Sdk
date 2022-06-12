import { template } from './template'
import type { CsfValueUnit } from '../../@types/csf'
import { FASTElement, customElement, attr, observable } from '@microsoft/fast-element'
import { styles } from './styles'

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
  @observable values?: CsfValueUnit[] = undefined
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
}
