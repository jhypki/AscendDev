import {
    Card,
    Image,
    Text,
    Badge,
    Button,
    Group,
    Stack,
    ActionIcon,
    Menu,
    rem
} from '@mantine/core'
import { IconDots, IconEdit, IconTrash, IconEye } from '@tabler/icons-react'
import { Link } from 'react-router-dom'
import type { Course } from '../../types/course'
import { useAppSelector } from '../../store/hooks'

interface CourseCardProps {
    course: Course
    onEdit?: (course: Course) => void
    onDelete?: (courseId: string) => void
    showActions?: boolean
}

const CourseCard = ({ course, onEdit, onDelete, showActions = false }: CourseCardProps) => {
    const { user } = useAppSelector((state) => state.auth)
    const isAdmin = user?.userRoles?.includes('Admin') || user?.userRoles?.includes('Instructor')

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
            month: 'short',
            day: 'numeric'
        })
    }

    return (
        <Card shadow="sm" padding="lg" radius="md" withBorder h="100%">
            <Card.Section>
                <Image
                    src={course.featuredImage || '/api/placeholder/400/200'}
                    height={200}
                    alt={course.title}
                    fallbackSrc="https://via.placeholder.com/400x200?text=No+Image"
                />
            </Card.Section>

            <Stack gap="sm" mt="md" mb="xs" style={{ flex: 1 }}>
                <Group justify="space-between" align="flex-start">
                    <Text fw={500} size="lg" lineClamp={2} style={{ flex: 1 }}>
                        {course.title}
                    </Text>

                    {(showActions && isAdmin) && (
                        <Menu shadow="md" width={200}>
                            <Menu.Target>
                                <ActionIcon variant="subtle" color="gray">
                                    <IconDots style={{ width: rem(16), height: rem(16) }} />
                                </ActionIcon>
                            </Menu.Target>

                            <Menu.Dropdown>
                                <Menu.Item
                                    leftSection={<IconEye style={{ width: rem(14), height: rem(14) }} />}
                                    component={Link}
                                    to={`/courses/${course.id}`}
                                >
                                    View Course
                                </Menu.Item>
                                <Menu.Item
                                    leftSection={<IconEdit style={{ width: rem(14), height: rem(14) }} />}
                                    onClick={() => onEdit?.(course)}
                                >
                                    Edit Course
                                </Menu.Item>
                                <Menu.Divider />
                                <Menu.Item
                                    color="red"
                                    leftSection={<IconTrash style={{ width: rem(14), height: rem(14) }} />}
                                    onClick={() => onDelete?.(course.id)}
                                >
                                    Delete Course
                                </Menu.Item>
                            </Menu.Dropdown>
                        </Menu>
                    )}
                </Group>

                <Text size="sm" c="dimmed" lineClamp={3} style={{ flex: 1 }}>
                    {course.description || 'No description available'}
                </Text>

                <Group gap="xs">
                    <Badge color={getStatusColor(course.status)} variant="light" size="sm">
                        {course.status.charAt(0).toUpperCase() + course.status.slice(1)}
                    </Badge>
                    <Badge color="blue" variant="outline" size="sm">
                        {course.language}
                    </Badge>
                    {course.lessonSummaries.length > 0 && (
                        <Badge color="gray" variant="outline" size="sm">
                            {course.lessonSummaries.length} lesson{course.lessonSummaries.length !== 1 ? 's' : ''}
                        </Badge>
                    )}
                </Group>

                {course.tags.length > 0 && (
                    <Group gap={4}>
                        {course.tags.slice(0, 3).map((tag) => (
                            <Badge key={tag} size="xs" variant="dot" color="gray">
                                {tag}
                            </Badge>
                        ))}
                        {course.tags.length > 3 && (
                            <Text size="xs" c="dimmed">
                                +{course.tags.length - 3} more
                            </Text>
                        )}
                    </Group>
                )}

                <Text size="xs" c="dimmed">
                    {course.updatedAt ? `Updated ${formatDate(course.updatedAt)}` : `Created ${formatDate(course.createdAt)}`}
                </Text>
            </Stack>

            <Button
                component={Link}
                to={`/courses/${course.id}`}
                variant="light"
                color="blue"
                fullWidth
                mt="md"
                radius="md"
            >
                View Course
            </Button>
        </Card>
    )
}

export default CourseCard