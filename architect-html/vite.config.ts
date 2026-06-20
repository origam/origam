import babel from '@rolldown/plugin-babel';
import react from '@vitejs/plugin-react';
import path from 'path';
import { defineConfig } from 'vite';
import mkcert from 'vite-plugin-mkcert';

// Allow disabling the HTTPS dev server (and mkcert) via an env flag. mkcert
// can't provision a local CA on CI runners, so the HTTPS server never starts
// there; the e2e job sets VITE_DISABLE_HTTPS=true to run over plain HTTP.
// Local development keeps HTTPS by default.
const httpsDisabled = process.env.VITE_DISABLE_HTTPS === 'true';

export default defineConfig({
  define: {
    __ORIGAM_ARCHITECT_HTML_VERSION__: JSON.stringify(
      process.env.VITE_ORIGAM_ARCHITECT_HTML_VERSION ?? 'dev',
    ),
  },
  plugins: [
    react(),
    babel({
      plugins: [['@babel/plugin-proposal-decorators', { version: '2023-05' }]],
    }),
    ...(httpsDisabled ? [] : [mkcert()]),
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
    ...(httpsDisabled ? {} : { https: {} }),
    proxy: {
      '/Model': {
        target: 'https://localhost:7099',
        secure: false,
      },
      '/Package': {
        target: 'https://localhost:7099',
        secure: false,
      },
      '/Tab': {
        target: 'https://localhost:7099',
        secure: false,
      },
      '/Test': {
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
      '/DeploymentScripts': {
        target: 'http://localhost:5003',
        secure: false,
      },
      '/DeploymentScriptsGenerator': {
        target: 'http://localhost:5003',
        secure: false,
      },
      '/Search': {
        target: 'http://localhost:5003',
        secure: false,
      },
    },
  },
});
