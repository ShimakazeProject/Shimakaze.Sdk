import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from 'tailwindcss'
import tailwindConfig from './tailwind.config'

// https://vitejs.dev/config/
export default defineConfig({
  cacheDir: '.yarn/.vite',
  css: {
    postcss: {
      plugins: [
        tailwindcss(tailwindConfig)
      ]
    }
  },
  resolve: {
    alias: [
      { find: '@', replacement: '/' }
    ]
  },
  plugins: [vue()]
})
