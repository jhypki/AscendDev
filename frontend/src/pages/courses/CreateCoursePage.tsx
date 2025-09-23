import {
    Container,
    Title,
    Text,
    Paper,
    Stack,
    Group,
    Button,
    TextInput,
    Textarea,
    Select,
    MultiSelect,
    Card,
    Badge,
    Grid,
    Box,
    Alert,
    Stepper,
    ActionIcon, Divider, ThemeIcon,
    rem
} from '@mantine/core'
import { useForm } from '@mantine/form'
import { notifications } from '@mantine/notifications'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
    IconBook,
    IconCode, IconPhoto,
    IconCheck,
    IconArrowLeft,
    IconInfoCircle,
    IconSparkles,
    IconTarget,
    IconUsers,
    IconClock
} from '@tabler/icons-react'
import { useCreateCourse } from '../../hooks/api/useCourses'
import type { CreateCourseRequest } from '../../types/course'

const PROGRAMMING_LANGUAGES = [
    { value: 'javascript', label: 'JavaScript', color: '#F7DF1E' },
    { value: 'typescript', label: 'TypeScript', color: '#3178C6' },
    { value: 'python', label: 'Python', color: '#3776AB' },
    { value: 'java', label: 'Java', color: '#ED8B00' },
    { value: 'csharp', label: 'C#', color: '#239120' },
    { value: 'go', label: 'Go', color: '#00ADD8' },
    { value: 'rust', label: 'Rust', color: '#000000' },
    { value: 'php', label: 'PHP', color: '#777BB4' },
    { value: 'ruby', label: 'Ruby', color: '#CC342D' },
    { value: 'swift', label: 'Swift', color: '#FA7343' },
]

const STATUS_OPTIONS = [
    { value: 'draft', label: 'Draft', description: 'Course is not visible to students' },
    { value: 'published', label: 'Published', description: 'Course is live and accessible' },
]

const COMMON_TAGS = [
    { value: 'beginner', label: 'Beginner', color: 'green' },
    { value: 'intermediate', label: 'Intermediate', color: 'yellow' },
    { value: 'advanced', label: 'Advanced', color: 'red' },
    { value: 'web-development', label: 'Web Development', color: 'blue' },
    { value: 'backend', label: 'Backend', color: 'violet' },
    { value: 'frontend', label: 'Frontend', color: 'cyan' },
    { value: 'algorithms', label: 'Algorithms', color: 'orange' },
    { value: 'data-structures', label: 'Data Structures', color: 'pink' },
    { value: 'api', label: 'API', color: 'teal' },
    { value: 'database', label: 'Database', color: 'indigo' },
    { value: 'mobile', label: 'Mobile', color: 'lime' },
    { value: 'desktop', label: 'Desktop', color: 'grape' },
    { value: 'game-development', label: 'Game Development', color: 'red' },
    { value: 'machine-learning', label: 'Machine Learning', color: 'violet' },
    { value: 'devops', label: 'DevOps', color: 'orange' },
]

interface CreateCourseFormData {
    title: string
    slug: string
    description: string
    language: string
    tags: string[]
    featuredImage: string | undefined
    status: 'draft' | 'published'
}

const CreateCoursePage = () => {
    const navigate = useNavigate()
    const createCourseMutation = useCreateCourse()
    const [activeStep, setActiveStep] = useState(0)

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
        initialValues: {
            title: '',
            slug: '',
            description: '',
            language: '',
            tags: [],
            featuredImage: undefined,
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
        if (!form.values.slug) {
            form.setFieldValue('slug', generateSlug(value))
        }
    }

    const handleSubmit = async (values: CreateCourseFormData) => {
        try {
            const course = await createCourseMutation.mutateAsync(values as CreateCourseRequest)
            notifications.show({
                title: 'Success!',
                message: 'Course created successfully',
                color: 'green',
                icon: <IconCheck size={16} />
            })
            navigate(`/courses/${course.id}`)
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to create course',
                color: 'red'
            })
        }
    }

    const nextStep = () => {
        if (activeStep === 0) {
            // Validate only the fields required for step 1
            const step1Validation = form.validate()
            const requiredFields = ['title', 'slug']
            const hasRequiredErrors = requiredFields.some(field => step1Validation.errors[field])

            if (hasRequiredErrors) {
                console.log('Validation errors:', step1Validation.errors)
                return
            }
        }
        if (activeStep === 1) {
            // Validate fields required for step 2
            const step2Validation = form.validate()
            const requiredFields = ['language']
            const hasRequiredErrors = requiredFields.some(field => step2Validation.errors[field])

            if (hasRequiredErrors) {
                console.log('Validation errors:', step2Validation.errors)
                return
            }
        }
        setActiveStep((current) => (current < 2 ? current + 1 : current))
    }

    const prevStep = () => setActiveStep((current) => (current > 0 ? current - 1 : current))

    const selectedLanguage = PROGRAMMING_LANGUAGES.find(lang => lang.value === form.values.language)

    return (
        <Container size="md" py="xl">
            <Stack gap="xl">
                {/* Header */}
                <Group>
                    <ActionIcon
                        variant="subtle"
                        size="lg"
                        onClick={() => navigate('/courses')}
                    >
                        <IconArrowLeft size={20} />
                    </ActionIcon>
                    <div>
                        <Title order={1} size="h2">
                            Create New Course
                        </Title>
                        <Text c="dimmed" size="sm">
                            Build an engaging programming course for your students
                        </Text>
                    </div>
                </Group>

                {/* Progress Stepper */}
                <Paper p="md" radius="md" withBorder>
                    <Stepper active={activeStep} onStepClick={setActiveStep} allowNextStepsSelect={false}>
                        <Stepper.Step
                            label="Basic Information"
                            description="Course title and description"
                            icon={<IconBook size={18} />}
                        />
                        <Stepper.Step
                            label="Configuration"
                            description="Language and settings"
                            icon={<IconCode size={18} />}
                        />
                        <Stepper.Step
                            label="Finalize"
                            description="Review and publish"
                            icon={<IconSparkles size={18} />}
                        />
                    </Stepper>
                </Paper>

                <form onSubmit={form.onSubmit(handleSubmit)}>
                    {/* Step 1: Basic Information */}
                    {activeStep === 0 && (
                        <Paper p="xl" radius="md" withBorder>
                            <Stack gap="lg">
                                <Group>
                                    <ThemeIcon size="lg" radius="md" color="blue">
                                        <IconTarget size={20} />
                                    </ThemeIcon>
                                    <div>
                                        <Title order={3}>Basic Information</Title>
                                        <Text c="dimmed" size="sm">
                                            Let's start with the fundamentals of your course
                                        </Text>
                                    </div>
                                </Group>

                                <Divider />

                                <TextInput
                                    label="Course Title"
                                    placeholder="e.g., Introduction to JavaScript"
                                    description="A clear, descriptive title that tells students what they'll learn"
                                    required
                                    size="md"
                                    {...form.getInputProps('title')}
                                    onChange={(event) => handleTitleChange(event.currentTarget.value)}
                                />

                                <TextInput
                                    label="Course Slug"
                                    placeholder="introduction-to-javascript"
                                    description="URL-friendly version of the title (automatically generated)"
                                    required
                                    size="md"
                                    {...form.getInputProps('slug')}
                                />

                                <Textarea
                                    label="Course Description"
                                    placeholder="Describe what students will learn in this course..."
                                    description="Help students understand what they'll gain from taking this course"
                                    minRows={4}
                                    maxRows={8}
                                    size="md"
                                    {...form.getInputProps('description')}
                                />

                                <Alert icon={<IconInfoCircle size={16} />} color="blue" variant="light">
                                    <Text size="sm">
                                        <strong>Tip:</strong> A good course description includes learning objectives,
                                        prerequisites, and what students will be able to do after completion.
                                    </Text>
                                </Alert>
                            </Stack>
                        </Paper>
                    )}

                    {/* Step 2: Configuration */}
                    {activeStep === 1 && (
                        <Paper p="xl" radius="md" withBorder>
                            <Stack gap="lg">
                                <Group>
                                    <ThemeIcon size="lg" radius="md" color="violet">
                                        <IconCode size={20} />
                                    </ThemeIcon>
                                    <div>
                                        <Title order={3}>Course Configuration</Title>
                                        <Text c="dimmed" size="sm">
                                            Set up the technical details and categorization
                                        </Text>
                                    </div>
                                </Group>

                                <Divider />

                                <Grid>
                                    <Grid.Col span={{ base: 12, md: 6 }}>
                                        <Select
                                            label="Programming Language"
                                            placeholder="Select the main language"
                                            description="Primary programming language for this course"
                                            data={PROGRAMMING_LANGUAGES}
                                            required
                                            searchable
                                            size="md"
                                            {...form.getInputProps('language')}
                                        />
                                    </Grid.Col>
                                    <Grid.Col span={{ base: 12, md: 6 }}>
                                        <Select
                                            label="Course Status"
                                            description="Control course visibility"
                                            data={STATUS_OPTIONS}
                                            required
                                            size="md"
                                            {...form.getInputProps('status')}
                                        />
                                    </Grid.Col>
                                </Grid>

                                {selectedLanguage && (
                                    <Card withBorder radius="md" p="md">
                                        <Group>
                                            <Box
                                                w={40}
                                                h={40}
                                                style={{
                                                    backgroundColor: selectedLanguage.color,
                                                    borderRadius: rem(8),
                                                    display: 'flex',
                                                    alignItems: 'center',
                                                    justifyContent: 'center'
                                                }}
                                            >
                                                <IconCode size={20} color="white" />
                                            </Box>
                                            <div>
                                                <Text fw={500}>{selectedLanguage.label}</Text>
                                                <Text size="sm" c="dimmed">Selected programming language</Text>
                                            </div>
                                        </Group>
                                    </Card>
                                )}

                                <MultiSelect
                                    label="Course Tags"
                                    placeholder="Select relevant tags"
                                    description="Help students find your course with relevant tags"
                                    data={COMMON_TAGS}
                                    searchable
                                    size="md"
                                    {...form.getInputProps('tags')}
                                />

                                {form.values.tags.length > 0 && (
                                    <Card withBorder radius="md" p="md">
                                        <Text size="sm" fw={500} mb="xs">Selected Tags:</Text>
                                        <Group gap="xs">
                                            {form.values.tags.map((tag) => {
                                                const tagData = COMMON_TAGS.find(t => t.value === tag)
                                                return (
                                                    <Badge
                                                        key={tag}
                                                        color={tagData?.color || 'gray'}
                                                        variant="light"
                                                    >
                                                        {tagData?.label || tag}
                                                    </Badge>
                                                )
                                            })}
                                        </Group>
                                    </Card>
                                )}

                                <TextInput
                                    label="Featured Image URL"
                                    placeholder="https://example.com/course-image.jpg"
                                    description="Optional: Add a featured image for your course"
                                    size="md"
                                    leftSection={<IconPhoto size={16} />}
                                    {...form.getInputProps('featuredImage')}
                                />
                            </Stack>
                        </Paper>
                    )}

                    {/* Step 3: Review */}
                    {activeStep === 2 && (
                        <Paper p="xl" radius="md" withBorder>
                            <Stack gap="lg">
                                <Group>
                                    <ThemeIcon size="lg" radius="md" color="green">
                                        <IconCheck size={20} />
                                    </ThemeIcon>
                                    <div>
                                        <Title order={3}>Review & Create</Title>
                                        <Text c="dimmed" size="sm">
                                            Review your course details before creating
                                        </Text>
                                    </div>
                                </Group>

                                <Divider />

                                <Grid>
                                    <Grid.Col span={{ base: 12, md: 8 }}>
                                        <Card withBorder radius="md" p="lg">
                                            <Stack gap="md">
                                                <Group>
                                                    <IconBook size={20} color="var(--mantine-color-blue-6)" />
                                                    <Text fw={600} size="lg">{form.values.title || 'Untitled Course'}</Text>
                                                </Group>

                                                {form.values.description && (
                                                    <Text c="dimmed" size="sm">
                                                        {form.values.description}
                                                    </Text>
                                                )}

                                                <Group gap="xs">
                                                    {selectedLanguage && (
                                                        <Badge color="blue" variant="light">
                                                            {selectedLanguage.label}
                                                        </Badge>
                                                    )}
                                                    <Badge color={form.values.status === 'published' ? 'green' : 'yellow'} variant="light">
                                                        {form.values.status === 'published' ? 'Published' : 'Draft'}
                                                    </Badge>
                                                </Group>

                                                {form.values.tags.length > 0 && (
                                                    <Group gap="xs">
                                                        {form.values.tags.map((tag) => {
                                                            const tagData = COMMON_TAGS.find(t => t.value === tag)
                                                            return (
                                                                <Badge
                                                                    key={tag}
                                                                    size="sm"
                                                                    color={tagData?.color || 'gray'}
                                                                    variant="outline"
                                                                >
                                                                    {tagData?.label || tag}
                                                                </Badge>
                                                            )
                                                        })}
                                                    </Group>
                                                )}
                                            </Stack>
                                        </Card>
                                    </Grid.Col>

                                    <Grid.Col span={{ base: 12, md: 4 }}>
                                        <Stack gap="md">
                                            <Card withBorder radius="md" p="md">
                                                <Group>
                                                    <ThemeIcon size="sm" color="blue" variant="light">
                                                        <IconUsers size={14} />
                                                    </ThemeIcon>
                                                    <div>
                                                        <Text size="sm" fw={500}>Ready for Students</Text>
                                                        <Text size="xs" c="dimmed">
                                                            {form.values.status === 'published' ? 'Visible to all' : 'Draft mode'}
                                                        </Text>
                                                    </div>
                                                </Group>
                                            </Card>

                                            <Card withBorder radius="md" p="md">
                                                <Group>
                                                    <ThemeIcon size="sm" color="green" variant="light">
                                                        <IconClock size={14} />
                                                    </ThemeIcon>
                                                    <div>
                                                        <Text size="sm" fw={500}>Next Steps</Text>
                                                        <Text size="xs" c="dimmed">Add lessons after creation</Text>
                                                    </div>
                                                </Group>
                                            </Card>
                                        </Stack>
                                    </Grid.Col>
                                </Grid>

                                <Alert icon={<IconInfoCircle size={16} />} color="blue" variant="light">
                                    <Text size="sm">
                                        After creating your course, you'll be able to add lessons, manage content,
                                        and track student progress from the course dashboard.
                                    </Text>
                                </Alert>
                            </Stack>
                        </Paper>
                    )}

                    {/* Navigation */}
                    <Group justify="space-between">
                        <Button
                            variant="default"
                            onClick={prevStep}
                            disabled={activeStep === 0}
                        >
                            Previous
                        </Button>

                        <Group>
                            <Button
                                variant="default"
                                onClick={() => navigate('/courses')}
                            >
                                Cancel
                            </Button>

                            {activeStep < 2 ? (
                                <Button onClick={nextStep}>
                                    Next Step
                                </Button>
                            ) : (
                                <Button
                                    type="submit"
                                    loading={createCourseMutation.isPending}
                                    leftSection={<IconCheck size={16} />}
                                >
                                    Create Course
                                </Button>
                            )}
                        </Group>
                    </Group>
                </form>
            </Stack>
        </Container>
    )
}

export default CreateCoursePage