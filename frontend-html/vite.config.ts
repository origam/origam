import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tsconfigPaths from 'vite-tsconfig-paths'
import basicSsl from '@vitejs/plugin-basic-ssl'
import { NodeGlobalsPolyfillPlugin } from '@esbuild-plugins/node-globals-polyfill'
import { NodeModulesPolyfillPlugin } from '@esbuild-plugins/node-modules-polyfill'
import rollupNodePolyFill from 'rollup-plugin-node-polyfills'
import * as path from "node:path";
import * as fs from "node:fs";


function readLocalHttps() {
  const keyPath = path.resolve(__dirname, "certs", "localhost+2-key.pem");
  const certPath = path.resolve(__dirname, "certs", "localhost+2.pem");

  if (fs.existsSync(keyPath) && fs.existsSync(certPath)) {
    return {
      key: fs.readFileSync(keyPath),
      cert: fs.readFileSync(certPath),
    };
  }
  return null;
}

const localHttpsCertificate = readLocalHttps();
const useBasicSsl = !localHttpsCertificate;


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
    ...(useBasicSsl ? [basicSsl()] : []),
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
		chunkSizeWarningLimit: 4000, // size in kB
		commonjsOptions: {
			transformMixedEsModules: true
		},
		rollupOptions: {
			plugins: [
				// Enable rollup polyfills plugin
				// used during production bundling
				rollupNodePolyFill(),
			],
			onwarn(warning, warn) {
				// Filter out warnings from several packages related to the externalized "process" module
				if (
					warning.message &&
					warning.message.includes('Module "process" has been externalized for browser compatibility')
				) {
					return
				}
				// Filter out warning from react-virtualized
				if (
					warning.message &&
					warning.message.includes('no babel-plugin-flow-react-proptypes')
				) {
					return
				}
				warn(warning)
			}
		},
	},
	server: {
    https: localHttpsCertificate ?? {},
    host: "localhost",
		proxy: {
      // OpenIddict endpoints
      '/.well-known': {
        target: 'https://localhost:44357',
        secure: false,
        changeOrigin: true,
      },
      '/connect': {
        target: 'https://localhost:44357',
        secure: false,
        changeOrigin: true,
      },
      // ASP.NET Identity endpoints
      '/Account': {
        target: 'https://localhost:44357',
        secure: false,
        changeOrigin: true,
      },
      '/account': {
        target: 'https://localhost:44357',
        secure: false,
        changeOrigin: true,
      },
      // Origam.Server endpoints
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
			'/assets':{
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

