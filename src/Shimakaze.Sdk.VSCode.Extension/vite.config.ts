import { defineConfig } from 'vite'

// https://vitejs.dev/config/
export default defineConfig({
  build: {
    target: 'node16',
    emptyOutDir: false,
    lib: {
      entry: './src/extension.ts',
      formats: ['cjs'],
      name: 'hello',
      fileName: 'extension.js'
    },
    rollupOptions: {
      external: ['vscode', '@vscode/webview-ui-toolkit'],
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
    include: [
      '@vscode/webview-ui-toolkit'
    ]
  }
})
