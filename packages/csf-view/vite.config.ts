import { defineConfig, UserConfig } from 'vite'

// https://vitejs.dev/config/
export default defineConfig(({ command, mode }) => {
  const options: UserConfig = {
    cacheDir: '.vite',
    build: {
      emptyOutDir: true,
      rollupOptions: {
        output: {
          entryFileNames: '[name].js',
          chunkFileNames: '[name].js',
          assetFileNames: 'assets/[name].[ext]'
        }
      }
    },
    optimizeDeps: {
      exclude: [
        '@microsoft/fast-element'
      ]
    }
  }

  if (command === 'build') {
    options.build!.lib = {
      entry: './src/main.ts',
      formats: ['es']
    }
    options.build!.rollupOptions!.external = [
      '@vscode/webview-ui-toolkit'
    ]
  }
  return options
})
