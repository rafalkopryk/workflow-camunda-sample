import type { NextConfig } from "next";

const nextConfig: NextConfig = {
    reactStrictMode: false,
    env: {
        APPLICATION_URL: 'https://localhost:7184'
    },
};

export default nextConfig;


