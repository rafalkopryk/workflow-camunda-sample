import { AppRouterCacheProvider } from '@mui/material-nextjs/v15-appRouter';
import DashboardIcon from '@mui/icons-material/Dashboard';
import type { Navigation } from '@toolpad/core/AppProvider';
import AttachMoneyIcon from '@mui/icons-material/AttachMoney';

import { AppProvider } from '@toolpad/core/AppProvider';

const NAVIGATION: Navigation = [
    {
        kind: 'header',
        title: 'Main items',
    },
    {
        segment: 'applications',
        title: 'Applications',
        icon: <DashboardIcon />,
    },
];


const BRANDING = {
    title: 'Credit application',
    Icon: <AttachMoneyIcon />
};


export default async function RootLayout(props: { children: React.ReactNode }) {


    return (
        <html lang="en" suppressHydrationWarning>
            <body >
                <AppRouterCacheProvider options={{ enableCssLayer: true }}>
                    <AppProvider
                        navigation={NAVIGATION}
                        branding={BRANDING}
                    >
                        {props.children}
                    </AppProvider>
                </AppRouterCacheProvider>
            </body>
        </html>
    );
}
