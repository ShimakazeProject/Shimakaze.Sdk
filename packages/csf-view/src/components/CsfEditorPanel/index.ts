import { FASTElement, customElement, attr, observable } from '@microsoft/fast-element'
import { CsfUnit } from '../../models'
import { template } from './template'

@customElement({
  name: 'csf-editor-panel',
  template
})
export class CsfEditorPanel extends FASTElement {
  @attr labelPlaceholder= 'Type a Label'
  @attr valuePlaceholder= 'Type a Value'
  @attr extraPlaceholder= 'Empty Extra'
  @attr rows: number = 10
  @observable value: CsfUnit | string = {}
}
