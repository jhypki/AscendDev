import { useState, useEffect, useRef } from 'react'
import {
    Container,
    Grid,
    Stack,
    Text,
    Paper,
    Group,
    Button,
    Badge,
    Divider,
    Alert,
    LoadingOverlay,
    Textarea,
    ActionIcon,
    Tooltip,
    ScrollArea,
    useComputedColorScheme,
    Collapse
} from '@mantine/core'
import {
    IconArrowLeft,
    IconX,
    IconMessageCircle,
    IconInfoCircle,
    IconUser,
    IconClock,
    IconCode,
    IconCornerDownRight,
    IconTrash
} from '@tabler/icons-react'
import { useParams, useNavigate } from 'react-router-dom'
import { notifications } from '@mantine/notifications'
import { useSelector } from 'react-redux'
import { useCodeReview, useCreateCodeReviewComment, useCreateCodeReview, useMyCodeReviewForSubmission, useAllCommentsForSubmission, useDeleteCodeReviewComment } from '../../hooks/api/useCodeReviews'
import { useSubmissionForReview } from '../../hooks/api/useSubmissions'
import { CodeReviewStatus } from '../../types/submission'
import type { CodeReviewComment } from '../../types/submission'
import type { RootState } from '../../store'

export const CodeReviewPage = () => {
    const { submissionId } = useParams<{ submissionId: string }>()
    const navigate = useNavigate()
    const [newComment, setNewComment] = useState('')
    const [selectedLine, setSelectedLine] = useState<number | null>(null)
    const [replyingTo, setReplyingTo] = useState<string | null>(null)
    const [replyText, setReplyText] = useState('')
    const colorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true })
    const currentUser = useSelector((state: RootState) => state.auth.user)
    const hasTriedToCreateReview = useRef(false)

    // API hooks
    const { data: submission, isLoading: submissionLoading } = useSubmissionForReview(Number(submissionId))
    const { data: existingCodeReview, isLoading: existingReviewLoading } = useMyCodeReviewForSubmission(Number(submissionId))
    const { data: codeReview, isLoading: reviewLoading } = useCodeReview(existingCodeReview?.id || '')

    // Get all comments for this submission from all code reviews
    const { data: allCommentsFromSubmission, isLoading: allCommentsLoading } = useAllCommentsForSubmission(Number(submissionId))

    // Use all comments from the submission
    const allComments = allCommentsFromSubmission || []
    const commentsLoading = allCommentsLoading

    const createCommentMutation = useCreateCodeReviewComment()
    const createCodeReviewMutation = useCreateCodeReview()
    const deleteCommentMutation = useDeleteCodeReviewComment()

    // Create code review when submission loads and no existing review found
    const handleCreateCodeReview = async () => {
        if (!submission || existingCodeReview || createCodeReviewMutation.isPending || hasTriedToCreateReview.current) return

        hasTriedToCreateReview.current = true

        try {
            await createCodeReviewMutation.mutateAsync({
                lessonId: submission.lessonId,
                revieweeId: submission.userId,
                submissionId: submission.id
            })
        } catch (error: unknown) {
            // If the error is about reviewing own code, don't show error notification
            const apiError = error as { response?: { data?: { message?: string } } }
            const errorMessage = apiError?.response?.data?.message
            if (typeof errorMessage === 'string' && errorMessage.includes('cannot review your own code')) {
                // This is expected for users viewing their own submissions
                return
            }
            notifications.show({
                title: 'Error',
                message: 'Failed to create code review. Please try again.',
                color: 'red'
            })
        }
    }

    // Auto-create code review when submission loads and no existing review found
    // But only if the submission is not from the current user (can't review your own code)
    useEffect(() => {
        const isOwnSubmission = submission && currentUser && submission.userId === currentUser.id
        if (submission && !existingReviewLoading && !existingCodeReview && !createCodeReviewMutation.isPending && !isOwnSubmission && !hasTriedToCreateReview.current) {
            handleCreateCodeReview()
        }
    }, [submission, existingReviewLoading, existingCodeReview, createCodeReviewMutation.isPending, currentUser])

    // Use existing code review or the one from the current query
    const currentCodeReview = codeReview || existingCodeReview

    const handleAddComment = async () => {
        if (!newComment.trim()) return

        // If we don't have a current code review, we need to create one first
        if (!currentCodeReview && submission && currentUser && submission.userId !== currentUser.id) {
            try {
                const newReview = await createCodeReviewMutation.mutateAsync({
                    lessonId: submission.lessonId,
                    revieweeId: submission.userId,
                    submissionId: submission.id
                })

                // Now add the comment to the newly created review
                await createCommentMutation.mutateAsync({
                    codeReviewId: newReview.id,
                    request: {
                        content: newComment,
                        lineNumber: selectedLine || undefined
                    }
                })
            } catch (error: unknown) {
                const apiError = error as { response?: { data?: { message?: string } } }
                const errorMessage = apiError?.response?.data?.message
                if (typeof errorMessage === 'string' && errorMessage.includes('cannot review your own code')) {
                    notifications.show({
                        title: 'Error',
                        message: 'You cannot add comments to your own submission.',
                        color: 'red'
                    })
                    return
                }
                notifications.show({
                    title: 'Error',
                    message: 'Failed to add comment. Please try again.',
                    color: 'red'
                })
                return
            }
        } else if (currentCodeReview) {
            try {
                await createCommentMutation.mutateAsync({
                    codeReviewId: currentCodeReview.id,
                    request: {
                        content: newComment,
                        lineNumber: selectedLine || undefined
                    }
                })
            } catch {
                notifications.show({
                    title: 'Error',
                    message: 'Failed to add comment. Please try again.',
                    color: 'red'
                })
                return
            }
        } else {
            notifications.show({
                title: 'Error',
                message: 'You cannot add comments to your own submission.',
                color: 'red'
            })
            return
        }

        setNewComment('')
        setSelectedLine(null)

        notifications.show({
            title: 'Comment Added',
            message: 'Your comment has been added successfully.',
            color: 'green'
        })
    }

    const handleAddReply = async (parentCommentId: string) => {
        if (!replyText.trim()) return

        // Find the parent comment to determine which code review it belongs to
        const parentComment = allComments.find(c => c.id === parentCommentId)
        if (!parentComment) {
            notifications.show({
                title: 'Error',
                message: 'Parent comment not found.',
                color: 'red'
            })
            return
        }

        try {
            await createCommentMutation.mutateAsync({
                codeReviewId: parentComment.codeReviewId,
                request: {
                    content: replyText,
                    parentCommentId
                }
            })

            setReplyText('')
            setReplyingTo(null)

            notifications.show({
                title: 'Reply Added',
                message: 'Your reply has been added successfully.',
                color: 'green'
            })
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to add reply. Please try again.',
                color: 'red'
            })
        }
    }

    const handleDeleteComment = async (commentId: string, codeReviewId: string) => {
        if (!confirm('Are you sure you want to delete this comment?')) return

        try {
            await deleteCommentMutation.mutateAsync({
                codeReviewId,
                commentId
            })

            notifications.show({
                title: 'Comment Deleted',
                message: 'Your comment has been deleted successfully.',
                color: 'green'
            })
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to delete comment. Please try again.',
                color: 'red'
            })
        }
    }

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        })
    }

    const getStatusColor = (status: CodeReviewStatus) => {
        switch (status) {
            case CodeReviewStatus.Pending:
                return 'yellow'
            case CodeReviewStatus.InReview:
                return 'blue'
            case CodeReviewStatus.ChangesRequested:
                return 'orange'
            case CodeReviewStatus.Approved:
                return 'green'
            case CodeReviewStatus.Completed:
                return 'gray'
            default:
                return 'gray'
        }
    }

    const renderComment = (comment: CodeReviewComment, isReply = false) => (
        <Paper
            key={comment.id}
            withBorder={!isReply}
            p="sm"
            ml={isReply ? "md" : 0}
            style={{
                borderLeft: isReply ? '2px solid var(--mantine-color-blue-4)' : undefined,
                backgroundColor: isReply ? (colorScheme === 'dark' ? 'var(--mantine-color-dark-6)' : 'var(--mantine-color-gray-0)') : undefined
            }}
        >
            <Stack gap="xs">
                <Group justify="space-between" align="flex-start">
                    <Group gap="xs">
                        {isReply && <IconCornerDownRight size={14} />}
                        <Text size="sm" fw={500}>
                            {comment.user.username}
                        </Text>
                        {comment.lineNumber && !isReply && (
                            <Badge size="xs" variant="light">
                                Line {comment.lineNumber}
                            </Badge>
                        )}
                        {comment.replyCount > 0 && !isReply && (
                            <Badge size="xs" variant="outline">
                                {comment.replyCount} {comment.replyCount === 1 ? 'reply' : 'replies'}
                            </Badge>
                        )}
                    </Group>
                    <Group gap="xs">
                        <Text size="xs" c="dimmed">
                            {formatDate(comment.createdAt)}
                        </Text>
                        {/* Show delete button only for replies and only if current user is the author */}
                        {isReply && currentUser && comment.user.id === currentUser.id && (
                            <Tooltip label="Delete reply">
                                <ActionIcon
                                    size="xs"
                                    variant="subtle"
                                    color="red"
                                    onClick={() => handleDeleteComment(comment.id, comment.codeReviewId)}
                                    loading={deleteCommentMutation.isPending}
                                >
                                    <IconTrash size={10} />
                                </ActionIcon>
                            </Tooltip>
                        )}
                    </Group>
                </Group>
                <Text size="sm">{comment.content}</Text>

                {!isReply && (
                    <Group gap="xs">
                        <Button
                            size="xs"
                            variant="subtle"
                            leftSection={<IconMessageCircle size={12} />}
                            onClick={() => setReplyingTo(comment.id)}
                        >
                            Reply
                        </Button>
                    </Group>
                )}

                {/* Reply Form */}
                <Collapse in={replyingTo === comment.id}>
                    <Stack gap="xs" mt="xs">
                        <Textarea
                            placeholder="Write a reply..."
                            value={replyText}
                            onChange={(e) => setReplyText(e.target.value)}
                            minRows={2}
                            maxRows={4}
                            size="sm"
                        />
                        <Group gap="xs">
                            <Button
                                size="xs"
                                onClick={() => handleAddReply(comment.id)}
                                loading={createCommentMutation.isPending}
                                disabled={!replyText.trim()}
                            >
                                Reply
                            </Button>
                            <Button
                                size="xs"
                                variant="subtle"
                                onClick={() => {
                                    setReplyingTo(null)
                                    setReplyText('')
                                }}
                            >
                                Cancel
                            </Button>
                        </Group>
                    </Stack>
                </Collapse>

                {/* Render replies */}
                {comment.replies && comment.replies.length > 0 && (
                    <Stack gap="xs" mt="xs">
                        {comment.replies.map((reply) => renderComment(reply, true))}
                    </Stack>
                )}
            </Stack>
        </Paper>
    )

    if (submissionLoading || existingReviewLoading || reviewLoading || commentsLoading) {
        return (
            <Container size="xl" py="xl" pos="relative">
                <LoadingOverlay visible />
            </Container>
        )
    }

    if (!submission) {
        return (
            <Container size="xl" py="xl">
                <Alert color="red" title="Error" icon={<IconInfoCircle size={16} />}>
                    Submission not found or not available for review.
                </Alert>
            </Container>
        )
    }


    return (
        <Container size="xl" py="xl">
            <Stack gap="lg">
                {/* Header */}
                <Paper withBorder p="md">
                    <Group justify="space-between" align="flex-start">
                        <Stack gap="xs">
                            <Group gap="xs" align="center">
                                <ActionIcon
                                    variant="light"
                                    onClick={() => navigate(-1)}
                                >
                                    <IconArrowLeft size={16} />
                                </ActionIcon>
                                <Text size="xl" fw={700}>Code Review</Text>
                                {currentCodeReview && (
                                    <Badge color={getStatusColor(currentCodeReview.status)} variant="filled">
                                        {currentCodeReview.status}
                                    </Badge>
                                )}
                            </Group>
                            <Group gap="md">
                                <Group gap="xs">
                                    <IconUser size={16} />
                                    <Text size="sm">
                                        {submission.firstName ?
                                            `${submission.firstName} (@${submission.username})` :
                                            submission.username
                                        }
                                    </Text>
                                </Group>
                                <Group gap="xs">
                                    <IconClock size={16} />
                                    <Text size="sm" c="dimmed">
                                        Submitted {formatDate(submission.submittedAt)}
                                    </Text>
                                </Group>
                                <Group gap="xs">
                                    <IconCode size={16} />
                                    <Text size="sm" c="dimmed">
                                        {submission.code.split('\n').length} lines
                                    </Text>
                                </Group>
                            </Group>
                        </Stack>
                    </Group>
                </Paper>

                {/* Main Content */}
                <Grid>
                    {/* Left Side - Code */}
                    <Grid.Col span={{ base: 12, lg: 8 }}>
                        <Paper withBorder h="calc(100vh - 300px)">
                            <Stack p="md" gap="md" h="100%">
                                <Group justify="space-between" align="center">
                                    <Text size="lg" fw={600}>Code Submission</Text>
                                    <Badge size="sm" variant="light">
                                        {submission.lessonTitle}
                                    </Badge>
                                </Group>
                                <Divider />

                                <ScrollArea style={{ flex: 1 }}>
                                    <Paper
                                        bg={colorScheme === 'dark' ? 'dark.8' : 'gray.0'}
                                        p="md"
                                        style={{ fontFamily: 'monospace', fontSize: '14px' }}
                                    >
                                        {submission.code.split('\n').map((line, index) => (
                                            <div
                                                key={index}
                                                style={{
                                                    display: 'flex',
                                                    padding: '2px 0',
                                                    backgroundColor: selectedLine === index + 1 ?
                                                        (colorScheme === 'dark' ? 'rgba(66, 165, 245, 0.2)' : '#e3f2fd') :
                                                        'transparent',
                                                    cursor: 'pointer',
                                                    borderRadius: '2px'
                                                }}
                                                onClick={() => setSelectedLine(index + 1)}
                                            >
                                                <Text
                                                    size="sm"
                                                    c="dimmed"
                                                    style={{
                                                        minWidth: '40px',
                                                        textAlign: 'right',
                                                        paddingRight: '12px',
                                                        userSelect: 'none'
                                                    }}
                                                >
                                                    {index + 1}
                                                </Text>
                                                <Text size="sm" style={{ whiteSpace: 'pre', flex: 1 }}>
                                                    {line || ' '}
                                                </Text>
                                            </div>
                                        ))}
                                    </Paper>
                                </ScrollArea>
                            </Stack>
                        </Paper>
                    </Grid.Col>

                    {/* Right Side - Comments */}
                    <Grid.Col span={{ base: 12, lg: 4 }}>
                        <Paper withBorder h="calc(100vh - 300px)" style={{ display: 'flex', flexDirection: 'column' }}>
                            <Stack p="md" gap="md" style={{ flex: 1, minHeight: 0 }}>
                                <Group justify="space-between" align="center">
                                    <Text size="lg" fw={600}>Comments</Text>
                                    <Badge size="sm" variant="light">
                                        {allComments?.length || 0}
                                    </Badge>
                                </Group>
                                <Divider />

                                {/* Comments List */}
                                <ScrollArea style={{ flex: 1 }}>
                                    <Stack gap="sm">
                                        {allComments && allComments.length > 0 ? (
                                            allComments
                                                .filter((comment: CodeReviewComment) => !comment.isReply) // Only show top-level comments
                                                .map((comment: CodeReviewComment) => renderComment(comment))
                                        ) : (
                                            <Text size="sm" c="dimmed" ta="center">
                                                No comments yet. Add the first one!
                                            </Text>
                                        )}
                                    </Stack>
                                </ScrollArea>

                                {/* Add Comment */}
                                <Stack gap="sm">
                                    <Divider />
                                    {selectedLine && (
                                        <Group gap="xs">
                                            <Badge size="sm" variant="light">
                                                Commenting on line {selectedLine}
                                            </Badge>
                                            <Tooltip label="Clear selection">
                                                <ActionIcon
                                                    size="sm"
                                                    variant="subtle"
                                                    onClick={() => setSelectedLine(null)}
                                                >
                                                    <IconX size={12} />
                                                </ActionIcon>
                                            </Tooltip>
                                        </Group>
                                    )}
                                    <Textarea
                                        placeholder="Add a comment..."
                                        value={newComment}
                                        onChange={(e) => setNewComment(e.target.value)}
                                        minRows={3}
                                        maxRows={6}
                                    />
                                    <Button
                                        leftSection={<IconMessageCircle size={16} />}
                                        onClick={handleAddComment}
                                        loading={createCommentMutation.isPending}
                                        disabled={!newComment.trim()}
                                    >
                                        Add Comment
                                    </Button>
                                </Stack>
                            </Stack>
                        </Paper>
                    </Grid.Col>
                </Grid>
            </Stack>
        </Container>
    )
}

export default CodeReviewPage