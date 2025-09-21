import {
    Container,
    Title,
    Text,
    SimpleGrid,
    Pagination,
    Group,
    Button,
    Stack,
    Center,
    Loader,
    Alert,
    Modal,
    ActionIcon,
    Tooltip
} from '@mantine/core'
import { IconPlus, IconRefresh, IconAlertCircle } from '@tabler/icons-react'
import { useState, useMemo } from 'react'
import { useDisclosure } from '@mantine/hooks'
import { notifications } from '@mantine/notifications'
import { useCourses, useDeleteCourse } from '../../hooks/api/useCourses'
import CourseCard from '../../components/courses/CourseCard'
import CourseFilters from '../../components/courses/CourseFilters'
import CourseForm from '../../components/courses/CourseForm'
import { useAppSelector } from '../../store/hooks'
import type { CourseFilters as CourseFiltersType, Course } from '../../types/course'

const CoursesPage = () => {
    const { user } = useAppSelector((state) => state.auth)
    const isAdmin = user?.userRoles?.includes('Admin') || user?.userRoles?.includes('Instructor')

    const [filters, setFilters] = useState<CourseFiltersType>({})
    const [currentPage, setCurrentPage] = useState(1)
    const [pageSize] = useState(12)
    const [deleteModalOpened, { open: openDeleteModal, close: closeDeleteModal }] = useDisclosure(false)
    const [courseToDelete, setCourseToDelete] = useState<string | null>(null)
    const [createModalOpened, { open: openCreateModal, close: closeCreateModal }] = useDisclosure(false)
    const [editModalOpened, { open: openEditModal, close: closeEditModal }] = useDisclosure(false)
    const [courseToEdit, setCourseToEdit] = useState<Course | null>(null)

    const {
        data: coursesResponse,
        isLoading,
        error,
        refetch
    } = useCourses(filters, currentPage, pageSize)

    const deleteCourseMutation = useDeleteCourse()

    const courses = coursesResponse?.courses || []
    const totalPages = coursesResponse?.totalPages || 0
    const totalCount = coursesResponse?.totalCount || 0

    const handleFiltersChange = (newFilters: CourseFiltersType) => {
        setFilters(newFilters)
        setCurrentPage(1) // Reset to first page when filters change
    }

    const handleClearFilters = () => {
        setFilters({})
        setCurrentPage(1)
    }

    const handleEditCourse = (course: Course) => {
        setCourseToEdit(course)
        openEditModal()
    }

    const handleDeleteCourse = (courseId: string) => {
        setCourseToDelete(courseId)
        openDeleteModal()
    }

    const confirmDeleteCourse = async () => {
        if (!courseToDelete) return

        try {
            await deleteCourseMutation.mutateAsync(courseToDelete)
            notifications.show({
                title: 'Success',
                message: 'Course deleted successfully',
                color: 'green'
            })
            closeDeleteModal()
            setCourseToDelete(null)
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to delete course',
                color: 'red'
            })
        }
    }

    const handleCreateCourse = () => {
        openCreateModal()
    }

    const handleCloseEditModal = () => {
        setCourseToEdit(null)
        closeEditModal()
    }

    const courseToDeleteData = useMemo(() => {
        return courses.find(course => course.id === courseToDelete)
    }, [courses, courseToDelete])

    if (error) {
        return (
            <Container size="xl" py="xl">
                <Alert
                    icon={<IconAlertCircle size={16} />}
                    title="Error loading courses"
                    color="red"
                    mb="xl"
                >
                    {error instanceof Error ? error.message : 'An unexpected error occurred'}
                </Alert>
                <Center>
                    <Button leftSection={<IconRefresh size={16} />} onClick={() => refetch()}>
                        Try Again
                    </Button>
                </Center>
            </Container>
        )
    }

    return (
        <Container size="xl" py="xl">
            <Stack gap="xl">
                {/* Header */}
                <Group justify="space-between" align="center">
                    <div>
                        <Title order={1}>Courses</Title>
                        <Text c="dimmed" size="sm">
                            {totalCount > 0 ? `${totalCount} course${totalCount !== 1 ? 's' : ''} available` : 'No courses found'}
                        </Text>
                    </div>

                    <Group>
                        <Tooltip label="Refresh courses">
                            <ActionIcon
                                variant="subtle"
                                onClick={() => refetch()}
                                loading={isLoading}
                            >
                                <IconRefresh size={16} />
                            </ActionIcon>
                        </Tooltip>

                        {isAdmin && (
                            <Button
                                leftSection={<IconPlus size={16} />}
                                onClick={handleCreateCourse}
                            >
                                Create Course
                            </Button>
                        )}
                    </Group>
                </Group>

                {/* Filters */}
                <CourseFilters
                    filters={filters}
                    onFiltersChange={handleFiltersChange}
                    onClearFilters={handleClearFilters}
                />

                {/* Loading State */}
                {isLoading && (
                    <Center py="xl">
                        <Loader size="lg" />
                    </Center>
                )}

                {/* Courses Grid */}
                {!isLoading && courses.length > 0 && (
                    <>
                        <SimpleGrid
                            cols={{ base: 1, sm: 2, md: 3, lg: 4 }}
                            spacing="lg"
                        >
                            {courses.map((course) => (
                                <CourseCard
                                    key={course.id}
                                    course={course}
                                    onEdit={handleEditCourse}
                                    onDelete={handleDeleteCourse}
                                    showActions={isAdmin}
                                />
                            ))}
                        </SimpleGrid>

                        {/* Pagination */}
                        {totalPages > 1 && (
                            <Center>
                                <Pagination
                                    value={currentPage}
                                    onChange={setCurrentPage}
                                    total={totalPages}
                                    size="md"
                                />
                            </Center>
                        )}
                    </>
                )}

                {/* Empty State */}
                {!isLoading && courses.length === 0 && (
                    <Center py="xl">
                        <Stack align="center" gap="md">
                            <Text size="lg" c="dimmed">No courses found</Text>
                            <Text size="sm" c="dimmed" ta="center">
                                {Object.keys(filters).length > 0
                                    ? 'Try adjusting your filters to see more results'
                                    : 'There are no courses available at the moment'
                                }
                            </Text>
                            {Object.keys(filters).length > 0 && (
                                <Button variant="light" onClick={handleClearFilters}>
                                    Clear Filters
                                </Button>
                            )}
                        </Stack>
                    </Center>
                )}
            </Stack>

            {/* Delete Confirmation Modal */}
            <Modal
                opened={deleteModalOpened}
                onClose={closeDeleteModal}
                title="Delete Course"
                centered
            >
                <Stack gap="md">
                    <Text>
                        Are you sure you want to delete "{courseToDeleteData?.title}"?
                        This action cannot be undone.
                    </Text>
                    <Group justify="flex-end">
                        <Button variant="default" onClick={closeDeleteModal}>
                            Cancel
                        </Button>
                        <Button
                            color="red"
                            onClick={confirmDeleteCourse}
                            loading={deleteCourseMutation.isPending}
                        >
                            Delete Course
                        </Button>
                    </Group>
                </Stack>
            </Modal>

            {/* Create Course Modal */}
            <CourseForm
                opened={createModalOpened}
                onClose={closeCreateModal}
                mode="create"
            />

            {/* Edit Course Modal */}
            <CourseForm
                opened={editModalOpened}
                onClose={handleCloseEditModal}
                course={courseToEdit || undefined}
                mode="edit"
            />
        </Container>
    )
}

export default CoursesPage