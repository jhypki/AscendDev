import { useState } from 'react'
import {
    Container,
    Stack,
    Text,
    Paper,
    Group,
    Button,
    Badge,
    Avatar,
    ActionIcon,
    Tooltip,
    LoadingOverlay,
    Alert,
    Tabs,
    Grid,
    Checkbox,
    Modal,
    ScrollArea,
    useComputedColorScheme
} from '@mantine/core'
import {
    IconCode,
    IconClock,
    IconUser,
    IconEye,
    IconTrash,
    IconInfoCircle,
    IconTrashX,
    IconSelectAll
} from '@tabler/icons-react'
import { useNavigate } from 'react-router-dom'
import { notifications } from '@mantine/notifications'
import { useMySubmissions, useDeleteSubmission, useBulkDeleteSubmissions } from '../../hooks/api/useSubmissions'
import { useMySubmissionsUnderReview } from '../../hooks/api/useCodeReviews'
import type { Submission, CodeReview } from '../../types/submission'

export const MySubmissionsPage = () => {
    const navigate = useNavigate()
    const colorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true })
    const [activeTab, setActiveTab] = useState<string>('submissions')
    const [selectedSubmissions, setSelectedSubmissions] = useState<Set<number>>(new Set())
    const [viewCodeModal, setViewCodeModal] = useState<{ opened: boolean; submission: Submission | null }>({
        opened: false,
        submission: null
    })

    // API hooks
    const { data: submissions, isLoading: submissionsLoading, error: submissionsError } = useMySubmissions()
    const { data: reviewsInProgress, isLoading: reviewsLoading, error: reviewsError } = useMySubmissionsUnderReview()
    const deleteSubmissionMutation = useDeleteSubmission()
    const bulkDeleteMutation = useBulkDeleteSubmissions()

    const handleDeleteSubmission = async (submissionId: number) => {
        if (!confirm('Are you sure you want to delete this submission?')) return

        try {
            await deleteSubmissionMutation.mutateAsync(submissionId)
            notifications.show({
                title: 'Submission Deleted',
                message: 'Your submission has been deleted successfully.',
                color: 'green'
            })
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to delete submission. Please try again.',
                color: 'red'
            })
        }
    }

    const handleBulkDelete = async () => {
        if (selectedSubmissions.size === 0) return

        const submissionIds = Array.from(selectedSubmissions)
        if (!confirm(`Are you sure you want to delete ${submissionIds.length} submission${submissionIds.length > 1 ? 's' : ''}?`)) return

        try {
            const result = await bulkDeleteMutation.mutateAsync(submissionIds)
            notifications.show({
                title: 'Submissions Deleted',
                message: `${result.deletedCount} submission${result.deletedCount > 1 ? 's' : ''} deleted successfully.`,
                color: 'green'
            })
            setSelectedSubmissions(new Set())
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to delete submissions. Please try again.',
                color: 'red'
            })
        }
    }

    const handleSelectAll = () => {
        if (!submissions) return

        if (selectedSubmissions.size === submissions.length) {
            setSelectedSubmissions(new Set())
        } else {
            setSelectedSubmissions(new Set(submissions.map(s => s.id)))
        }
    }

    const handleSelectSubmission = (submissionId: number) => {
        const newSelected = new Set(selectedSubmissions)
        if (newSelected.has(submissionId)) {
            newSelected.delete(submissionId)
        } else {
            newSelected.add(submissionId)
        }
        setSelectedSubmissions(newSelected)
    }

    const handleViewCode = (submission: Submission) => {
        setViewCodeModal({ opened: true, submission })
    }

    const formatExecutionTime = (timeMs: number) => {
        if (timeMs < 1000) {
            return `${timeMs}ms`
        }
        return `${(timeMs / 1000).toFixed(2)}s`
    }

    const formatSubmissionDate = (dateString: string) => {
        const date = new Date(dateString)
        return date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        })
    }

    const getStatusColor = (status: string | undefined) => {
        if (!status || typeof status !== 'string') {
            return 'gray'
        }
        switch (status.toLowerCase()) {
            case 'pending':
                return 'yellow'
            case 'in_review':
                return 'blue'
            case 'changes_requested':
                return 'orange'
            case 'approved':
                return 'green'
            case 'completed':
                return 'gray'
            default:
                return 'gray'
        }
    }

    const renderSubmissionCard = (submission: Submission) => (
        <Paper key={submission.id} withBorder p="md" h="320px" style={{ display: 'flex', flexDirection: 'column' }}>
            <Stack gap="sm" style={{ flex: 1, overflow: 'hidden' }}>
                {/* Header */}
                <Group justify="space-between" align="flex-start" style={{ flexShrink: 0 }}>
                    <Group gap="sm">
                        <Checkbox
                            checked={selectedSubmissions.has(submission.id)}
                            onChange={() => handleSelectSubmission(submission.id)}
                        />
                        <div>
                            <Text size="lg" fw={600}>{submission.lessonTitle}</Text>
                            <Group gap="xs" mt="xs">
                                <Badge
                                    color={submission.passed ? 'green' : 'red'}
                                    variant="filled"
                                    size="sm"
                                >
                                    {submission.passed ? 'Passed' : 'Failed'}
                                </Badge>
                                <Text size="sm" c="dimmed">
                                    {formatSubmissionDate(submission.submittedAt)}
                                </Text>
                            </Group>
                        </div>
                    </Group>

                    <Group gap="xs">
                        <Tooltip label="View Code">
                            <ActionIcon
                                size="sm"
                                variant="light"
                                onClick={() => handleViewCode(submission)}
                            >
                                <IconEye size={14} />
                            </ActionIcon>
                        </Tooltip>
                        <Tooltip label="Delete Submission">
                            <ActionIcon
                                size="sm"
                                variant="light"
                                color="red"
                                onClick={() => handleDeleteSubmission(submission.id)}
                                loading={deleteSubmissionMutation.isPending}
                            >
                                <IconTrash size={14} />
                            </ActionIcon>
                        </Tooltip>
                    </Group>
                </Group>

                {/* Stats */}
                <Group gap="md" style={{ flexShrink: 0 }}>
                    <Group gap="xs">
                        <IconClock size={14} />
                        <Text size="xs" c="dimmed">
                            {formatExecutionTime(submission.executionTimeMs)}
                        </Text>
                    </Group>
                    <Group gap="xs">
                        <IconCode size={14} />
                        <Text size="xs" c="dimmed">
                            {submission.code.split('\n').length} lines
                        </Text>
                    </Group>
                </Group>

                {/* Error Message */}
                {submission.errorMessage && (
                    <Alert color="red" style={{ flexShrink: 0, maxHeight: '60px', overflow: 'hidden' }}>
                        <Text size="sm" style={{
                            overflow: 'hidden',
                            textOverflow: 'ellipsis',
                            display: '-webkit-box',
                            WebkitLineClamp: 2,
                            WebkitBoxOrient: 'vertical'
                        }}>
                            {submission.errorMessage}
                        </Text>
                    </Alert>
                )}

                {/* Code Preview */}
                <Paper
                    bg={colorScheme === 'dark' ? 'dark.6' : 'gray.0'}
                    p="xs"
                    style={{
                        borderRadius: 4,
                        flex: 1,
                        minHeight: '60px',
                        overflow: 'hidden'
                    }}
                >
                    <Text
                        size="xs"
                        ff="monospace"
                        style={{
                            whiteSpace: 'pre-wrap',
                            overflow: 'hidden',
                            textOverflow: 'ellipsis',
                            display: '-webkit-box',
                            WebkitLineClamp: submission.errorMessage ? 3 : 4,
                            WebkitBoxOrient: 'vertical',
                            height: '100%'
                        }}
                    >
                        {submission.code}
                    </Text>
                </Paper>
            </Stack>
        </Paper>
    )

    const renderCodeReviewCard = (review: CodeReview) => (
        <Paper key={review.id} withBorder p="md" h="280px" style={{ display: 'flex', flexDirection: 'column' }}>
            <Stack gap="sm" style={{ flex: 1, justifyContent: 'space-between' }}>
                {/* Header */}
                <Group justify="space-between" align="flex-start">
                    <div>
                        <Text size="lg" fw={600}>{review.submission?.lessonTitle}</Text>
                        <Group gap="xs" mt="xs">
                            <Badge
                                color={getStatusColor(review.status)}
                                variant="filled"
                                size="sm"
                            >
                                {review.status}
                            </Badge>
                            <Text size="sm" c="dimmed">
                                Started {formatSubmissionDate(review.createdAt)}
                            </Text>
                        </Group>
                    </div>

                    <Group gap="xs">
                        <Button
                            size="xs"
                            variant="light"
                            onClick={() => navigate(`/submissions/${review.submissionId}/review`)}
                        >
                            View Review
                        </Button>
                    </Group>
                </Group>

                {/* Reviewer Info */}
                <Group gap="xs">
                    <Avatar size="sm">
                        <IconUser size={16} />
                    </Avatar>
                    <div>
                        <Text size="sm" fw={500}>
                            Reviewed by {review.reviewer.username}
                        </Text>
                        <Text size="xs" c="dimmed">
                            {review.commentCount} comments
                        </Text>
                    </div>
                </Group>

                {/* Review Duration */}
                {review.reviewDuration && (
                    <Group gap="xs">
                        <IconClock size={14} />
                        <Text size="xs" c="dimmed">
                            Review took {review.reviewDuration}
                        </Text>
                    </Group>
                )}
            </Stack>
        </Paper>
    )

    return (
        <Container size="xl" py="xl">
            <Stack gap="lg">
                {/* Header */}
                <Paper withBorder p="md">
                    <Group justify="space-between" align="center">
                        <div>
                            <Text size="xl" fw={700}>My Submissions</Text>
                            <Text size="sm" c="dimmed">
                                View and manage your code submissions and reviews
                            </Text>
                        </div>

                        {activeTab === 'submissions' && submissions && submissions.length > 0 && (
                            <Group gap="sm">
                                {selectedSubmissions.size > 0 && (
                                    <Button
                                        size="sm"
                                        color="red"
                                        variant="light"
                                        leftSection={<IconTrashX size={16} />}
                                        onClick={handleBulkDelete}
                                        loading={bulkDeleteMutation.isPending}
                                    >
                                        Delete Selected ({selectedSubmissions.size})
                                    </Button>
                                )}
                                <Button
                                    size="sm"
                                    variant="light"
                                    leftSection={<IconSelectAll size={16} />}
                                    onClick={handleSelectAll}
                                >
                                    {selectedSubmissions.size === submissions.length ? 'Deselect All' : 'Select All'}
                                </Button>
                            </Group>
                        )}
                    </Group>
                </Paper>

                {/* Tabs */}
                <Tabs value={activeTab} onChange={(value) => setActiveTab(value || 'submissions')}>
                    <Tabs.List>
                        <Tabs.Tab value="submissions" leftSection={<IconCode size={16} />}>
                            My Submissions
                            {submissions && (
                                <Badge size="sm" variant="light" ml="xs">
                                    {submissions.length}
                                </Badge>
                            )}
                        </Tabs.Tab>
                        <Tabs.Tab value="reviews" leftSection={<IconEye size={16} />}>
                            Under Review
                            {reviewsInProgress && (
                                <Badge size="sm" variant="light" ml="xs">
                                    {reviewsInProgress.length}
                                </Badge>
                            )}
                        </Tabs.Tab>
                    </Tabs.List>

                    <Tabs.Panel value="submissions" pt="md">
                        {submissionsLoading ? (
                            <Paper withBorder h="400px" pos="relative">
                                <LoadingOverlay visible />
                            </Paper>
                        ) : submissionsError ? (
                            <Alert color="red" icon={<IconInfoCircle size={16} />}>
                                Failed to load submissions. Please try again later.
                            </Alert>
                        ) : !submissions || submissions.length === 0 ? (
                            <Alert icon={<IconInfoCircle size={16} />}>
                                You haven't made any submissions yet. Start coding to see your submissions here!
                            </Alert>
                        ) : (
                            <Grid>
                                {submissions.map((submission) => (
                                    <Grid.Col key={submission.id} span={{ base: 12, md: 6, lg: 4 }}>
                                        {renderSubmissionCard(submission)}
                                    </Grid.Col>
                                ))}
                            </Grid>
                        )}
                    </Tabs.Panel>

                    <Tabs.Panel value="reviews" pt="md">
                        {reviewsLoading ? (
                            <Paper withBorder h="400px" pos="relative">
                                <LoadingOverlay visible />
                            </Paper>
                        ) : reviewsError ? (
                            <Alert color="red" icon={<IconInfoCircle size={16} />}>
                                Failed to load reviews. Please try again later.
                            </Alert>
                        ) : !reviewsInProgress || reviewsInProgress.length === 0 ? (
                            <Alert icon={<IconInfoCircle size={16} />}>
                                None of your submissions are currently under review.
                            </Alert>
                        ) : (
                            <Grid>
                                {reviewsInProgress.map((review) => (
                                    <Grid.Col key={review.id} span={{ base: 12, md: 6, lg: 4 }}>
                                        {renderCodeReviewCard(review)}
                                    </Grid.Col>
                                ))}
                            </Grid>
                        )}
                    </Tabs.Panel>
                </Tabs>
            </Stack>

            {/* View Code Modal */}
            <Modal
                opened={viewCodeModal.opened}
                onClose={() => setViewCodeModal({ opened: false, submission: null })}
                title={
                    <Group gap="sm">
                        <IconCode size={20} />
                        <Text fw={600}>
                            {viewCodeModal.submission?.lessonTitle} - Code Submission
                        </Text>
                    </Group>
                }
                size="xl"
                centered
            >
                {viewCodeModal.submission && (
                    <Stack gap="md">
                        {/* Submission Info */}
                        <Group gap="md">
                            <Badge
                                color={viewCodeModal.submission.passed ? 'green' : 'red'}
                                variant="filled"
                                size="sm"
                            >
                                {viewCodeModal.submission.passed ? 'Passed' : 'Failed'}
                            </Badge>
                            <Group gap="xs">
                                <IconClock size={14} />
                                <Text size="sm" c="dimmed">
                                    {formatExecutionTime(viewCodeModal.submission.executionTimeMs)}
                                </Text>
                            </Group>
                            <Group gap="xs">
                                <IconCode size={14} />
                                <Text size="sm" c="dimmed">
                                    {viewCodeModal.submission.code.split('\n').length} lines
                                </Text>
                            </Group>
                            <Text size="sm" c="dimmed">
                                {formatSubmissionDate(viewCodeModal.submission.submittedAt)}
                            </Text>
                        </Group>

                        {/* Error Message */}
                        {viewCodeModal.submission.errorMessage && (
                            <Alert color="red">
                                <Text size="sm">{viewCodeModal.submission.errorMessage}</Text>
                            </Alert>
                        )}

                        {/* Code Display */}
                        <Paper
                            bg={colorScheme === 'dark' ? 'dark.8' : 'gray.0'}
                            p="md"
                            style={{ fontFamily: 'monospace', fontSize: '14px' }}
                        >
                            <ScrollArea h={400}>
                                {viewCodeModal.submission.code.split('\n').map((line, index) => (
                                    <div
                                        key={index}
                                        style={{
                                            display: 'flex',
                                            padding: '2px 0',
                                        }}
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
                            </ScrollArea>
                        </Paper>
                    </Stack>
                )}
            </Modal>
        </Container>
    )
}

export default MySubmissionsPage