import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tsconfigPaths from 'vite-tsconfig-paths'
import basicSsl from '@vitejs/plugin-basic-ssl'
import { NodeGlobalsPolyfillPlugin } from '@esbuild-plugins/node-globals-polyfill'
import { NodeModulesPolyfillPlugin } from '@esbuild-plugins/node-modules-polyfill'
import rollupNodePolyFill from 'rollup-plugin-node-polyfills'
import path from 'path';

// https://vitejs.dev/config/
export default defineConfig({
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
				find: 'events',
				replacement: 'rollup-plugin-node-polyfills/polyfills/events',
			},
			{
				find: 'buffer',
				replacement: 'rollup-plugin-node-polyfills/polyfills/buffer-es6',
			}
		],
	},
	css:{
		modules: {
			generateScopedName: "[name]__[local]__[hash:base64:2]"
		},
		preprocessorOptions: {
      scss: {
        api: 'modern',
				loadPaths: ['./'],
      }
    }
	},
	optimizeDeps: {
		esbuildOptions: {
			// Node.js global to browser globalThis
			define: {
				global: 'globalThis'
			},
			// Enable esbuild polyfill plugins
			plugins: [
				NodeGlobalsPolyfillPlugin({
					process: true,
					buffer: true
				}),
				NodeModulesPolyfillPlugin()
			]
		}
	},
	build: {
		commonjsOptions: {
			transformMixedEsModules: true
		},
		rollupOptions: {
			plugins: [
				// Enable rollup polyfills plugin
				// used during production bundling
				rollupNodePolyFill(),
			],
		},
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

