import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import mkcert from 'vite-plugin-mkcert'

export default defineConfig({
  plugins: [react(),  mkcert() ],
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
