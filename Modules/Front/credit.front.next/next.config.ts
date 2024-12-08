import type { NextConfig } from "next";

const nextConfig: NextConfig = {
    reactStrictMode: false,
    env: {
        APPLICATION_URL: 'https://localhost:63111'
    },
};

export default nextConfig;


