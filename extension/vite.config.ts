import { defineConfig } from 'vite'

// https://vitejs.dev/config/
export default defineConfig({
  build: {
    target: 'node16',
    emptyOutDir: true,
    lib: {
      entry: './src/extension.ts',
      formats: ['cjs'],
      name: 'hello',
      fileName: 'extension.js'
    },
    rollupOptions: {
      external: ['vscode'],
      output: {
        entryFileNames: '[name].js',
        chunkFileNames: '[name].js',
        assetFileNames: 'assets/[name].[ext]'
      }
    }
  }
})
