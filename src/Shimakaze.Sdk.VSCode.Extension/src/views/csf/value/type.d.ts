import type { WebViewMessage } from '../../../@types/WebViewMessage'
import type { CsfValueViewProvider } from './provider'

export interface ReceiveMethod {
  (sender: CsfValueViewProvider, data: WebViewMessage): void | Promise<void>
}

export interface Receives {
  [key: string]: ReceiveMethod;
}
