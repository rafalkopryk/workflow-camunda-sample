import * as React from 'react';
import { AppProvider } from '@toolpad/core/nextjs';
import { AppRouterCacheProvider } from '@mui/material-nextjs/v15-appRouter';
import DashboardIcon from '@mui/icons-material/Dashboard';
import AttachMoneyIcon from '@mui/icons-material/AttachMoney';
import LinearProgress from '@mui/material/LinearProgress'
import type { Navigation } from '@toolpad/core/AppProvider';

import theme from '../../theme';

const NAVIGATION: Navigation = [
    {
        kind: 'header',
        title: 'Main items',
    },
    {
        segment: '',
        title: 'Applications',
        icon: <DashboardIcon />,
    },
];


const BRANDING = {
    title: 'Credit application',
    Icon: <AttachMoneyIcon />,
};


export default async function RootLayout(props: { children: React.ReactNode }) {

    return (
        <html lang="en" data-toolpad-color-scheme="light" suppressHydrationWarning>
            <body>

                <AppRouterCacheProvider options={{ enableCssLayer: true }}>
                    <React.Suspense fallback={<LinearProgress />}>
                        <AppProvider
                            navigation={NAVIGATION}
                            branding={BRANDING}

                            theme={theme}
                        >
                            {props.children}
                        </AppProvider>
                    </React.Suspense>
                </AppRouterCacheProvider>

            </body>
        </html>
    );

    //return (
    //    <html lang="en" suppressHydrationWarning>
    //        <body >
    //            <AppRouterCacheProvider options={{ enableCssLayer: true }}>
    //                <AppProvider
    //                    navigation={NAVIGATION}
    //                    branding={BRANDING}
    //                >
    //                    {props.children}
    //                </AppProvider>
    //            </AppRouterCacheProvider>
    //        </body>
    //    </html>
    //);
}
