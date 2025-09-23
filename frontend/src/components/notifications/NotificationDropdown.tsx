import { ActionIcon, Button, Divider, Group, Indicator, Menu, ScrollArea, Stack, Text } from '@mantine/core'
import { IconBell, IconCheck } from '@tabler/icons-react'
import { useNavigate } from 'react-router-dom'
import { useNotifications, useUnreadCount, useMarkAsRead, useMarkAllAsRead, useDeleteNotification } from '../../hooks/api/useNotifications'
import { NotificationItem } from './NotificationItem'
import type { Notification } from '../../types/notification'

export function NotificationDropdown() {
    const navigate = useNavigate()
    const { data: notifications = [], isLoading } = useNotifications(1, 10) // Show recent 10 notifications
    const { data: unreadCount = 0 } = useUnreadCount()
    const markAsReadMutation = useMarkAsRead()
    const markAllAsReadMutation = useMarkAllAsRead()
    const deleteNotificationMutation = useDeleteNotification()

    const handleMarkAsRead = (id: string) => {
        markAsReadMutation.mutate(id)
    }

    const handleMarkAllAsRead = () => {
        markAllAsReadMutation.mutate()
    }

    const handleDelete = (id: string) => {
        deleteNotificationMutation.mutate(id)
    }

    const handleNotificationClick = (notification: Notification) => {
        if (notification.actionUrl) {
            navigate(notification.actionUrl)
        }
    }

    return (
        <Menu width={400} position="bottom-end" withArrow>
            <Menu.Target>
                <ActionIcon variant="subtle" size="lg">
                    <Indicator
                        inline
                        label={unreadCount > 0 ? unreadCount : undefined}
                        size={16}
                        disabled={unreadCount === 0}
                    >
                        <IconBell size={20} />
                    </Indicator>
                </ActionIcon>
            </Menu.Target>

            <Menu.Dropdown>
                <Group justify="space-between" p="sm">
                    <Text fw={600} size="sm">
                        Notifications
                    </Text>
                    {unreadCount > 0 && (
                        <Button
                            variant="subtle"
                            size="xs"
                            leftSection={<IconCheck size={14} />}
                            onClick={handleMarkAllAsRead}
                            loading={markAllAsReadMutation.isPending}
                        >
                            Mark all read
                        </Button>
                    )}
                </Group>

                <Divider />

                <ScrollArea.Autosize mah={400} p="xs">
                    {isLoading ? (
                        <Text c="dimmed" ta="center" p="md">
                            Loading notifications...
                        </Text>
                    ) : notifications.length === 0 ? (
                        <Text c="dimmed" ta="center" p="md">
                            No notifications
                        </Text>
                    ) : (
                        <Stack gap="xs">
                            {notifications.map((notification) => (
                                <NotificationItem
                                    key={notification.id}
                                    notification={notification}
                                    onMarkAsRead={handleMarkAsRead}
                                    onDelete={handleDelete}
                                    onClick={handleNotificationClick}
                                />
                            ))}
                        </Stack>
                    )}
                </ScrollArea.Autosize>

                {notifications.length > 0 && (
                    <>
                        <Divider />
                        <Group justify="center" p="sm">
                            <Button
                                variant="subtle"
                                size="sm"
                                onClick={() => navigate('/notifications')}
                            >
                                View all notifications
                            </Button>
                        </Group>
                    </>
                )}
            </Menu.Dropdown>
        </Menu>
    )
}