import react from '@vitejs/plugin-react';
import path from 'path';
import { defineConfig } from 'vite';
import mkcert from 'vite-plugin-mkcert';

export default defineConfig({
  plugins: [
    react({
      babel: {
        plugins: [['@babel/plugin-proposal-decorators', { version: '2023-05' }]],
      },
    }),
    mkcert(),
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, 'src'),
      '@api': path.resolve(__dirname, 'src/API'),
      '@dialogs': path.resolve(__dirname, 'src/dialog'),
      '@errors': path.resolve(__dirname, 'src/errorHandling'),
      '@stores': path.resolve(__dirname, 'src/stores'),
      '@utils': path.resolve(__dirname, 'src/utils'),
      '@components': path.resolve(__dirname, 'src/components'),
      '@editors': path.resolve(__dirname, 'src/components/editors'),
      '@modules': path.resolve(__dirname, 'src/modules'),
    },
  },
  css: {
    modules: {
      generateScopedName: '[name]__[local]__[hash:base64:2]',
    },
  },
  build: {
    chunkSizeWarningLimit: 4000, // size in kB
  },
  server: {
    https: {},
    proxy: {
      '/Model': {
        target: 'https://localhost:7099',
        secure: false,
      },
      '/Package': {
        target: 'https://localhost:7099',
        secure: false,
      },
      '/Editor': {
        target: 'https://localhost:7099',
        secure: false,
      },
      '/ScreenEditor': {
        target: 'https://localhost:7099',
        secure: false,
      },
      '/SectionEditor': {
        target: 'https://localhost:7099',
        secure: false,
      },
      '/PropertyEditor': {
        target: 'https://localhost:7099',
        secure: false,
      },
      '/Documentation': {
        target: 'https://localhost:7099',
        secure: false,
      },
      '/Xslt': {
        target: 'https://localhost:7099',
        secure: false,
      },
      '/Icons': {
        target: 'http://localhost:5003',
        secure: false,
      },
      '/DeploymentScript': {
        target: 'http://localhost:5003',
        secure: false,
      },
    },
  },
});
