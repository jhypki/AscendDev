import {
    Container,
    Title,
    Text,
    Grid,
    Card,
    Group,
    Stack,
    Progress,
    Badge,
    ActionIcon,
    Tooltip,
    Center,
    Loader,
    Alert,
    RingProgress,
    Timeline, SimpleGrid,
    Paper,
    ThemeIcon
} from '@mantine/core'
import {
    IconBook,
    IconTrophy,
    IconClock,
    IconFlame,
    IconChevronRight,
    IconBookmark,
    IconCheck,
    IconPlayerPlay,
    IconAlertCircle
} from '@tabler/icons-react'
import { useDashboardStats, useUserProgress, useLearningStreak, useRecentActivity } from '../../hooks/api/useDashboard'
import { Link } from 'react-router-dom'

const DashboardPage = () => {
    const { data: stats, isLoading: statsLoading, error: statsError } = useDashboardStats()
    const { data: progress, isLoading: progressLoading } = useUserProgress()
    const { data: streakData, isLoading: streakLoading } = useLearningStreak(30)
    const { data: recentActivity, isLoading: activityLoading } = useRecentActivity(5)

    if (statsLoading || progressLoading || streakLoading || activityLoading) {
        return (
            <Container size="xl" py="xl">
                <Center>
                    <Loader size="lg" />
                </Center>
            </Container>
        )
    }

    if (statsError) {
        return (
            <Container size="xl" py="xl">
                <Alert
                    icon={<IconAlertCircle size={16} />}
                    title="Error loading dashboard"
                    color="red"
                >
                    Failed to load dashboard data. Please try again later.
                </Alert>
            </Container>
        )
    }

    const formatTime = (minutes: number) => {
        const hours = Math.floor(minutes / 60)
        const mins = minutes % 60
        return `${hours}h ${mins}m`
    }

    const getActivityIcon = (type: string) => {
        switch (type) {
            case 'lesson_completed':
                return <IconCheck size={16} />
            case 'course_started':
                return <IconPlayerPlay size={16} />
            case 'course_completed':
                return <IconTrophy size={16} />
            default:
                return <IconBookmark size={16} />
        }
    }


    return (
        <Container size="xl" py="xl">
            <Stack gap="xl">
                {/* Header */}
                <Group justify="space-between" align="center">
                    <div>
                        <Title order={1}>Dashboard</Title>
                        <Text c="dimmed" size="sm">
                            Welcome back! Here's your learning progress.
                        </Text>
                    </div>
                </Group>

                {/* Stats Cards */}
                <SimpleGrid cols={{ base: 1, sm: 2, lg: 4 }} spacing="lg">
                    <Card shadow="sm" padding="lg" radius="md" withBorder>
                        <Group justify="space-between">
                            <div>
                                <Text c="dimmed" size="sm" tt="uppercase" fw={700}>
                                    Total Courses
                                </Text>
                                <Text fw={700} size="xl">
                                    {stats?.totalCourses || 0}
                                </Text>
                            </div>
                            <ThemeIcon color="blue" size={40} radius="md">
                                <IconBook size={24} />
                            </ThemeIcon>
                        </Group>
                    </Card>

                    <Card shadow="sm" padding="lg" radius="md" withBorder>
                        <Group justify="space-between">
                            <div>
                                <Text c="dimmed" size="sm" tt="uppercase" fw={700}>
                                    Completed
                                </Text>
                                <Text fw={700} size="xl">
                                    {stats?.completedCourses || 0}
                                </Text>
                            </div>
                            <ThemeIcon color="green" size={40} radius="md">
                                <IconTrophy size={24} />
                            </ThemeIcon>
                        </Group>
                    </Card>

                    <Card shadow="sm" padding="lg" radius="md" withBorder>
                        <Group justify="space-between">
                            <div>
                                <Text c="dimmed" size="sm" tt="uppercase" fw={700}>
                                    Study Time
                                </Text>
                                <Text fw={700} size="xl">
                                    {formatTime(stats?.totalStudyTime || 0)}
                                </Text>
                            </div>
                            <ThemeIcon color="orange" size={40} radius="md">
                                <IconClock size={24} />
                            </ThemeIcon>
                        </Group>
                    </Card>

                    <Card shadow="sm" padding="lg" radius="md" withBorder>
                        <Group justify="space-between">
                            <div>
                                <Text c="dimmed" size="sm" tt="uppercase" fw={700}>
                                    Streak
                                </Text>
                                <Text fw={700} size="xl">
                                    {stats?.streakDays || 0} days
                                </Text>
                            </div>
                            <ThemeIcon color="red" size={40} radius="md">
                                <IconFlame size={24} />
                            </ThemeIcon>
                        </Group>
                    </Card>
                </SimpleGrid>

                <Grid>
                    {/* Progress Overview */}
                    <Grid.Col span={{ base: 12, md: 8 }}>
                        <Card shadow="sm" padding="lg" radius="md" withBorder h="100%">
                            <Group justify="space-between" mb="md">
                                <Title order={3}>Course Progress</Title>
                                <ActionIcon
                                    variant="subtle"
                                    component={Link}
                                    to="/courses"
                                >
                                    <IconChevronRight size={16} />
                                </ActionIcon>
                            </Group>

                            <Stack gap="md">
                                {progress?.map((course) => (
                                    <Paper key={course.courseId} p="md" withBorder>
                                        <Group justify="space-between" mb="xs">
                                            <Text fw={500}>{course.courseTitle}</Text>
                                            <Badge
                                                color={course.completionPercentage === 100 ? 'green' : 'blue'}
                                                variant="light"
                                            >
                                                {course.completionPercentage}%
                                            </Badge>
                                        </Group>
                                        <Progress
                                            value={course.completionPercentage}
                                            size="sm"
                                            radius="xl"
                                            mb="xs"
                                        />
                                        <Group justify="space-between">
                                            <Text size="sm" c="dimmed">
                                                {course.completedLessons} of {course.totalLessons} lessons
                                            </Text>
                                            <Text size="sm" c="dimmed">
                                                Last accessed: {new Date(course.lastAccessed).toLocaleDateString()}
                                            </Text>
                                        </Group>
                                    </Paper>
                                ))}
                            </Stack>
                        </Card>
                    </Grid.Col>

                    {/* Overall Progress */}
                    <Grid.Col span={{ base: 12, md: 4 }}>
                        <Card shadow="sm" padding="lg" radius="md" withBorder h="100%">
                            <Title order={3} mb="md">Overall Progress</Title>

                            <Center mb="xl">
                                <RingProgress
                                    size={120}
                                    thickness={12}
                                    sections={[
                                        {
                                            value: stats ? (stats.completedLessons / stats.totalLessons) * 100 : 0,
                                            color: 'blue'
                                        }
                                    ]}
                                    label={
                                        <Text c="blue" fw={700} ta="center" size="xl">
                                            {stats ? Math.round((stats.completedLessons / stats.totalLessons) * 100) : 0}%
                                        </Text>
                                    }
                                />
                            </Center>

                            <Stack gap="sm">
                                <Group justify="space-between">
                                    <Text size="sm">Total Lessons</Text>
                                    <Text size="sm" fw={500}>{stats?.totalLessons || 0}</Text>
                                </Group>
                                <Group justify="space-between">
                                    <Text size="sm">Completed</Text>
                                    <Text size="sm" fw={500}>{stats?.completedLessons || 0}</Text>
                                </Group>
                                <Group justify="space-between">
                                    <Text size="sm">In Progress</Text>
                                    <Text size="sm" fw={500}>{stats?.inProgressCourses || 0}</Text>
                                </Group>
                            </Stack>
                        </Card>
                    </Grid.Col>

                    {/* Recent Activity */}
                    <Grid.Col span={{ base: 12, md: 6 }}>
                        <Card shadow="sm" padding="lg" radius="md" withBorder h="100%">
                            <Title order={3} mb="md">Recent Activity</Title>

                            <Timeline active={-1} bulletSize={24} lineWidth={2}>
                                {recentActivity?.map((activity) => (
                                    <Timeline.Item
                                        key={activity.id}
                                        bullet={getActivityIcon(activity.type)}
                                        title={activity.title}
                                    >
                                        <Text c="dimmed" size="sm">
                                            {activity.courseTitle && `${activity.courseTitle} • `}
                                            {new Date(activity.timestamp).toLocaleString()}
                                        </Text>
                                    </Timeline.Item>
                                ))}
                            </Timeline>
                        </Card>
                    </Grid.Col>

                    {/* Learning Streak */}
                    <Grid.Col span={{ base: 12, md: 6 }}>
                        <Card shadow="sm" padding="lg" radius="md" withBorder h="100%">
                            <Group justify="space-between" mb="md">
                                <Title order={3}>Learning Streak</Title>
                                <Group>
                                    <IconFlame size={20} color="orange" />
                                    <Text fw={700} c="orange">{stats?.streakDays || 0} days</Text>
                                </Group>
                            </Group>

                            <Text size="sm" c="dimmed" mb="md">
                                Keep up the great work! Your consistency is paying off.
                            </Text>

                            {/* Simple streak visualization */}
                            <Group gap="xs" justify="center">
                                {streakData?.slice(-14).map((day) => (
                                    <Tooltip
                                        key={day.date}
                                        label={`${day.date}: ${day.completed} lessons`}
                                    >
                                        <div
                                            style={{
                                                width: 12,
                                                height: 12,
                                                borderRadius: 2,
                                                backgroundColor: day.completed > 0 ? 'var(--mantine-color-green-6)' : 'var(--mantine-color-gray-3)'
                                            }}
                                        />
                                    </Tooltip>
                                ))}
                            </Group>
                        </Card>
                    </Grid.Col>
                </Grid>
            </Stack>
        </Container>
    )
}

export default DashboardPage