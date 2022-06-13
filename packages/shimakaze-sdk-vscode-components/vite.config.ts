import { defineConfig, UserConfig } from 'vite'

// https://vitejs.dev/config/
export default defineConfig(({ command, mode }) => {
  const options: UserConfig = {
    cacheDir: '.vite',
    build: {
      emptyOutDir: false,
      rollupOptions: {
        input: {
          'components-csf': './src/csf.ts'
        },
        output: {
          entryFileNames: '[name].js',
          chunkFileNames: '[name].js',
          assetFileNames: 'assets/[name].[ext]',
          manualChunks: (id, { getModuleInfo }) => {
            if (id.includes('@microsoft/fast-element')) {
              return 'deps/@microsoft/fast-element'
            }
            if (id.includes('@vscode/webview-ui-toolkit')) {
              return 'deps/@vscode/webview-ui-toolkit'
            }
          }
        }
      },
      minify: false,
      sourcemap: true
    },
    optimizeDeps: {
      exclude: [
        '@microsoft/fast-element'
      ]
    }
  }

  if (command === 'build') {
    options.build!.outDir = '../../extension/dist'
  }
  return options
})
