import path from 'path';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import fs from 'fs';

const __dirname = 'C:/Users/Dubster/.aspnet/https';

export default defineConfig({
  base: './',
  plugins: [
    react(),
  ],
  server: {
    host: true,
    open: true,
    port: 3000,
    allowedHosts: ['surrounding-managers-fold-assessing.trycloudflare.com'],
    // https: {
    //   key: fs.readFileSync(path.resolve(__dirname, './key.pem')),
    //   cert: fs.readFileSync(path.resolve(__dirname, './cert.pem')),
    // },
  },
  build: {
    outDir: 'build',
  },
});