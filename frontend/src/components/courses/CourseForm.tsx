import {
    Modal,
    TextInput,
    Textarea,
    Select,
    MultiSelect,
    Button,
    Group,
    Stack, Alert
} from '@mantine/core'
import { useForm } from '@mantine/form'
import { notifications } from '@mantine/notifications'
import { IconAlertCircle } from '@tabler/icons-react'
import { useCreateCourse, useUpdateCourse } from '../../hooks/api/useCourses'
import type { Course, CreateCourseRequest, UpdateCourseRequest } from '../../types/course'

interface CourseFormProps {
    opened: boolean
    onClose: () => void
    course?: Course // If provided, we're editing; otherwise creating
    mode: 'create' | 'edit'
}

const PROGRAMMING_LANGUAGES = [
    { value: 'javascript', label: 'JavaScript' },
    { value: 'typescript', label: 'TypeScript' },
    { value: 'python', label: 'Python' },
    { value: 'java', label: 'Java' },
    { value: 'csharp', label: 'C#' },
    { value: 'go', label: 'Go' },
    { value: 'rust', label: 'Rust' },
    { value: 'php', label: 'PHP' },
    { value: 'ruby', label: 'Ruby' },
    { value: 'swift', label: 'Swift' },
]

const STATUS_OPTIONS = [
    { value: 'draft', label: 'Draft' },
    { value: 'published', label: 'Published' },
    { value: 'archived', label: 'Archived' },
]

const COMMON_TAGS = [
    { value: 'beginner', label: 'Beginner' },
    { value: 'intermediate', label: 'Intermediate' },
    { value: 'advanced', label: 'Advanced' },
    { value: 'web-development', label: 'Web Development' },
    { value: 'backend', label: 'Backend' },
    { value: 'frontend', label: 'Frontend' },
    { value: 'algorithms', label: 'Algorithms' },
    { value: 'data-structures', label: 'Data Structures' },
    { value: 'api', label: 'API' },
    { value: 'database', label: 'Database' },
    { value: 'mobile', label: 'Mobile' },
    { value: 'desktop', label: 'Desktop' },
    { value: 'game-development', label: 'Game Development' },
    { value: 'machine-learning', label: 'Machine Learning' },
    { value: 'devops', label: 'DevOps' },
]

interface CreateCourseFormData {
    title: string
    slug: string
    description: string
    language: string
    tags: string[]
    featuredImage: string
    status: 'draft' | 'published' | 'archived'
}

const CourseForm = ({ opened, onClose, course, mode }: CourseFormProps) => {
    const createCourseMutation = useCreateCourse()
    const updateCourseMutation = useUpdateCourse()

    const isEditing = mode === 'edit' && course

    const form = useForm<CreateCourseFormData>({
        validate: {
            title: (value) => {
                if (!value || value.length < 3) return 'Title must be at least 3 characters'
                if (value.length > 200) return 'Title must be less than 200 characters'
                return null
            },
            slug: (value) => {
                if (!value || value.length < 3) return 'Slug must be at least 3 characters'
                if (value.length > 100) return 'Slug must be less than 100 characters'
                if (!/^[a-z0-9-]+$/.test(value)) return 'Slug must contain only lowercase letters, numbers, and hyphens'
                return null
            },
            language: (value) => !value ? 'Please select a programming language' : null,
            featuredImage: (value) => {
                if (value && value.length > 0) {
                    try {
                        new URL(value)
                        return null
                    } catch {
                        return 'Please enter a valid URL'
                    }
                }
                return null
            },
            description: (value) => {
                if (value && value.length > 1000) return 'Description must be less than 1000 characters'
                return null
            }
        },
        initialValues: isEditing ? {
            title: course.title,
            slug: course.slug,
            description: course.description || '',
            language: course.language,
            tags: course.tags,
            featuredImage: course.featuredImage || '',
            status: course.status,
        } : {
            title: '',
            slug: '',
            description: '',
            language: '',
            tags: [],
            featuredImage: '',
            status: 'draft' as const,
        },
    })

    const generateSlug = (title: string) => {
        return title
            .toLowerCase()
            .replace(/[^a-z0-9\s-]/g, '')
            .replace(/\s+/g, '-')
            .replace(/-+/g, '-')
            .trim()
    }

    const handleTitleChange = (value: string) => {
        form.setFieldValue('title', value)
        if (!isEditing && !form.values.slug) {
            form.setFieldValue('slug', generateSlug(value))
        }
    }

    const handleSubmit = async (values: CreateCourseFormData) => {
        try {
            if (isEditing && course) {
                await updateCourseMutation.mutateAsync({
                    courseId: course.id,
                    courseData: values as UpdateCourseRequest
                })
                notifications.show({
                    title: 'Success',
                    message: 'Course updated successfully',
                    color: 'green'
                })
            } else {
                await createCourseMutation.mutateAsync(values as CreateCourseRequest)
                notifications.show({
                    title: 'Success',
                    message: 'Course created successfully',
                    color: 'green'
                })
            }
            form.reset()
            onClose()
        } catch {
            notifications.show({
                title: 'Error',
                message: `Failed to ${isEditing ? 'update' : 'create'} course`,
                color: 'red'
            })
        }
    }

    const handleClose = () => {
        form.reset()
        onClose()
    }

    const isLoading = createCourseMutation.isPending || updateCourseMutation.isPending

    return (
        <Modal
            opened={opened}
            onClose={handleClose}
            title={isEditing ? 'Edit Course' : 'Create New Course'}
            size="lg"
            centered
        >
            <form onSubmit={form.onSubmit(handleSubmit)}>
                <Stack gap="md">
                    <TextInput
                        label="Course Title"
                        placeholder="Enter course title"
                        required
                        {...form.getInputProps('title')}
                        onChange={(event) => handleTitleChange(event.currentTarget.value)}
                    />

                    <TextInput
                        label="Course Slug"
                        placeholder="course-slug"
                        description="URL-friendly version of the title (lowercase, hyphens only)"
                        required
                        {...form.getInputProps('slug')}
                    />

                    <Textarea
                        label="Description"
                        placeholder="Enter course description"
                        description="Brief description of what students will learn"
                        minRows={3}
                        maxRows={6}
                        {...form.getInputProps('description')}
                    />

                    <Select
                        label="Programming Language"
                        placeholder="Select programming language"
                        data={PROGRAMMING_LANGUAGES}
                        required
                        searchable
                        {...form.getInputProps('language')}
                    />

                    <MultiSelect
                        label="Tags"
                        placeholder="Select relevant tags"
                        data={COMMON_TAGS}
                        searchable
                        {...form.getInputProps('tags')}
                    />

                    <TextInput
                        label="Featured Image URL"
                        placeholder="https://example.com/image.jpg"
                        description="URL to the course's featured image"
                        {...form.getInputProps('featuredImage')}
                    />

                    <Select
                        label="Status"
                        data={STATUS_OPTIONS}
                        required
                        {...form.getInputProps('status')}
                    />

                    {form.errors.root && (
                        <Alert icon={<IconAlertCircle size={16} />} color="red">
                            {form.errors.root}
                        </Alert>
                    )}

                    <Group justify="flex-end" mt="md">
                        <Button variant="default" onClick={handleClose} disabled={isLoading}>
                            Cancel
                        </Button>
                        <Button type="submit" loading={isLoading}>
                            {isEditing ? 'Update Course' : 'Create Course'}
                        </Button>
                    </Group>
                </Stack>
            </form>
        </Modal>
    )
}

export default CourseForm