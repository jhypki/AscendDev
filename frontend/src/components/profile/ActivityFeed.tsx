import {
    Card,
    Text,
    Group,
    Stack,
    Badge,
    ActionIcon,
    Pagination,
    Center,
    Loader,
    Alert,
    Timeline, Title,
    Select,
    Button,
    useMantineColorScheme,
    useMantineTheme
} from '@mantine/core'
import {
    IconTrophy,
    IconBook,
    IconMessageCircle,
    IconArrowBackUp,
    IconRefresh,
    IconFilter,
    IconClock
} from '@tabler/icons-react'
import { useState } from 'react'
import dayjs from 'dayjs'
import relativeTime from 'dayjs/plugin/relativeTime'
import { useActivityFeed } from '../../hooks/api/useUserProfile'
import type { ActivityItem, ActivityFeedRequest } from '../../types/profile'

dayjs.extend(relativeTime)

interface ActivityFeedProps {
    userId?: string
    showFilters?: boolean
    pageSize?: number
}

const ActivityFeed = ({ showFilters = true, pageSize = 10 }: ActivityFeedProps) => {
    const [filters, setFilters] = useState<ActivityFeedRequest>({
        page: 1,
        pageSize,
        activityTypes: undefined
    })

    const { data, isLoading, error, refetch } = useActivityFeed(filters)
    const { colorScheme } = useMantineColorScheme()
    const theme = useMantineTheme()
    const isDark = colorScheme === 'dark'

    const getActivityIcon = (type: ActivityItem['type']) => {
        switch (type) {
            case 'lesson_completed':
                return <IconBook size={16} />
            case 'course_completed':
                return <IconTrophy size={16} />
            case 'achievement_unlocked':
                return <IconTrophy size={16} />
            case 'discussion_created':
                return <IconMessageCircle size={16} />
            case 'discussion_replied':
                return <IconArrowBackUp size={16} />
            default:
                return <IconClock size={16} />
        }
    }

    const getActivityColor = (type: ActivityItem['type']) => {
        switch (type) {
            case 'lesson_completed':
                return 'blue'
            case 'course_completed':
                return 'green'
            case 'achievement_unlocked':
                return 'yellow'
            case 'discussion_created':
                return 'violet'
            case 'discussion_replied':
                return 'grape'
            default:
                return 'gray'
        }
    }

    const getActivityTitle = (activity: ActivityItem) => {
        switch (activity.type) {
            case 'lesson_completed':
                return 'Completed Lesson'
            case 'course_completed':
                return 'Completed Course'
            case 'achievement_unlocked':
                return 'Achievement Unlocked'
            case 'discussion_created':
                return 'Started Discussion'
            case 'discussion_replied':
                return 'Replied to Discussion'
            default:
                return 'Activity'
        }
    }

    const handlePageChange = (page: number) => {
        setFilters(prev => ({ ...prev, page }))
    }

    const handleFilterChange = (activityTypes: string[] | undefined) => {
        setFilters(prev => ({ ...prev, activityTypes, page: 1 }))
    }

    const activityTypeOptions = [
        { value: 'lesson_completed', label: 'Lessons Completed' },
        { value: 'course_completed', label: 'Courses Completed' },
        { value: 'achievement_unlocked', label: 'Achievements' },
        { value: 'discussion_created', label: 'Discussions Created' },
        { value: 'discussion_replied', label: 'Discussion Replies' }
    ]

    if (isLoading) {
        return (
            <Card shadow="sm" padding="lg" radius="md" withBorder>
                <Center py="xl">
                    <Loader size="md" />
                </Center>
            </Card>
        )
    }

    if (error) {
        return (
            <Card shadow="sm" padding="lg" radius="md" withBorder>
                <Alert color="red" title="Error loading activity feed">
                    Failed to load activity data. Please try again.
                </Alert>
            </Card>
        )
    }

    if (!data || !data.activities || data.activities.length === 0) {
        return (
            <Card shadow="sm" padding="lg" radius="md" withBorder>
                <Center py="xl">
                    <Stack align="center" gap="md">
                        <IconClock size={48} stroke={1} color="gray" />
                        <Text c="dimmed">No activity yet</Text>
                        <Text size="sm" c="dimmed" ta="center">
                            Start learning to see your activity here!
                        </Text>
                    </Stack>
                </Center>
            </Card>
        )
    }

    return (
        <Card
            shadow="md"
            padding="xl"
            radius="lg"
            withBorder
            style={{
                background: isDark
                    ? `linear-gradient(135deg, ${theme.colors.dark[7]} 0%, ${theme.colors.dark[6]} 100%)`
                    : `linear-gradient(135deg, ${theme.colors.gray[0]} 0%, ${theme.colors.blue[0]} 100%)`,
                border: `1px solid ${isDark ? theme.colors.dark[4] : theme.colors.gray[2]}`
            }}
        >
            <Group justify="space-between" mb="lg">
                <Group>
                    <Title order={3} size="h4" c="dark">Activity Feed</Title>
                    <Badge variant="light" size="sm" color="blue">
                        {data?.totalCount || 0} activities
                    </Badge>
                </Group>
                <ActionIcon
                    variant="gradient"
                    gradient={{ from: 'blue', to: 'indigo' }}
                    onClick={() => refetch()}
                    loading={isLoading}
                    size="lg"
                >
                    <IconRefresh size={18} />
                </ActionIcon>
            </Group>

            {showFilters && (
                <Card
                    padding="md"
                    radius="md"
                    mb="lg"
                    bg={isDark ? theme.colors.dark[6] : 'white'}
                    withBorder
                    style={{ border: `1px solid ${isDark ? theme.colors.dark[4] : theme.colors.gray[2]}` }}
                >
                    <Group>
                        <Select
                            placeholder="Filter by activity type"
                            data={activityTypeOptions}
                            value={filters.activityTypes?.[0] || null}
                            onChange={(value) => handleFilterChange(value ? [value] : undefined)}
                            clearable
                            leftSection={<IconFilter size={16} />}
                            style={{ minWidth: 200 }}
                        />
                        <Button
                            variant="light"
                            size="sm"
                            onClick={() => handleFilterChange(undefined)}
                        >
                            Clear Filters
                        </Button>
                    </Group>
                </Card>
            )}

            <Timeline
                active={data?.activities?.length || 0}
                bulletSize={32}
                lineWidth={3}
                color="blue"
            >
                {data?.activities?.map((activity, index) => (
                    <Timeline.Item
                        key={activity.id}
                        bullet={
                            <Card
                                padding="xs"
                                radius="xl"
                                bg={getActivityColor(activity.type)}
                                style={{ border: 'none' }}
                            >
                                {getActivityIcon(activity.type)}
                            </Card>
                        }
                        title={
                            <Card
                                padding="md"
                                radius="md"
                                withBorder
                                bg={isDark ? theme.colors.dark[6] : 'white'}
                                style={{
                                    boxShadow: isDark ? '0 2px 8px rgba(0,0,0,0.3)' : '0 2px 8px rgba(0,0,0,0.1)',
                                    marginBottom: index === (data?.activities?.length || 0) - 1 ? 0 : '1rem',
                                    border: `1px solid ${isDark ? theme.colors.dark[4] : theme.colors.gray[2]}`
                                }}
                            >
                                <Stack gap="sm">
                                    <Group justify="space-between">
                                        <Group gap="xs">
                                            <Text size="sm" fw={600}>
                                                {getActivityTitle(activity)}
                                            </Text>
                                            <Badge
                                                size="xs"
                                                variant="gradient"
                                                gradient={{ from: getActivityColor(activity.type), to: 'indigo' }}
                                            >
                                                {activity.type.replace('_', ' ')}
                                            </Badge>
                                        </Group>
                                        <Text size="xs" c="dimmed">
                                            {dayjs(activity.createdAt).fromNow()}
                                        </Text>
                                    </Group>

                                    <Text size="sm" c="dark">
                                        {activity.description}
                                    </Text>

                                    {/* Additional metadata based on activity type */}
                                    {activity.metadata && (
                                        <Group gap="xs">
                                            {activity.type === 'lesson_completed' &&
                                                typeof activity.metadata === 'object' &&
                                                activity.metadata !== null &&
                                                'score' in activity.metadata && (
                                                    <Badge size="sm" variant="light" color="green">
                                                        Score: {String(activity.metadata.score)}%
                                                    </Badge>
                                                )}
                                            {activity.type === 'course_completed' &&
                                                typeof activity.metadata === 'object' &&
                                                activity.metadata !== null &&
                                                'completionTime' in activity.metadata && (
                                                    <Badge size="sm" variant="light" color="blue">
                                                        Completed in {String(activity.metadata.completionTime)}
                                                    </Badge>
                                                )}
                                            {activity.type === 'achievement_unlocked' &&
                                                typeof activity.metadata === 'object' &&
                                                activity.metadata !== null &&
                                                'category' in activity.metadata && (
                                                    <Badge size="sm" variant="light" color="yellow">
                                                        {String(activity.metadata.category)}
                                                    </Badge>
                                                )}
                                        </Group>
                                    )}
                                </Stack>
                            </Card>
                        }
                    />
                ))}
            </Timeline>

            {(data?.totalPages || 0) > 1 && (
                <Center mt="xl">
                    <Pagination
                        value={data?.page || 1}
                        onChange={handlePageChange}
                        total={data?.totalPages || 1}
                        size="md"
                        color="blue"
                    />
                </Center>
            )}

            <Card
                padding="sm"
                radius="md"
                mt="md"
                bg={isDark ? theme.colors.dark[6] : theme.colors.gray[0]}
                withBorder
                style={{ border: `1px solid ${isDark ? theme.colors.dark[4] : theme.colors.gray[2]}` }}
            >
                <Group justify="space-between">
                    <Text size="xs" c="dimmed" fw={500}>
                        Showing {data?.activities?.length || 0} of {data?.totalCount || 0} activities
                    </Text>
                    <Text size="xs" c="dimmed" fw={500}>
                        Page {data?.page || 1} of {data?.totalPages || 1}
                    </Text>
                </Group>
            </Card>
        </Card>
    )
}

export default ActivityFeed