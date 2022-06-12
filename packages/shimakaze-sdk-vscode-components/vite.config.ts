import { defineConfig, UserConfig } from 'vite'

// https://vitejs.dev/config/
export default defineConfig(({ command, mode }) => {
  const options: UserConfig = {
    cacheDir: '.vite',
    build: {
      emptyOutDir: false,
      rollupOptions: {
        output: {
          entryFileNames: '[name].js',
          chunkFileNames: '[name].js',
          assetFileNames: 'assets/[name].[ext]'
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
    options.build!.lib = {
      entry: './src/shimakaze-sdk-vscode-components.ts',
      formats: ['es']
    }
    options.build!.rollupOptions!.external = [
      '@vscode/webview-ui-toolkit'
    ]
  }
  return options
})
