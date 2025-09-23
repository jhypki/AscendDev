import { Card, Text, Group, Badge, Avatar, ActionIcon, Menu, Stack } from '@mantine/core';
import { IconDots, IconPin, IconLock, IconEye, IconMessage, IconEdit, IconTrash } from '@tabler/icons-react';
import type { Discussion } from '../../types/discussion';

// Simple time ago utility
function timeAgo(date: string | Date): string {
    const now = new Date();
    const past = new Date(date);
    const diffInSeconds = Math.floor((now.getTime() - past.getTime()) / 1000);

    if (diffInSeconds < 60) return 'just now';
    if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)}m ago`;
    if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)}h ago`;
    if (diffInSeconds < 2592000) return `${Math.floor(diffInSeconds / 86400)}d ago`;
    return `${Math.floor(diffInSeconds / 2592000)}mo ago`;
}

interface DiscussionCardProps {
    discussion: Discussion;
    onEdit?: (discussion: Discussion) => void;
    onDelete?: (discussion: Discussion) => void;
    onPin?: (discussion: Discussion) => void;
    onLock?: (discussion: Discussion) => void;
    canModerate?: boolean;
    isOwner?: boolean;
}

export function DiscussionCard({
    discussion,
    onEdit,
    onDelete,
    onPin,
    onLock,
    canModerate = false,
    isOwner = false,
}: DiscussionCardProps) {
    const createdTimeAgo = timeAgo(discussion.createdAt);
    const lastActivityAgo = timeAgo(discussion.lastActivity);

    return (
        <Card shadow="sm" padding="lg" radius="md" withBorder>
            <Stack gap="md">
                {/* Header */}
                <Group justify="space-between" align="flex-start">
                    <Group gap="sm" align="flex-start" style={{ flex: 1 }}>
                        <Avatar
                            src={discussion.user.profilePictureUrl}
                            alt={discussion.user.username}
                            size="sm"
                            radius="xl"
                        >
                            {discussion.user.username.charAt(0).toUpperCase()}
                        </Avatar>

                        <Stack gap={4} style={{ flex: 1 }}>
                            <Group gap="xs" align="center">
                                <Text size="sm" fw={500} c="dimmed">
                                    {discussion.user.username}
                                </Text>
                                <Text size="xs" c="dimmed">
                                    •
                                </Text>
                                <Text size="xs" c="dimmed">
                                    {createdTimeAgo}
                                </Text>
                                {discussion.isPinned && (
                                    <Badge size="xs" color="yellow" leftSection={<IconPin size={10} />}>
                                        Pinned
                                    </Badge>
                                )}
                                {discussion.isLocked && (
                                    <Badge size="xs" color="red" leftSection={<IconLock size={10} />}>
                                        Locked
                                    </Badge>
                                )}
                            </Group>

                            <Text fw={600} size="lg" lineClamp={2}>
                                {discussion.title}
                            </Text>
                        </Stack>
                    </Group>

                    {/* Actions Menu */}
                    {(isOwner || canModerate) && (
                        <Menu shadow="md" width={200}>
                            <Menu.Target>
                                <ActionIcon variant="subtle" color="gray">
                                    <IconDots size={16} />
                                </ActionIcon>
                            </Menu.Target>

                            <Menu.Dropdown>
                                {isOwner && onEdit && (
                                    <Menu.Item leftSection={<IconEdit size={14} />} onClick={() => onEdit(discussion)}>
                                        Edit
                                    </Menu.Item>
                                )}

                                {canModerate && onPin && (
                                    <Menu.Item
                                        leftSection={<IconPin size={14} />}
                                        onClick={() => onPin(discussion)}
                                    >
                                        {discussion.isPinned ? 'Unpin' : 'Pin'}
                                    </Menu.Item>
                                )}

                                {canModerate && onLock && (
                                    <Menu.Item
                                        leftSection={<IconLock size={14} />}
                                        onClick={() => onLock(discussion)}
                                    >
                                        {discussion.isLocked ? 'Unlock' : 'Lock'}
                                    </Menu.Item>
                                )}

                                {(isOwner || canModerate) && onDelete && (
                                    <>
                                        <Menu.Divider />
                                        <Menu.Item
                                            color="red"
                                            leftSection={<IconTrash size={14} />}
                                            onClick={() => onDelete(discussion)}
                                        >
                                            Delete
                                        </Menu.Item>
                                    </>
                                )}
                            </Menu.Dropdown>
                        </Menu>
                    )}
                </Group>

                {/* Content Preview */}
                <Text size="sm" c="dimmed" lineClamp={3}>
                    {discussion.content}
                </Text>

                {/* Footer Stats */}
                <Group justify="space-between" align="center">
                    <Group gap="lg">
                        <Group gap={4}>
                            <IconEye size={16} color="var(--mantine-color-dimmed)" />
                            <Text size="xs" c="dimmed">
                                {discussion.viewCount}
                            </Text>
                        </Group>

                        <Group gap={4}>
                            <IconMessage size={16} color="var(--mantine-color-dimmed)" />
                            <Text size="xs" c="dimmed">
                                {discussion.replyCount}
                            </Text>
                        </Group>
                    </Group>

                    {discussion.replyCount > 0 && (
                        <Text size="xs" c="dimmed">
                            Last activity {lastActivityAgo}
                        </Text>
                    )}
                </Group>
            </Stack>
        </Card>
    );
}