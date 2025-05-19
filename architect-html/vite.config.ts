import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import mkcert from 'vite-plugin-mkcert'
import path from 'path'

export default defineConfig({
  plugins: [
		react({
			babel: {
				plugins: [
					[
						"@babel/plugin-proposal-decorators",
						{ "version": "2023-05" }
					]
				],
			},
		}),
		mkcert() ],
  resolve: {
    alias: {
      src: path.resolve(__dirname, './src')
    }
  },
  css: {
    modules: {
      generateScopedName: "[name]__[local]__[hash:base64:2]"
    }
  },
  build: {
    chunkSizeWarningLimit: 4000, // size in kB
  },
  server: {
    https: {},
    proxy: {
      '/Model': {
        target: "https://localhost:7099",
        secure: false
      }, '/Package': {
        target: "https://localhost:7099",
        secure: false
      }, '/Editor': {
        target: "https://localhost:7099",
        secure: false
      }, '/ScreenEditor': {
        target: "https://localhost:7099",
        secure: false
      }, '/SectionEditor': {
        target: "https://localhost:7099",
        secure: false
      }, '/PropertyEditor': {
        target: "https://localhost:7099",
        secure: false
      }, '/Icons': {
        target: "http://localhost:5003",
        secure: false
      },
    }
  },
})
