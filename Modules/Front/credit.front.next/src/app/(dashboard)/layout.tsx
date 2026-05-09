'use client';

import * as React from 'react';
import Link from 'next/link';
import Box from '@mui/material/Box';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Drawer from '@mui/material/Drawer';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import ListSubheader from '@mui/material/ListSubheader';
import Typography from '@mui/material/Typography';
import DashboardIcon from '@mui/icons-material/Dashboard';
import AttachMoneyIcon from '@mui/icons-material/AttachMoney';

const DRAWER_WIDTH = 240;

export default function DashboardPagesLayout(props: { children: React.ReactNode }) {
    return (
        <Box sx={{ display: 'flex' }}>
            <AppBar position="fixed" sx={{ zIndex: (t) => t.zIndex.drawer + 1 }}>
                <Toolbar>
                    <AttachMoneyIcon sx={{ mr: 1 }} />
                    <Typography variant="h6" noWrap component="div">
                        Credit application
                    </Typography>
                </Toolbar>
            </AppBar>
            <Drawer
                variant="permanent"
                sx={{
                    width: DRAWER_WIDTH,
                    flexShrink: 0,
                    [`& .MuiDrawer-paper`]: { width: DRAWER_WIDTH, boxSizing: 'border-box' },
                }}
            >
                <Toolbar />
                <Box sx={{ overflow: 'auto' }}>
                    <List subheader={<ListSubheader component="div">Main items</ListSubheader>}>
                        <ListItem disablePadding>
                            <ListItemButton component={Link} href="/">
                                <ListItemIcon><DashboardIcon /></ListItemIcon>
                                <ListItemText primary="Applications" />
                            </ListItemButton>
                        </ListItem>
                    </List>
                </Box>
            </Drawer>
            <Box component="main" sx={{ flexGrow: 1, p: 3 }}>
                <Toolbar />
                {props.children}
            </Box>
        </Box>
    );
}
