import vscode from '@tomjs/vite-plugin-vscode'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'
import { defineConfig } from 'vite'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    vue({
      template: {
        compilerOptions: {
          isCustomElement: (tag: string) => tag.startsWith('vscode-'),
        },
      },
    }),
    vscode({
      extension: {
        tsconfig: 'tsconfig.vscode.json',
        target: 'node18',
      },
    }),
  ],
  resolve: {
    alias: {
      '@shimakaze.sdk/webview': resolve(__dirname, 'src'),
    },
  },
  build: {
    modulePreload: {
      polyfill: false,
    },
    rollupOptions: {
      input: {
        main: resolve(__dirname, 'index.html'),
        'shp-viewer': resolve(__dirname, 'shp-viewer.html'),
      },
      output: {
        manualChunks: (id, meta) => {
          if (id.includes('pixi.js')) {
            return 'pixi'
          }
        },
      },
    },
  },
})
