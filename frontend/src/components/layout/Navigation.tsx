import { NavLink, Stack } from '@mantine/core'
import type { Icon } from '@tabler/icons-react'

interface NavigationItem {
    icon: Icon
    label: string
    href: string
}

interface NavigationProps {
    items: NavigationItem[]
    currentPath: string
    onNavigate: (href: string) => void
}

export function Navigation({ items, currentPath, onNavigate }: NavigationProps) {
    return (
        <Stack gap="xs">
            {items.map((item) => {
                const IconComponent = item.icon
                const isActive = currentPath === item.href ||
                    (item.href !== '/' && currentPath.startsWith(item.href))

                return (
                    <NavLink
                        key={item.href}
                        href={item.href}
                        label={item.label}
                        leftSection={<IconComponent size={20} />}
                        active={isActive}
                        onClick={(event) => {
                            event.preventDefault()
                            onNavigate(item.href)
                        }}
                        style={{
                            borderRadius: '8px',
                        }}
                    />
                )
            })}
        </Stack>
    )
}