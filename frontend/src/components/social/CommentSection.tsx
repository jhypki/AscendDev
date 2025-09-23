import { useState } from 'react'
import {
    Stack,
    Textarea,
    Button,
    Group,
    Text,
    Avatar,
    Paper,
    ActionIcon,
    Menu,
    Divider,
    Badge
} from '@mantine/core'
import {
    IconSend,
    IconDots,
    IconEdit,
    IconTrash,
    IconPin,
    IconLock,
    IconHeart
} from '@tabler/icons-react'
import {
    useCreateDiscussion,
    useDiscussions,
    useUpdateDiscussion,
    useDeleteDiscussion,
    useLikeDiscussion,
    useUnlikeDiscussion,
    useCreateDiscussionReply
} from '../../hooks/api/useDiscussions'
import { useAppSelector } from '../../store/hooks'
import { notifications } from '@mantine/notifications'
import { modals } from '@mantine/modals'
import type { Discussion } from '../../types/discussion'
// Simple time ago utility function
const timeAgo = (dateString: string): string => {
    const now = new Date()
    const date = new Date(dateString)
    const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000)

    if (diffInSeconds < 60) return 'just now'
    if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)}m ago`
    if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)}h ago`
    if (diffInSeconds < 2592000) return `${Math.floor(diffInSeconds / 86400)}d ago`
    return date.toLocaleDateString()
}

interface CommentSectionProps {
    lessonId?: string
    courseId?: string
    canModerate?: boolean
    onPinDiscussion?: (discussion: Discussion) => void
    onLockDiscussion?: (discussion: Discussion) => void
}

export const CommentSection = ({
    lessonId,
    courseId,
    canModerate = false,
    onPinDiscussion,
    onLockDiscussion
}: CommentSectionProps) => {
    const [newComment, setNewComment] = useState('')
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [editingDiscussion, setEditingDiscussion] = useState<Discussion | null>(null)
    const [editContent, setEditContent] = useState('')
    const [replyingTo, setReplyingTo] = useState<Discussion | null>(null)
    const [replyContent, setReplyContent] = useState('')
    const { user } = useAppSelector((state) => state.auth)

    const { data: discussions, isLoading } = useDiscussions(lessonId, courseId)
    const createDiscussionMutation = useCreateDiscussion()
    const updateDiscussionMutation = useUpdateDiscussion()
    const deleteDiscussionMutation = useDeleteDiscussion()
    const likeDiscussionMutation = useLikeDiscussion()
    const unlikeDiscussionMutation = useUnlikeDiscussion()
    const createReplyMutation = useCreateDiscussionReply()

    const handleSubmitComment = async () => {
        if (!newComment.trim()) return

        setIsSubmitting(true)
        try {
            await createDiscussionMutation.mutateAsync({
                title: newComment.slice(0, 50) + (newComment.length > 50 ? '...' : ''),
                content: newComment,
                lessonId,
                courseId
            })

            setNewComment('')
            notifications.show({
                title: 'Comment Posted',
                message: 'Your comment has been posted successfully',
                color: 'green'
            })
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to post comment',
                color: 'red'
            })
        } finally {
            setIsSubmitting(false)
        }
    }

    const handleKeyPress = (event: React.KeyboardEvent) => {
        if (event.key === 'Enter' && (event.ctrlKey || event.metaKey)) {
            handleSubmitComment()
        }
    }

    const handleEditDiscussion = (discussion: Discussion) => {
        setEditingDiscussion(discussion)
        setEditContent(discussion.content)
    }

    const handleSaveEdit = async () => {
        if (!editingDiscussion || !editContent.trim()) return

        try {
            await updateDiscussionMutation.mutateAsync({
                id: editingDiscussion.id,
                data: {
                    title: editContent.slice(0, 50) + (editContent.length > 50 ? '...' : ''),
                    content: editContent
                },
                lessonId,
                courseId
            })

            setEditingDiscussion(null)
            setEditContent('')
            notifications.show({
                title: 'Comment Updated',
                message: 'Your comment has been updated successfully',
                color: 'green'
            })
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to update comment',
                color: 'red'
            })
        }
    }

    const handleCancelEdit = () => {
        setEditingDiscussion(null)
        setEditContent('')
    }

    const handleDeleteDiscussion = (discussion: Discussion) => {
        modals.openConfirmModal({
            title: 'Delete Comment',
            children: 'Are you sure you want to delete this comment? This action cannot be undone.',
            labels: { confirm: 'Delete', cancel: 'Cancel' },
            confirmProps: { color: 'red' },
            onConfirm: async () => {
                try {
                    await deleteDiscussionMutation.mutateAsync({
                        id: discussion.id,
                        lessonId,
                        courseId
                    })
                    notifications.show({
                        title: 'Comment Deleted',
                        message: 'Comment has been deleted successfully',
                        color: 'green'
                    })
                } catch {
                    notifications.show({
                        title: 'Error',
                        message: 'Failed to delete comment',
                        color: 'red'
                    })
                }
            }
        })
    }

    const handleLikeDiscussion = async (discussion: Discussion) => {
        try {
            if (discussion.isLikedByCurrentUser) {
                await unlikeDiscussionMutation.mutateAsync(discussion.id)
                notifications.show({
                    title: 'Unliked',
                    message: 'Comment unliked',
                    color: 'blue'
                })
            } else {
                await likeDiscussionMutation.mutateAsync(discussion.id)
                notifications.show({
                    title: 'Liked',
                    message: 'Comment liked',
                    color: 'blue'
                })
            }
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to update like status',
                color: 'red'
            })
        }
    }

    const handleReplyToDiscussion = (discussion: Discussion) => {
        setReplyingTo(discussion)
        setReplyContent('')
    }

    const handleSubmitReply = async () => {
        if (!replyingTo || !replyContent.trim()) return

        try {
            await createReplyMutation.mutateAsync({
                discussionId: replyingTo.id,
                data: {
                    content: replyContent
                }
            })

            notifications.show({
                title: 'Reply Posted',
                message: 'Your reply has been posted successfully',
                color: 'green'
            })

            setReplyingTo(null)
            setReplyContent('')
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to post reply',
                color: 'red'
            })
        }
    }

    const handleCancelReply = () => {
        setReplyingTo(null)
        setReplyContent('')
    }

    const getInitials = (username: string) => {
        return username?.charAt(0)?.toUpperCase() || '?'
    }

    return (
        <Stack gap="md">
            {/* Comment Input */}
            <Group align="flex-start" gap="sm">
                <Avatar
                    src={user?.profilePictureUrl}
                    alt={user?.username || 'User'}
                    size="sm"
                    radius="xl"
                >
                    {user?.username?.charAt(0).toUpperCase()}
                </Avatar>
                <Stack gap="xs" style={{ flex: 1 }}>
                    <Textarea
                        placeholder="Add a comment..."
                        value={newComment}
                        onChange={(e) => setNewComment(e.target.value)}
                        onKeyDown={handleKeyPress}
                        minRows={1}
                        maxRows={4}
                        autosize
                        styles={{
                            input: {
                                border: '1px solid var(--mantine-color-default-border)',
                                borderRadius: '12px',
                                padding: '12px 16px',
                                backgroundColor: 'var(--mantine-color-body)',
                                '&:focus': {
                                    borderColor: 'var(--mantine-color-blue-6)',
                                    backgroundColor: 'var(--mantine-color-body)'
                                },
                                '&:hover': {
                                    borderColor: 'var(--mantine-color-gray-4)',
                                    backgroundColor: 'var(--mantine-color-body)'
                                }
                            }
                        }}
                    />
                    {newComment.trim() && (
                        <Group justify="flex-end" gap="xs">
                            <Button
                                variant="subtle"
                                size="xs"
                                onClick={() => setNewComment('')}
                            >
                                Cancel
                            </Button>
                            <Button
                                size="xs"
                                leftSection={<IconSend size={14} />}
                                onClick={handleSubmitComment}
                                loading={isSubmitting}
                                disabled={!newComment.trim()}
                            >
                                Comment
                            </Button>
                        </Group>
                    )}
                </Stack>
            </Group>

            <Divider />

            {/* Comments List */}
            <Stack gap="md">
                {isLoading ? (
                    <Text c="dimmed" ta="center">Loading comments...</Text>
                ) : discussions && discussions.length > 0 ? (
                    discussions.map((discussion) => (
                        <Paper key={discussion.id} p="sm" withBorder={false}>
                            <Group align="flex-start" gap="sm">
                                <Avatar
                                    src={discussion.user?.profilePictureUrl || null}
                                    alt={discussion.user?.username || 'User'}
                                    size="sm"
                                    radius="xl"
                                >
                                    {getInitials(discussion.user?.username || 'User')}
                                </Avatar>
                                <Stack gap="xs" style={{ flex: 1 }}>
                                    <Group gap="xs" align="center">
                                        <Text size="sm" fw={500}>
                                            {discussion.user?.username || 'Unknown User'}
                                        </Text>
                                        <Text size="xs" c="dimmed">
                                            {timeAgo(discussion.createdAt)}
                                        </Text>
                                        {discussion.isPinned && (
                                            <Badge size="xs" color="yellow" variant="light">
                                                Pinned
                                            </Badge>
                                        )}
                                        {discussion.isLocked && (
                                            <Badge size="xs" color="red" variant="light">
                                                Locked
                                            </Badge>
                                        )}
                                    </Group>

                                    {editingDiscussion?.id === discussion.id ? (
                                        <Stack gap="xs">
                                            <Textarea
                                                value={editContent}
                                                onChange={(e) => setEditContent(e.target.value)}
                                                minRows={2}
                                                maxRows={6}
                                                autosize
                                            />
                                            <Group gap="xs" justify="flex-end">
                                                <Button size="xs" variant="subtle" onClick={handleCancelEdit}>
                                                    Cancel
                                                </Button>
                                                <Button
                                                    size="xs"
                                                    onClick={handleSaveEdit}
                                                    loading={updateDiscussionMutation.isPending}
                                                    disabled={!editContent.trim()}
                                                >
                                                    Save
                                                </Button>
                                            </Group>
                                        </Stack>
                                    ) : (
                                        <Text size="sm" style={{ whiteSpace: 'pre-wrap' }}>
                                            {discussion.content}
                                        </Text>
                                    )}

                                    <Group gap="sm" align="center">
                                        <Group gap="xs" align="center">
                                            <ActionIcon
                                                variant="subtle"
                                                size="sm"
                                                color={discussion.isLikedByCurrentUser ? 'red' : 'gray'}
                                                onClick={() => handleLikeDiscussion(discussion)}
                                            >
                                                <IconHeart
                                                    size={14}
                                                    fill={discussion.isLikedByCurrentUser ? 'currentColor' : 'none'}
                                                />
                                            </ActionIcon>
                                            <Text size="xs" c="dimmed">
                                                {discussion.likeCount || 0}
                                            </Text>
                                        </Group>

                                        <Button
                                            variant="subtle"
                                            size="xs"
                                            onClick={() => handleReplyToDiscussion(discussion)}
                                        >
                                            Reply
                                        </Button>

                                        <Text size="xs" c="dimmed">
                                            {discussion.replyCount || 0} replies
                                        </Text>
                                        {(canModerate || discussion.user?.id === user?.id) && !editingDiscussion && (
                                            <Menu shadow="md" width={200}>
                                                <Menu.Target>
                                                    <ActionIcon variant="subtle" size="sm">
                                                        <IconDots size={14} />
                                                    </ActionIcon>
                                                </Menu.Target>
                                                <Menu.Dropdown>
                                                    {discussion.user?.id === user?.id && (
                                                        <Menu.Item
                                                            leftSection={<IconEdit size={14} />}
                                                            onClick={() => handleEditDiscussion(discussion)}
                                                        >
                                                            Edit
                                                        </Menu.Item>
                                                    )}
                                                    {canModerate && (
                                                        <>
                                                            <Menu.Item
                                                                leftSection={<IconPin size={14} />}
                                                                onClick={() => onPinDiscussion?.(discussion)}
                                                            >
                                                                {discussion.isPinned ? 'Unpin' : 'Pin'}
                                                            </Menu.Item>
                                                            <Menu.Item
                                                                leftSection={<IconLock size={14} />}
                                                                onClick={() => onLockDiscussion?.(discussion)}
                                                            >
                                                                {discussion.isLocked ? 'Unlock' : 'Lock'}
                                                            </Menu.Item>
                                                        </>
                                                    )}
                                                    {(canModerate || discussion.user?.id === user?.id) && (
                                                        <>
                                                            <Menu.Divider />
                                                            <Menu.Item
                                                                color="red"
                                                                leftSection={<IconTrash size={14} />}
                                                                onClick={() => handleDeleteDiscussion(discussion)}
                                                            >
                                                                Delete
                                                            </Menu.Item>
                                                        </>
                                                    )}
                                                </Menu.Dropdown>
                                            </Menu>
                                        )}
                                    </Group>
                                </Stack>
                            </Group>

                            {/* Replies */}
                            {discussion.replies && discussion.replies.length > 0 && (
                                <Stack gap="sm" mt="sm" pl="xl">
                                    {discussion.replies.map((reply) => (
                                        <Group key={reply.id} align="flex-start" gap="sm">
                                            <Avatar
                                                src={reply.user?.profilePictureUrl || null}
                                                alt={reply.user?.username || 'User'}
                                                size="xs"
                                                radius="xl"
                                            >
                                                {getInitials(reply.user?.username || 'User')}
                                            </Avatar>
                                            <Stack gap="xs" style={{ flex: 1 }}>
                                                <Group gap="xs" align="center">
                                                    <Text size="xs" fw={500}>
                                                        {reply.user?.username || 'Unknown User'}
                                                    </Text>
                                                    <Text size="xs" c="dimmed">
                                                        {timeAgo(reply.createdAt)}
                                                    </Text>
                                                </Group>
                                                <Text size="xs" style={{ whiteSpace: 'pre-wrap' }}>
                                                    {reply.content}
                                                </Text>
                                            </Stack>
                                        </Group>
                                    ))}
                                </Stack>
                            )}

                            {/* Reply Input */}
                            {replyingTo?.id === discussion.id && (
                                <Stack gap="xs" mt="sm" pl="xl">
                                    <Group align="flex-start" gap="sm">
                                        <Avatar
                                            src={user?.profilePictureUrl}
                                            alt={user?.username || 'User'}
                                            size="xs"
                                            radius="xl"
                                        >
                                            {getInitials(user?.username || 'User')}
                                        </Avatar>
                                        <Stack gap="xs" style={{ flex: 1 }}>
                                            <Textarea
                                                placeholder={`Reply to ${discussion.user?.username}...`}
                                                value={replyContent}
                                                onChange={(e) => setReplyContent(e.target.value)}
                                                minRows={2}
                                                maxRows={4}
                                                autosize
                                                size="sm"
                                                styles={{
                                                    input: {
                                                        border: '1px solid var(--mantine-color-default-border)',
                                                        borderRadius: '8px',
                                                        padding: '8px 12px',
                                                        backgroundColor: 'var(--mantine-color-body)',
                                                        '&:focus': {
                                                            borderColor: 'var(--mantine-color-blue-6)',
                                                            backgroundColor: 'var(--mantine-color-body)'
                                                        },
                                                        '&:hover': {
                                                            borderColor: 'var(--mantine-color-gray-4)',
                                                            backgroundColor: 'var(--mantine-color-body)'
                                                        }
                                                    }
                                                }}
                                            />
                                            <Group gap="xs" justify="flex-end">
                                                <Button size="xs" variant="subtle" onClick={handleCancelReply}>
                                                    Cancel
                                                </Button>
                                                <Button
                                                    size="xs"
                                                    onClick={handleSubmitReply}
                                                    disabled={!replyContent.trim()}
                                                >
                                                    Reply
                                                </Button>
                                            </Group>
                                        </Stack>
                                    </Group>
                                </Stack>
                            )}
                        </Paper>
                    ))
                ) : (
                    <Text c="dimmed" ta="center" py="xl">
                        No comments yet. Be the first to comment!
                    </Text>
                )}
            </Stack>
        </Stack>
    )
}