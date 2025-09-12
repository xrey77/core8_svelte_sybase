import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';

export default defineConfig({
	plugins: [sveltekit()],
	server: {
		hmr: { overlay: false }
	},	
	build: {
		cssCodeSplit: true, // This is the default, but ensures it's not disabled.
	}		
});
