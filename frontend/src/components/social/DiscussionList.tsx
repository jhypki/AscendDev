import { Stack, Text, Button, Group, Loader, Center, Alert } from '@mantine/core';
import { IconPlus, IconAlertCircle } from '@tabler/icons-react';
import { DiscussionCard } from './DiscussionCard';
import { useDiscussions } from '../../hooks/api/useDiscussions';
import type { Discussion } from '../../types/discussion';

interface DiscussionListProps {
    lessonId?: string;
    courseId?: string;
    onCreateDiscussion?: () => void;
    onEditDiscussion?: (discussion: Discussion) => void;
    onDeleteDiscussion?: (discussion: Discussion) => void;
    onPinDiscussion?: (discussion: Discussion) => void;
    onLockDiscussion?: (discussion: Discussion) => void;
    canModerate?: boolean;
    currentUserId?: string;
    showCreateButton?: boolean;
    title?: string;
}

export function DiscussionList({
    lessonId,
    courseId,
    onCreateDiscussion,
    onEditDiscussion,
    onDeleteDiscussion,
    onPinDiscussion,
    onLockDiscussion,
    canModerate = false,
    currentUserId,
    showCreateButton = true,
    title = 'Discussions',
}: DiscussionListProps) {
    const { data: discussions, isLoading, error } = useDiscussions(lessonId, courseId);

    if (isLoading) {
        return (
            <Center py="xl">
                <Loader size="md" />
            </Center>
        );
    }

    if (error) {
        return (
            <Alert icon={<IconAlertCircle size={16} />} color="red" title="Error">
                Failed to load discussions. Please try again later.
            </Alert>
        );
    }

    const sortedDiscussions = discussions ? [...discussions].sort((a, b) => {
        // Sort by pinned first, then by creation date (newest first)
        if (a.isPinned && !b.isPinned) return -1;
        if (!a.isPinned && b.isPinned) return 1;
        return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
    }) : [];

    return (
        <Stack gap="md">
            {/* Header */}
            <Group justify="space-between" align="center">
                <Text size="xl" fw={600}>
                    {title} ({sortedDiscussions.length})
                </Text>

                {showCreateButton && onCreateDiscussion && (
                    <Button
                        leftSection={<IconPlus size={16} />}
                        onClick={onCreateDiscussion}
                        size="sm"
                    >
                        New Discussion
                    </Button>
                )}
            </Group>

            {/* Discussion List */}
            {sortedDiscussions.length === 0 ? (
                <Center py="xl">
                    <Stack align="center" gap="md">
                        <Text size="lg" c="dimmed">
                            No discussions yet
                        </Text>
                        <Text size="sm" c="dimmed" ta="center">
                            {showCreateButton && onCreateDiscussion
                                ? 'Be the first to start a discussion!'
                                : 'Check back later for discussions.'}
                        </Text>
                        {showCreateButton && onCreateDiscussion && (
                            <Button
                                leftSection={<IconPlus size={16} />}
                                onClick={onCreateDiscussion}
                                variant="light"
                            >
                                Start Discussion
                            </Button>
                        )}
                    </Stack>
                </Center>
            ) : (
                <Stack gap="md">
                    {sortedDiscussions.map((discussion) => (
                        <DiscussionCard
                            key={discussion.id}
                            discussion={discussion}
                            onEdit={onEditDiscussion}
                            onDelete={onDeleteDiscussion}
                            onPin={onPinDiscussion}
                            onLock={onLockDiscussion}
                            canModerate={canModerate}
                            isOwner={currentUserId === discussion.userId}
                        />
                    ))}
                </Stack>
            )}
        </Stack>
    );
}