import vscode from '@tomjs/vite-plugin-vscode'
import { resolve } from 'path'
import { defineConfig } from 'vite'

// https://vitejs.dev/config/
export default defineConfig(env => {
  const vscodePlugin = vscode({
    extension: {
      tsconfig: 'tsconfig.vscode.json',
      target: 'node18',
      define: {
        'import.meta.env.MODE': env.mode,
        'import.meta.env.PROD': String(env.mode === 'production'),
        'import.meta.env.DEV': String(env.mode !== 'production'),
      },
    },
  })

  return {
    plugins: [vscodePlugin],
    resolve: {
      alias: {
        '@shimakaze.sdk/webview': resolve(__dirname, 'src'),
      },
    },
  }
})
