import { ActionIcon, Avatar, Group, Paper, Text, UnstyledButton, rem, useMantineTheme, useComputedColorScheme } from '@mantine/core'
import { IconX, IconBell, IconCode, IconTrophy, IconMessage } from '@tabler/icons-react'
import dayjs from 'dayjs'
import relativeTime from 'dayjs/plugin/relativeTime'
import type { Notification, NotificationType } from '../../types/notification'

dayjs.extend(relativeTime)

interface NotificationItemProps {
    notification: Notification
    onMarkAsRead: (id: string) => void
    onDelete: (id: string) => void
    onClick?: (notification: Notification) => void
}

const getNotificationIcon = (type: NotificationType) => {
    switch (type) {
        case 'CodeReview':
            return <IconCode size={20} />
        case 'Achievement':
            return <IconTrophy size={20} />
        case 'Discussion':
        case 'Social':
            return <IconMessage size={20} />
        default:
            return <IconBell size={20} />
    }
}

const getNotificationColor = (type: NotificationType) => {
    switch (type) {
        case 'CodeReview':
            return 'blue'
        case 'Achievement':
            return 'yellow'
        case 'Discussion':
        case 'Social':
            return 'green'
        case 'System':
            return 'red'
        default:
            return 'gray'
    }
}

export function NotificationItem({ notification, onMarkAsRead, onDelete, onClick }: NotificationItemProps) {
    const theme = useMantineTheme()
    const computedColorScheme = useComputedColorScheme('light')

    const handleClick = () => {
        if (!notification.isRead) {
            onMarkAsRead(notification.id)
        }
        onClick?.(notification)
    }

    const timeAgo = dayjs(notification.createdAt).fromNow()

    // Use theme-aware colors that work in both light and dark mode
    const unreadBgColor = computedColorScheme === 'dark'
        ? 'rgba(34, 139, 230, 0.15)' // More transparent blue for dark mode
        : 'rgba(34, 139, 230, 0.1)'  // More transparent blue for light mode

    const borderColor = computedColorScheme === 'dark'
        ? theme.colors.blue[4]
        : theme.colors.blue[6]

    return (
        <Paper
            p="md"
            withBorder
            bg={notification.isRead ? undefined : unreadBgColor}
            style={{
                borderLeft: notification.isRead ? undefined : `${rem(4)} solid ${borderColor}`,
            }}
        >
            <Group justify="space-between" align="flex-start">
                <UnstyledButton onClick={handleClick} style={{ flex: 1 }}>
                    <Group align="flex-start" gap="sm">
                        <Avatar
                            color={getNotificationColor(notification.type)}
                            radius="xl"
                            size="md"
                        >
                            {getNotificationIcon(notification.type)}
                        </Avatar>

                        <div style={{ flex: 1 }}>
                            <Text
                                fw={notification.isRead ? 400 : 600}
                                size="sm"
                                c={notification.isRead ? undefined : 'white'}
                            >
                                {notification.title}
                            </Text>
                            <Text
                                c={notification.isRead ? "dimmed" : "rgba(255, 255, 255, 0.8)"}
                                size="xs"
                                mt={2}
                            >
                                {notification.message}
                            </Text>
                            <Text
                                c={notification.isRead ? "dimmed" : "rgba(255, 255, 255, 0.7)"}
                                size="xs"
                                mt={4}
                            >
                                {timeAgo}
                            </Text>
                        </div>
                    </Group>
                </UnstyledButton>

                <ActionIcon
                    variant="subtle"
                    color="gray"
                    size="sm"
                    onClick={(e) => {
                        e.stopPropagation()
                        onDelete(notification.id)
                    }}
                >
                    <IconX size={14} />
                </ActionIcon>
            </Group>
        </Paper>
    )
}