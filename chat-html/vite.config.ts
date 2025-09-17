import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tsconfigPaths from 'vite-tsconfig-paths'
import basicSsl from '@vitejs/plugin-basic-ssl'

// https://vitejs.dev/config/
export default defineConfig({
  logLevel: 'warn',
  plugins: [
    react({
      babel: {
        plugins: [
          [
            "@babel/plugin-proposal-decorators",
            { legacy: true }
          ],
          [
            "@babel/plugin-proposal-class-properties",
            { loose: true },
          ],
        ],
      },
    }),
    tsconfigPaths(),
    basicSsl()
  ],
  resolve: {
    alias: [
      {
        // this is required for the SCSS modules
        find: /^~(.*)$/,
        replacement: '$1',
      },
      {
        find: 'stream',
        replacement: `stream-browserify`,
      },
      {
        find: 'buffer',
        replacement: 'buffer',
      },
      {
        find: 'events',
        replacement: 'events',
      },
      {
        find: 'react-virtualized',
        replacement: 'react-virtualized/dist/commonjs',
      },
    ],
  },
  optimizeDeps: {
    include: ['buffer', 'events', 'stream-browserify', 'readable-stream', 'safe-buffer']
  },
  css:{
    modules: {
      generateScopedName: "[name]__[local]__[hash:base64:2]"
    }
  },
  build: {
    chunkSizeWarningLimit: 3000,
    commonjsOptions: {
      transformMixedEsModules: true
    },
    rollupOptions: {
      onwarn(warning, warn) {
        const msg = typeof warning.message === 'string' ? warning.message : '';
        if (msg.includes('externalized for browser compatibility')) {
          return;
        }
        if ((warning as any).code === 'chunkSize') {
          return;
        }
        warn(warning);
      },
      output: {
        assetFileNames: (assetInfo) => {
          return `chatAssets/[name]-[hash][extname]`;
        },
        chunkFileNames: 'chatAssets/[name]-[hash].js',
        entryFileNames: 'chatAssets/[name]-[hash].js'
      }
    }
  },
  server: {
    https: true,
    proxy: {
      '/internalApi':{
        target: "https://localhost:44357",
        secure: false
      },
      '/customAssets':{
        target: "https://localhost:44357",
        secure: false
      },
      '/chatrooms':{
        target: "https://localhost:44357",
        secure: false
      },
      '/api':{
        target: "https://localhost:44357",
        secure: false
      },
      '/connect':{
        target: "https://localhost:44357",
        secure: false
      },
      '/assets':{
        target: "https://localhost:44357",
        secure: false
      },
      '/Account':{
        target: "https://localhost:44357",
        secure: false
      },
      '/account':{
        target: "https://localhost:44357",
        secure: false
      },
      '/.well-known':{
        target: "https://localhost:44357",
        secure: false
      },
      '/home':{
        target: "https://localhost:44357",
        secure: false
      },
      '/locale':{
        target: "https://localhost:44357",
        secure: false
      }
    }
  },
  define: {
    global: 'window',
  }
})

