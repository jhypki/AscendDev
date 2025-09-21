import {
    Container,
    Title,
    Text,
    Grid,
    Card,
    Badge,
    Button,
    Group,
    Stack,
    Image,
    Divider,
    Progress,
    Alert,
    Center,
    Loader,
    ActionIcon,
    Menu,
    rem,
    Paper, ThemeIcon
} from '@mantine/core'
import {
    IconPlayerPlay,
    IconCheck,
    IconLock,
    IconDots,
    IconEdit,
    IconTrash,
    IconAlertCircle,
    IconBook,
    IconClock
} from '@tabler/icons-react'
import { Link, useParams, useNavigate } from 'react-router-dom'
import { notifications } from '@mantine/notifications'
import { useCourse, useCourseLessons, useDeleteCourse } from '../../hooks/api/useCourses'
import { useCourseProgress } from '../../hooks/api/useUserProgress'
import { useAppSelector } from '../../store/hooks'
import CourseForm from '../../components/courses/CourseForm'
import { useDisclosure } from '@mantine/hooks'
import type { Lesson } from '../../types/course'

const CourseDetailPage = () => {
    const { courseId } = useParams<{ courseId: string }>()
    const navigate = useNavigate()
    const { user } = useAppSelector((state) => state.auth)
    const isAdmin = user?.userRoles?.includes('Admin') || user?.userRoles?.includes('Instructor')
    const [editModalOpened, { open: openEditModal, close: closeEditModal }] = useDisclosure(false)

    const {
        data: course,
        isLoading: courseLoading,
        error: courseError
    } = useCourse(courseId!, !!courseId)

    const {
        data: lessons,
        isLoading: lessonsLoading,
        error: lessonsError
    } = useCourseLessons(courseId!, !!courseId)

    const {
        data: progressData,
        isLoading: progressLoading,
        error: progressError
    } = useCourseProgress(courseId!)

    const deleteCourseMutation = useDeleteCourse()

    const isLoading = courseLoading || lessonsLoading || progressLoading
    const error = courseError || lessonsError || progressError

    const handleEditCourse = () => {
        openEditModal()
    }

    const handleDeleteCourse = async () => {
        if (!courseId || !course) return

        try {
            await deleteCourseMutation.mutateAsync(courseId)
            notifications.show({
                title: 'Success',
                message: 'Course deleted successfully',
                color: 'green'
            })
            navigate('/courses')
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to delete course',
                color: 'red'
            })
        }
    }

    const getStatusColor = (status: string) => {
        switch (status) {
            case 'published':
                return 'green'
            case 'draft':
                return 'yellow'
            case 'archived':
                return 'gray'
            default:
                return 'blue'
        }
    }

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        })
    }

    // Get real progress data from API
    const completedLessons = new Set(progressData?.completedLessonIds || [])
    const progressPercentage = progressData?.completionPercentage || 0

    const getLessonStatus = (lesson: Lesson, index: number) => {
        if (completedLessons.has(lesson.id)) {
            return 'completed'
        }
        if (index === 0 || completedLessons.has(lessons?.[index - 1]?.id || '')) {
            return 'available'
        }
        return 'locked'
    }

    const getLessonIcon = (status: string) => {
        switch (status) {
            case 'completed':
                return <IconCheck size={16} />
            case 'available':
                return <IconPlayerPlay size={16} />
            case 'locked':
                return <IconLock size={16} />
            default:
                return <IconPlayerPlay size={16} />
        }
    }

    const getLessonColor = (status: string) => {
        switch (status) {
            case 'completed':
                return 'green'
            case 'available':
                return 'blue'
            case 'locked':
                return 'gray'
            default:
                return 'blue'
        }
    }

    if (error) {
        return (
            <Container size="xl" py="xl">
                <Alert
                    icon={<IconAlertCircle size={16} />}
                    title="Error loading course"
                    color="red"
                >
                    {error instanceof Error ? error.message : 'An unexpected error occurred'}
                </Alert>
            </Container>
        )
    }

    if (isLoading) {
        return (
            <Container size="xl" py="xl">
                <Center>
                    <Loader size="lg" />
                </Center>
            </Container>
        )
    }

    if (!course) {
        return (
            <Container size="xl" py="xl">
                <Alert
                    icon={<IconAlertCircle size={16} />}
                    title="Course not found"
                    color="yellow"
                >
                    The requested course could not be found.
                </Alert>
            </Container>
        )
    }

    return (
        <Container size="xl" py="xl">
            <Grid>
                {/* Course Information */}
                <Grid.Col span={{ base: 12, md: 8 }}>
                    <Stack gap="xl">
                        {/* Course Header */}
                        <Card shadow="sm" padding="lg" radius="md" withBorder>
                            <Group justify="space-between" align="flex-start" mb="md">
                                <div style={{ flex: 1 }}>
                                    <Group gap="xs" mb="xs">
                                        <Badge color={getStatusColor(course.status)} variant="light">
                                            {course.status.charAt(0).toUpperCase() + course.status.slice(1)}
                                        </Badge>
                                        <Badge color="blue" variant="outline">
                                            {course.language}
                                        </Badge>
                                    </Group>
                                    <Title order={1} mb="xs">{course.title}</Title>
                                    <Text c="dimmed" size="sm">
                                        {course.updatedAt ? `Last updated ${formatDate(course.updatedAt)}` : `Created ${formatDate(course.createdAt)}`}
                                    </Text>
                                </div>

                                {isAdmin && (
                                    <Menu shadow="md" width={200}>
                                        <Menu.Target>
                                            <ActionIcon variant="subtle" color="gray">
                                                <IconDots style={{ width: rem(16), height: rem(16) }} />
                                            </ActionIcon>
                                        </Menu.Target>

                                        <Menu.Dropdown>
                                            <Menu.Item
                                                leftSection={<IconEdit style={{ width: rem(14), height: rem(14) }} />}
                                                onClick={handleEditCourse}
                                            >
                                                Edit Course
                                            </Menu.Item>
                                            <Menu.Divider />
                                            <Menu.Item
                                                color="red"
                                                leftSection={<IconTrash style={{ width: rem(14), height: rem(14) }} />}
                                                onClick={handleDeleteCourse}
                                            >
                                                Delete Course
                                            </Menu.Item>
                                        </Menu.Dropdown>
                                    </Menu>
                                )}
                            </Group>

                            {course.featuredImage && (
                                <Image
                                    src={course.featuredImage}
                                    height={200}
                                    radius="md"
                                    mb="md"
                                    alt={course.title}
                                />
                            )}

                            <Text mb="md">{course.description}</Text>

                            {course.tags.length > 0 && (
                                <Group gap="xs">
                                    {course.tags.map((tag) => (
                                        <Badge key={tag} size="sm" variant="dot" color="gray">
                                            {tag}
                                        </Badge>
                                    ))}
                                </Group>
                            )}
                        </Card>

                        {/* Course Stats */}
                        <Paper p="md" withBorder>
                            <Group justify="space-around">
                                <Stack align="center" gap={4}>
                                    <ThemeIcon variant="light" size="lg">
                                        <IconBook size={20} />
                                    </ThemeIcon>
                                    <Text size="lg" fw={500}>{lessons?.length || 0}</Text>
                                    <Text size="xs" c="dimmed">Lessons</Text>
                                </Stack>

                                <Stack align="center" gap={4}>
                                    <ThemeIcon variant="light" size="lg" color="green">
                                        <IconCheck size={20} />
                                    </ThemeIcon>
                                    <Text size="lg" fw={500}>{progressData?.completedLessons || 0}</Text>
                                    <Text size="xs" c="dimmed">Completed</Text>
                                </Stack>

                                <Stack align="center" gap={4}>
                                    <ThemeIcon variant="light" size="lg" color="orange">
                                        <IconClock size={20} />
                                    </ThemeIcon>
                                    <Text size="lg" fw={500}>{Math.round(progressPercentage)}%</Text>
                                    <Text size="xs" c="dimmed">Progress</Text>
                                </Stack>
                            </Group>
                        </Paper>
                    </Stack>
                </Grid.Col>

                {/* Lessons Sidebar */}
                <Grid.Col span={{ base: 12, md: 4 }}>
                    <Card shadow="sm" padding="lg" radius="md" withBorder>
                        <Group justify="space-between" mb="md">
                            <Title order={3}>Course Content</Title>
                            <Text size="sm" c="dimmed">
                                {lessons?.length || 0} lessons
                            </Text>
                        </Group>

                        {lessons && lessons.length > 0 && (
                            <>
                                <Progress
                                    value={progressPercentage}
                                    mb="md"
                                    color="green"
                                    size="sm"
                                />
                                <Text size="xs" c="dimmed" mb="lg" ta="center">
                                    {Math.round(progressPercentage)}% Complete
                                </Text>
                            </>
                        )}

                        <Divider mb="md" />

                        {lessons && lessons.length > 0 ? (
                            <Stack gap="xs">
                                {lessons
                                    .sort((a, b) => a.order - b.order)
                                    .map((lesson, index) => {
                                        const status = getLessonStatus(lesson, index)
                                        const isDisabled = status === 'locked'

                                        if (isDisabled) {
                                            return (
                                                <Button
                                                    key={lesson.id}
                                                    variant="subtle"
                                                    color={getLessonColor(status)}
                                                    leftSection={getLessonIcon(status)}
                                                    justify="flex-start"
                                                    fullWidth
                                                    disabled
                                                    style={{
                                                        height: 'auto',
                                                        padding: '12px 16px',
                                                        opacity: 0.5
                                                    }}
                                                >
                                                    <div style={{ textAlign: 'left', flex: 1 }}>
                                                        <Text size="sm" fw={500} lineClamp={1}>
                                                            {lesson.title}
                                                        </Text>
                                                        <Text size="xs" c="dimmed">
                                                            Lesson {lesson.order}
                                                        </Text>
                                                    </div>
                                                </Button>
                                            )
                                        }

                                        return (
                                            <Button
                                                key={lesson.id}
                                                component={Link}
                                                to={`/courses/${courseId}/lessons/${lesson.id}`}
                                                variant={status === 'completed' ? 'light' : 'subtle'}
                                                color={getLessonColor(status)}
                                                leftSection={getLessonIcon(status)}
                                                justify="flex-start"
                                                fullWidth
                                                style={{
                                                    height: 'auto',
                                                    padding: '12px 16px'
                                                }}
                                            >
                                                <div style={{ textAlign: 'left', flex: 1 }}>
                                                    <Text size="sm" fw={500} lineClamp={1}>
                                                        {lesson.title}
                                                    </Text>
                                                    <Text size="xs" c="dimmed">
                                                        Lesson {lesson.order}
                                                    </Text>
                                                </div>
                                            </Button>
                                        )
                                    })}
                            </Stack>
                        ) : (
                            <Text c="dimmed" ta="center" py="xl">
                                No lessons available yet
                            </Text>
                        )}
                    </Card>
                </Grid.Col>
            </Grid>

            {/* Edit Course Modal */}
            {course && (
                <CourseForm
                    opened={editModalOpened}
                    onClose={closeEditModal}
                    course={course}
                    mode="edit"
                />
            )}
        </Container>
    )
}

export default CourseDetailPage