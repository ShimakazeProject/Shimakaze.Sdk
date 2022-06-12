interface WebViewMessageBase{
  type: string
}

export interface WebViewMessageWithData<T = any> extends WebViewMessageBase{
  type: string,
  data: T
}
export type WebViewMessage = WebViewMessageBase | WebViewMessageWithData;
