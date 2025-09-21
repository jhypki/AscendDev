import { Group, Burger, Text, Avatar, Menu, UnstyledButton, rem } from '@mantine/core'
import { IconChevronDown, IconUser, IconSettings, IconLogout } from '@tabler/icons-react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useLogout } from '../../hooks/api/useAuth'

interface User {
    id: string
    email: string
    firstName?: string
    lastName?: string
    avatar?: string
}

interface HeaderProps {
    opened: boolean
    toggle: () => void
    user: User | null
}

export function Header({ opened, toggle, user }: HeaderProps) {
    const [userMenuOpened, setUserMenuOpened] = useState(false)
    const navigate = useNavigate()
    const logoutMutation = useLogout()

    const displayName = user
        ? `${user.firstName || ''} ${user.lastName || ''}`.trim() || user.email
        : 'Guest'

    const handleLogout = async () => {
        try {
            await logoutMutation.mutateAsync()
            navigate('/login', { replace: true })
        } catch {
            // Even if logout fails on server, redirect to login
            navigate('/login', { replace: true })
        }
    }

    return (
        <Group h="100%" px="md" justify="space-between">
            <Group>
                <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" />
                <Text size="xl" fw={700} c="brand.6">
                    AscendDev
                </Text>
            </Group>

            {user && (
                <Menu
                    width={200}
                    position="bottom-end"
                    transitionProps={{ transition: 'pop-top-right' }}
                    onClose={() => setUserMenuOpened(false)}
                    onOpen={() => setUserMenuOpened(true)}
                    withinPortal
                >
                    <Menu.Target>
                        <UnstyledButton
                            style={{
                                padding: rem(8),
                                borderRadius: rem(4),
                                display: 'flex',
                                alignItems: 'center',
                                gap: rem(8),
                            }}
                        >
                            <Avatar
                                src={user.avatar}
                                alt={displayName}
                                radius="xl"
                                size={32}
                                color="brand"
                            >
                                {displayName.charAt(0).toUpperCase()}
                            </Avatar>
                            <div style={{ flex: 1 }}>
                                <Text size="sm" fw={500}>
                                    {displayName}
                                </Text>
                                <Text c="dimmed" size="xs">
                                    {user.email}
                                </Text>
                            </div>
                            <IconChevronDown
                                size={16}
                                style={{
                                    transform: userMenuOpened ? 'rotate(180deg)' : 'none',
                                    transition: 'transform 200ms ease',
                                }}
                            />
                        </UnstyledButton>
                    </Menu.Target>

                    <Menu.Dropdown>
                        <Menu.Label>Account</Menu.Label>
                        <Menu.Item leftSection={<IconUser size={16} />}>
                            Profile
                        </Menu.Item>
                        <Menu.Item leftSection={<IconSettings size={16} />}>
                            Settings
                        </Menu.Item>
                        <Menu.Divider />
                        <Menu.Item
                            leftSection={<IconLogout size={16} />}
                            color="red"
                            onClick={handleLogout}
                        >
                            Logout
                        </Menu.Item>
                    </Menu.Dropdown>
                </Menu>
            )}
        </Group>
    )
}