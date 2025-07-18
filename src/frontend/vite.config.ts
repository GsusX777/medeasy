// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';

export default defineConfig({
	plugins: [sveltekit()],
	// Prevent Vite from trying to server-side render Tauri's API [ZTS]
	ssr: {
		external: ['@tauri-apps/api', '@tauri-apps/api/tauri']
	}
});
