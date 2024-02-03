import { createApp } from 'vue'
import App from './App.vue'
import {
  allComponents,
  provideVSCodeDesignSystem,
} from '@vscode/webview-ui-toolkit'

provideVSCodeDesignSystem().register(allComponents)
createApp(App).mount('#app')
