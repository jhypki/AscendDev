import { Box, Group, Paper, Progress, Stack, Text, Badge, RingProgress, SimpleGrid } from '@mantine/core'
import { IconCheck, IconClock, IconTrophy, IconTarget } from '@tabler/icons-react'
import type { LessonProgress } from '../../types/lesson'

interface ProgressTrackerProps {
    progress: LessonProgress[]
    currentLessonId?: string
}

interface ProgressStatsProps {
    progress: LessonProgress[]
}

const ProgressStats = ({ progress }: ProgressStatsProps) => {
    const totalLessons = progress.length
    const completedLessons = progress.filter(p => p.completed).length
    const totalAttempts = progress.reduce((sum, p) => sum + p.attempts, 0)
    const totalTimeSpent = progress.reduce((sum, p) => sum + p.timeSpent, 0)
    const averageScore = progress.length > 0
        ? progress.reduce((sum, p) => sum + (p.bestScore || 0), 0) / progress.length
        : 0

    const completionRate = totalLessons > 0 ? (completedLessons / totalLessons) * 100 : 0

    const formatTime = (seconds: number): string => {
        const hours = Math.floor(seconds / 3600)
        const minutes = Math.floor((seconds % 3600) / 60)

        if (hours > 0) {
            return `${hours}h ${minutes}m`
        }
        return `${minutes}m`
    }

    return (
        <SimpleGrid cols={{ base: 2, sm: 4 }} spacing="md">
            <Paper withBorder p="md" ta="center">
                <RingProgress
                    size={80}
                    thickness={8}
                    sections={[{ value: completionRate, color: 'blue' }]}
                    label={
                        <Text size="xs" ta="center" fw={700}>
                            {completionRate.toFixed(0)}%
                        </Text>
                    }
                    mb="xs"
                />
                <Text size="sm" fw={500}>Completion</Text>
                <Text size="xs" c="dimmed">{completedLessons}/{totalLessons} lessons</Text>
            </Paper>

            <Paper withBorder p="md" ta="center">
                <Group justify="center" mb="xs">
                    <IconTrophy size={24} color="var(--mantine-color-yellow-6)" />
                    <Text size="xl" fw={700}>{averageScore.toFixed(1)}</Text>
                </Group>
                <Text size="sm" fw={500}>Avg Score</Text>
                <Text size="xs" c="dimmed">out of 100</Text>
            </Paper>

            <Paper withBorder p="md" ta="center">
                <Group justify="center" mb="xs">
                    <IconTarget size={24} color="var(--mantine-color-green-6)" />
                    <Text size="xl" fw={700}>{totalAttempts}</Text>
                </Group>
                <Text size="sm" fw={500}>Attempts</Text>
                <Text size="xs" c="dimmed">total tries</Text>
            </Paper>

            <Paper withBorder p="md" ta="center">
                <Group justify="center" mb="xs">
                    <IconClock size={24} color="var(--mantine-color-blue-6)" />
                    <Text size="xl" fw={700}>{formatTime(totalTimeSpent)}</Text>
                </Group>
                <Text size="sm" fw={500}>Time Spent</Text>
                <Text size="xs" c="dimmed">learning</Text>
            </Paper>
        </SimpleGrid>
    )
}

interface LessonProgressItemProps {
    lessonProgress: LessonProgress
    lessonNumber: number
    isCurrentLesson?: boolean
}

const LessonProgressItem = ({ lessonProgress, lessonNumber, isCurrentLesson }: LessonProgressItemProps) => {
    const getStatusColor = () => {
        if (lessonProgress.completed) return 'green'
        if (lessonProgress.attempts > 0) return 'yellow'
        return 'gray'
    }

    const getStatusText = () => {
        if (lessonProgress.completed) return 'Completed'
        if (lessonProgress.attempts > 0) return 'In Progress'
        return 'Not Started'
    }

    return (
        <Paper
            withBorder
            p="sm"
            bg={isCurrentLesson ? 'blue.0' : undefined}
            style={{
                borderColor: isCurrentLesson ? 'var(--mantine-color-blue-3)' : undefined
            }}
        >
            <Group justify="space-between" align="center">
                <Group gap="sm" align="center">
                    <Box
                        w={32}
                        h={32}
                        bg={getStatusColor()}
                        style={{
                            borderRadius: '50%',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center'
                        }}
                    >
                        {lessonProgress.completed ? (
                            <IconCheck size={16} color="white" />
                        ) : (
                            <Text size="sm" c="white" fw={700}>
                                {lessonNumber}
                            </Text>
                        )}
                    </Box>

                    <Stack gap={2}>
                        <Group gap="xs" align="center">
                            <Text size="sm" fw={500}>
                                Lesson {lessonNumber}
                            </Text>
                            {isCurrentLesson && (
                                <Badge size="xs" color="blue" variant="filled">
                                    Current
                                </Badge>
                            )}
                        </Group>
                        <Text size="xs" c="dimmed">
                            {getStatusText()}
                        </Text>
                    </Stack>
                </Group>

                <Stack gap={2} align="end">
                    {lessonProgress.bestScore !== undefined && (
                        <Badge size="sm" color={lessonProgress.bestScore >= 80 ? 'green' : 'yellow'}>
                            {lessonProgress.bestScore}%
                        </Badge>
                    )}
                    {lessonProgress.attempts > 0 && (
                        <Text size="xs" c="dimmed">
                            {lessonProgress.attempts} attempt{lessonProgress.attempts !== 1 ? 's' : ''}
                        </Text>
                    )}
                    {lessonProgress.completedAt && (
                        <Text size="xs" c="dimmed">
                            {new Date(lessonProgress.completedAt).toLocaleDateString()}
                        </Text>
                    )}
                </Stack>
            </Group>

            {lessonProgress.attempts > 0 && (
                <Box mt="sm">
                    <Progress
                        value={lessonProgress.bestScore || 0}
                        color={lessonProgress.completed ? 'green' : 'yellow'}
                        size="xs"
                        radius="md"
                    />
                </Box>
            )}
        </Paper>
    )
}

export const ProgressTracker = ({ progress, currentLessonId }: ProgressTrackerProps) => {
    return (
        <Stack gap="lg">
            {/* Overall Progress Stats */}
            <Box>
                <Text size="lg" fw={600} mb="md">Course Progress</Text>
                <ProgressStats progress={progress} />
            </Box>

            {/* Individual Lesson Progress */}
            <Box>
                <Text size="lg" fw={600} mb="md">Lesson Progress</Text>
                <Stack gap="xs">
                    {progress.map((lessonProgress, index) => (
                        <LessonProgressItem
                            key={lessonProgress.lessonId}
                            lessonProgress={lessonProgress}
                            lessonNumber={index + 1}
                            isCurrentLesson={lessonProgress.lessonId === currentLessonId}
                        />
                    ))}
                </Stack>
            </Box>

            {/* Overall Course Progress Bar */}
            <Paper withBorder p="md">
                <Group justify="space-between" mb="xs">
                    <Text size="sm" fw={500}>Overall Course Progress</Text>
                    <Text size="sm" fw={500}>
                        {progress.filter(p => p.completed).length}/{progress.length} completed
                    </Text>
                </Group>
                <Progress
                    value={progress.length > 0 ? (progress.filter(p => p.completed).length / progress.length) * 100 : 0}
                    color="blue"
                    size="lg"
                    radius="md"
                />
            </Paper>
        </Stack>
    )
}

export default ProgressTracker