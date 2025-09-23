import {
    Stack,
    Text,
    Paper,
    Group,
    Button,
    Badge,
    Avatar, LoadingOverlay,
    Alert,
    ScrollArea,
    useComputedColorScheme
} from '@mantine/core'
import {
    IconCode,
    IconClock,
    IconUser, IconMessageCircle,
    IconInfoCircle
} from '@tabler/icons-react'
import { useSubmissionsForReview } from '../../hooks/api/useSubmissions'
import type { PublicSubmission } from '../../types/submission'

interface SubmissionsPanelProps {
    lessonId: string
    onViewSubmission?: (submission: PublicSubmission) => void
}

export const SubmissionsPanel = ({ lessonId, onViewSubmission }: SubmissionsPanelProps) => {
    const { data: submissions, isLoading, error } = useSubmissionsForReview(lessonId)
    const colorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true })

    const formatExecutionTime = (timeMs: number) => {
        if (timeMs < 1000) {
            return `${timeMs}ms`
        }
        return `${(timeMs / 1000).toFixed(2)}s`
    }

    const formatSubmissionDate = (dateString: string) => {
        const date = new Date(dateString)
        const now = new Date()
        const diffInHours = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60))

        if (diffInHours < 1) {
            return 'Just now'
        } else if (diffInHours < 24) {
            return `${diffInHours}h ago`
        } else {
            const diffInDays = Math.floor(diffInHours / 24)
            return `${diffInDays}d ago`
        }
    }

    if (isLoading) {
        return (
            <Paper withBorder h="100%" pos="relative">
                <LoadingOverlay visible />
                <Stack p="md" gap="md">
                    <Text size="lg" fw={600}>Community Submissions</Text>
                </Stack>
            </Paper>
        )
    }

    if (error) {
        return (
            <Paper withBorder h="100%">
                <Stack p="md" gap="md">
                    <Text size="lg" fw={600}>Community Submissions</Text>
                    <Alert color="red" icon={<IconInfoCircle size={16} />}>
                        Failed to load submissions. Please try again later.
                    </Alert>
                </Stack>
            </Paper>
        )
    }

    return (
        <Paper withBorder h="100%" style={{ display: 'flex', flexDirection: 'column' }}>
            <Stack p="md" gap="md" style={{ flex: 1, minHeight: 0 }}>
                <Group justify="space-between" align="center">
                    <Text size="lg" fw={600}>Community Submissions</Text>
                    <Badge size="sm" variant="light">
                        {submissions?.length || 0} available
                    </Badge>
                </Group>

                {!submissions || submissions.length === 0 ? (
                    <Alert icon={<IconInfoCircle size={16} />}>
                        No submissions available for review yet. Check back later!
                    </Alert>
                ) : (
                    <ScrollArea style={{ flex: 1 }}>
                        <Stack gap="sm">
                            {submissions.map((submission) => (
                                <Paper key={submission.id} withBorder p="sm" style={{ cursor: 'pointer' }}>
                                    <Stack gap="xs">
                                        {/* Header */}
                                        <Group justify="space-between" align="flex-start">
                                            <Group gap="xs" align="center">
                                                <Avatar
                                                    size="sm"
                                                    src={submission.profilePictureUrl}
                                                    alt={submission.username}
                                                >
                                                    <IconUser size={16} />
                                                </Avatar>
                                                <div>
                                                    <Text size="sm" fw={500}>
                                                        {submission.firstName ?
                                                            `${submission.firstName} (@${submission.username})` :
                                                            submission.username
                                                        }
                                                    </Text>
                                                    <Text size="xs" c="dimmed">
                                                        {formatSubmissionDate(submission.submittedAt)}
                                                    </Text>
                                                </div>
                                            </Group>

                                            <Group gap="xs">
                                                <Button
                                                    size="xs"
                                                    variant="light"
                                                    leftSection={<IconMessageCircle size={12} />}
                                                    onClick={() => onViewSubmission?.(submission)}
                                                >
                                                    Review
                                                </Button>
                                            </Group>
                                        </Group>

                                        {/* Stats */}
                                        <Group gap="md">
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

                                        {/* Code Preview */}
                                        <Paper
                                            bg={colorScheme === 'dark' ? 'dark.8' : 'gray.0'}
                                            p="xs"
                                            style={{ borderRadius: 4 }}
                                        >
                                            <Text
                                                size="xs"
                                                ff="monospace"
                                                style={{
                                                    whiteSpace: 'pre-wrap',
                                                    overflow: 'hidden',
                                                    textOverflow: 'ellipsis',
                                                    display: '-webkit-box',
                                                    WebkitLineClamp: 3,
                                                    WebkitBoxOrient: 'vertical'
                                                }}
                                            >
                                                {submission.code}
                                            </Text>
                                        </Paper>
                                    </Stack>
                                </Paper>
                            ))}
                        </Stack>
                    </ScrollArea>
                )}
            </Stack>
        </Paper>
    )
}