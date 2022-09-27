import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
    base: "/packages/",
    plugins: [react()],
    server: {
        watch: {
            ignored: [
                "**/*.fs"
            ]
        },
        port: 3000
    }
})
