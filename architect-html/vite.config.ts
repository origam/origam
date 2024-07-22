import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import mkcert from 'vite-plugin-mkcert'
import path from 'path'

export default defineConfig({
  plugins: [react(),  mkcert() ],
  resolve: {
    alias: {
      src: path.resolve(__dirname, './src')
    }
  },
	css:{
		modules: {
			generateScopedName: "[name]__[local]__[hash:base64:2]"
		}
	},
  server: {
		https: true,
		proxy: {
			'/WeatherForecast':{
				target: "https://localhost:7099",
				secure: false
			},'/Model':{
				target: "https://localhost:7099",
				secure: false
			},'/Package':{
				target: "https://localhost:7099",
				secure: false
			},'/Editor':{
				target: "https://localhost:7099",
				secure: false
			},
		}
	},
})
