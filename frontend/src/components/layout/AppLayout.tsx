import { AppShell, Group, Text, UnstyledButton, rem } from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { IconHome, IconBook, IconUser, IconSettings, IconLogout, IconShield, IconCode } from '@tabler/icons-react'
import { useSelector } from 'react-redux'
import { useNavigate, useLocation } from 'react-router-dom'
import type { RootState } from '../../store'
import { useLogout } from '../../hooks/api/useAuth'
import { Header } from './Header'
import { Navigation } from './Navigation'
import { Footer } from './Footer'
import { ThemeToggle } from './ThemeToggle'

interface AppLayoutProps {
    children: React.ReactNode
}

export function AppLayout({ children }: AppLayoutProps) {
    const [opened, { toggle }] = useDisclosure(true) // Start with sidebar open
    const { user, isAuthenticated } = useSelector((state: RootState) => state.auth)
    const navigate = useNavigate()
    const location = useLocation()
    const logoutMutation = useLogout()

    const handleLogout = async () => {
        try {
            await logoutMutation.mutateAsync()
            navigate('/login', { replace: true })
        } catch {
            // Even if logout fails on server, redirect to login
            navigate('/login', { replace: true })
        }
    }

    const navigationItems = [
        { icon: IconHome, label: 'Dashboard', href: '/dashboard' },
        { icon: IconBook, label: 'Courses', href: '/courses' },
        { icon: IconCode, label: 'Playground', href: '/playground' },
        { icon: IconUser, label: 'Profile', href: '/profile' },
        { icon: IconSettings, label: 'Settings', href: '/settings' },
    ]

    // Add admin dashboard link for Admin and SuperAdmin users
    console.log('AppLayout - User data:', user)
    console.log('AppLayout - User roles:', user?.userRoles)

    if (user?.userRoles?.some((role: string) => ['Admin', 'SuperAdmin'].includes(role))) {
        console.log('AppLayout - Adding admin dashboard link')
        navigationItems.splice(2, 0, { icon: IconShield, label: 'Admin Dashboard', href: '/admin' })
    } else {
        console.log('AppLayout - User does not have admin roles or user data is missing')
    }

    if (!isAuthenticated) {
        return <>{children}</>
    }

    return (
        <AppShell
            header={{ height: 60 }}
            navbar={{
                width: 300,
                breakpoint: 'sm',
                collapsed: { mobile: !opened, desktop: !opened },
            }}
            padding="md"
        >
            <AppShell.Header>
                <Header opened={opened} toggle={toggle} user={user} />
            </AppShell.Header>

            <AppShell.Navbar p="md">
                <AppShell.Section grow>
                    <Navigation
                        items={navigationItems}
                        currentPath={location.pathname}
                        onNavigate={(href: string) => {
                            navigate(href)
                            // Don't auto-close sidebar on navigation
                        }}
                    />
                </AppShell.Section>

                <AppShell.Section>
                    <Group justify="space-between" p="md">
                        <ThemeToggle />
                        <UnstyledButton
                            onClick={handleLogout}
                            style={{
                                display: 'flex',
                                alignItems: 'center',
                                gap: rem(8),
                                padding: rem(8),
                                borderRadius: rem(4),
                                color: 'var(--mantine-color-red-6)',
                            }}
                        >
                            <IconLogout size={16} />
                            <Text size="sm">Logout</Text>
                        </UnstyledButton>
                    </Group>
                </AppShell.Section>
            </AppShell.Navbar>

            <AppShell.Main>
                {children}
                <Footer />
            </AppShell.Main>
        </AppShell>
    )
}