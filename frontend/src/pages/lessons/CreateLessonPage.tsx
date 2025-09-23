
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
    ActionIcon,
    Divider,
    Center,
    ThemeIcon,
    NumberInput,
    JsonInput,
    Tabs,
    Code,
    ScrollArea,
    Switch
} from '@mantine/core'
import { useForm } from '@mantine/form'
import { notifications } from '@mantine/notifications'
import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import {
    IconBook,
    IconCode, IconCheck,
    IconArrowLeft,
    IconInfoCircle,
    IconSparkles,
    IconTarget,
    IconFlask,
    IconFileText,
    IconBulb,
    IconPlaylist,
    IconTestPipe,
    IconLink,
    IconX
} from '@tabler/icons-react'
import { useCourse } from '../../hooks/api/useCourses'
import type { AdditionalResource, TestCase } from '../../types/course'

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

const RESOURCE_TYPES = [
    { value: 'documentation', label: 'Documentation', icon: IconFileText },
    { value: 'video', label: 'Video', icon: IconPlaylist },
    { value: 'article', label: 'Article', icon: IconBook },
    { value: 'example', label: 'Example', icon: IconCode },
]

const COMMON_TAGS = [
    { value: 'beginner', label: 'Beginner', color: 'green' },
    { value: 'intermediate', label: 'Intermediate', color: 'yellow' },
    { value: 'advanced', label: 'Advanced', color: 'red' },
    { value: 'practice', label: 'Practice', color: 'blue' },
    { value: 'theory', label: 'Theory', color: 'violet' },
    { value: 'hands-on', label: 'Hands-on', color: 'orange' },
    { value: 'quiz', label: 'Quiz', color: 'pink' },
    { value: 'project', label: 'Project', color: 'teal' },
]

interface CreateLessonFormData {
    title: string
    slug: string
    content: string
    template: string
    language: string
    order: number
    tags: string[]
    mainFunction: string
    testCases: TestCase[]
    additionalResources: AdditionalResource[]
}

const CreateLessonPage = () => {
    const navigate = useNavigate()
    const { courseId } = useParams<{ courseId: string }>()
    const [activeStep, setActiveStep] = useState(0)
    const [activeTab, setActiveTab] = useState<string>('content')

    const { data: course, isLoading: courseLoading } = useCourse(courseId || '', !!courseId)

    const form = useForm<CreateLessonFormData>({
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
            content: (value) => !value ? 'Lesson content is required' : null,
            template: (value) => !value ? 'Code template is required' : null,
            language: (value) => !value ? 'Please select a programming language' : null,
            order: (value) => value < 1 ? 'Order must be at least 1' : null,
            mainFunction: (value) => !value ? 'Main function name is required' : null,
        },
        initialValues: {
            title: '',
            slug: '',
            content: '',
            template: '',
            language: course?.language || '',
            order: 1,
            tags: [],
            mainFunction: '',
            testCases: [],
            additionalResources: [],
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

    const addTestCase = () => {
        const newTestCase: TestCase = {
            id: `test-${Date.now()}`,
            name: `Test Case ${form.values.testCases.length + 1}`,
            input: '',
            expectedOutput: '',
            description: '',
            testCode: '',
            isHidden: false
        }
        form.setFieldValue('testCases', [...form.values.testCases, newTestCase])
    }

    const removeTestCase = (index: number) => {
        const newTestCases = form.values.testCases.filter((_, i) => i !== index)
        form.setFieldValue('testCases', newTestCases)
    }

    const addResource = () => {
        const newResource: AdditionalResource = {
            title: '',
            url: '',
            type: 'documentation'
        }
        form.setFieldValue('additionalResources', [...form.values.additionalResources, newResource])
    }

    const removeResource = (index: number) => {
        const newResources = form.values.additionalResources.filter((_, i) => i !== index)
        form.setFieldValue('additionalResources', newResources)
    }

    const generateTestTemplate = (language: string, functionName: string, testName: string) => {
        switch (language) {
            case 'javascript':
            case 'typescript':
                return `test('${testName}', () => {
    // Arrange
    const input = /* your input here */;
    const expected = /* expected output here */;
    
    // Act
    const result = ${functionName}(input);
    
    // Assert
    expect(result).toBe(expected);
});`
            case 'python':
                return `def test_${testName.toLowerCase().replace(/\s+/g, '_')}():
    # Arrange
    input_value = # your input here
    expected = # expected output here
    
    # Act
    result = ${functionName}(input_value)
    
    # Assert
    assert result == expected`
            case 'java':
                return `@Test
public void test${testName.replace(/\s+/g, '')}() {
    // Arrange
    Object input = /* your input here */;
    Object expected = /* expected output here */;
    
    // Act
    Object result = ${functionName}(input);
    
    // Assert
    assertEquals(expected, result);
}`
            case 'csharp':
                return `[Test]
public void Test${testName.replace(/\s+/g, '')}()
{
    // Arrange
    var input = /* your input here */;
    var expected = /* expected output here */;
    
    // Act
    var result = ${functionName}(input);
    
    // Assert
    Assert.AreEqual(expected, result);
}`
            default:
                return `// Write your unit test here for: ${testName}
// Function to test: ${functionName}
// Input: /* specify input */
// Expected: /* specify expected output */`
        }
    }

    const handleSubmit = async (values: CreateLessonFormData) => {
        try {
            // Here you would call the API to create the lesson
            // const lesson = await createLessonMutation.mutateAsync({ courseId, lessonData: values })
            console.log('Creating lesson with data:', values)
            notifications.show({
                title: 'Success!',
                message: 'Lesson created successfully',
                color: 'green',
                icon: <IconCheck size={16} />
            })
            navigate(`/courses/${courseId}`)
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to create lesson',
                color: 'red'
            })
        }
    }

    const nextStep = () => {
        if (activeStep === 0) {
            // Validate only the fields required for step 1
            const step1Validation = form.validate()
            const requiredFields = ['title', 'slug', 'content', 'language']
            const hasRequiredErrors = requiredFields.some(field => step1Validation.errors[field])

            if (hasRequiredErrors) {
                console.log('Step 1 validation errors:', step1Validation.errors)
                return
            }
        }
        if (activeStep === 1) {
            // Validate fields required for step 2
            const step2Validation = form.validate()
            const requiredFields = ['template', 'mainFunction']
            const hasRequiredErrors = requiredFields.some(field => step2Validation.errors[field])

            if (hasRequiredErrors) {
                console.log('Step 2 validation errors:', step2Validation.errors)
                return
            }
        }
        setActiveStep((current) => (current < 2 ? current + 1 : current))
    }

    const prevStep = () => setActiveStep((current) => (current > 0 ? current - 1 : current))

    const selectedLanguage = PROGRAMMING_LANGUAGES.find(lang => lang.value === form.values.language)

    if (courseLoading) {
        return (
            <Container size="md" py="xl">
                <Center>
                    <Text>Loading course...</Text>
                </Center>
            </Container>
        )
    }

    if (!course) {
        return (
            <Container size="md" py="xl">
                <Alert color="red" title="Course not found">
                    The course you're trying to add a lesson to could not be found.
                </Alert>
            </Container>
        )
    }

    return (
        <Container size="lg" py="xl">
            <Stack gap="xl">
                {/* Header */}
                <Group>
                    <ActionIcon
                        variant="subtle"
                        size="lg"
                        onClick={() => navigate(`/courses/${courseId}`)}
                    >
                        <IconArrowLeft size={20} />
                    </ActionIcon>
                    <div>
                        <Title order={1} size="h2">
                            Create New Lesson
                        </Title>
                        <Text c="dimmed" size="sm">
                            Adding lesson to: <strong>{course.title}</strong>
                        </Text>
                    </div>
                </Group>

                {/* Progress Stepper */}
                <Paper p="md" radius="md" withBorder>
                    <Stepper active={activeStep} onStepClick={setActiveStep} allowNextStepsSelect={false}>
                        <Stepper.Step
                            label="Basic Information"
                            description="Title and content"
                            icon={<IconBook size={18} />}
                        />
                        <Stepper.Step
                            label="Code & Tests"
                            description="Template and test cases"
                            icon={<IconCode size={18} />}
                        />
                        <Stepper.Step
                            label="Finalize"
                            description="Review and create"
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
                                            Set up the lesson fundamentals
                                        </Text>
                                    </div>
                                </Group>

                                <Divider />

                                <Grid>
                                    <Grid.Col span={{ base: 12, md: 8 }}>
                                        <TextInput
                                            label="Lesson Title"
                                            placeholder="e.g., Variables and Data Types"
                                            description="A clear title that describes what students will learn"
                                            required
                                            size="md"
                                            {...form.getInputProps('title')}
                                            onChange={(event) => handleTitleChange(event.currentTarget.value)}
                                        />
                                    </Grid.Col>
                                    <Grid.Col span={{ base: 12, md: 4 }}>
                                        <NumberInput
                                            label="Lesson Order"
                                            description="Position in course"
                                            required
                                            min={1}
                                            size="md"
                                            {...form.getInputProps('order')}
                                        />
                                    </Grid.Col>
                                </Grid>

                                <TextInput
                                    label="Lesson Slug"
                                    placeholder="variables-and-data-types"
                                    description="URL-friendly version (automatically generated)"
                                    required
                                    size="md"
                                    {...form.getInputProps('slug')}
                                />

                                <Tabs value={activeTab} onChange={(value) => setActiveTab(value || 'content')}>
                                    <Tabs.List>
                                        <Tabs.Tab value="content" leftSection={<IconFileText size={16} />}>
                                            Content
                                        </Tabs.Tab>
                                        <Tabs.Tab value="preview" leftSection={<IconBulb size={16} />}>
                                            Preview
                                        </Tabs.Tab>
                                    </Tabs.List>

                                    <Tabs.Panel value="content" pt="md">
                                        <Textarea
                                            label="Lesson Content"
                                            placeholder="Write your lesson content in Markdown..."
                                            description="Use Markdown to format your lesson content"
                                            required
                                            minRows={10}
                                            maxRows={20}
                                            size="md"
                                            {...form.getInputProps('content')}
                                        />
                                    </Tabs.Panel>

                                    <Tabs.Panel value="preview" pt="md">
                                        <Paper withBorder p="md" radius="md" mih={200}>
                                            {form.values.content ? (
                                                <ScrollArea h={300}>
                                                    <Text size="sm" style={{ whiteSpace: 'pre-wrap' }}>
                                                        {form.values.content}
                                                    </Text>
                                                </ScrollArea>
                                            ) : (
                                                <Center h={200}>
                                                    <Text c="dimmed">Content preview will appear here</Text>
                                                </Center>
                                            )}
                                        </Paper>
                                    </Tabs.Panel>
                                </Tabs>

                                <Grid>
                                    <Grid.Col span={{ base: 12, md: 6 }}>
                                        <Select
                                            label="Programming Language"
                                            placeholder="Select language"
                                            description="Language for code examples"
                                            data={PROGRAMMING_LANGUAGES}
                                            required
                                            searchable
                                            size="md"
                                            {...form.getInputProps('language')}
                                        />
                                    </Grid.Col>
                                    <Grid.Col span={{ base: 12, md: 6 }}>
                                        <MultiSelect
                                            label="Lesson Tags"
                                            placeholder="Select tags"
                                            description="Categorize this lesson"
                                            data={COMMON_TAGS}
                                            searchable
                                            size="md"
                                            {...form.getInputProps('tags')}
                                        />
                                    </Grid.Col>
                                </Grid>

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
                            </Stack>
                        </Paper>
                    )}

                    {/* Step 2: Code & Tests */}
                    {activeStep === 1 && (
                        <Paper p="xl" radius="md" withBorder>
                            <Stack gap="lg">
                                <Group>
                                    <ThemeIcon size="lg" radius="md" color="violet">
                                        <IconCode size={20} />
                                    </ThemeIcon>
                                    <div>
                                        <Title order={3}>Code Template & Tests</Title>
                                        <Text c="dimmed" size="sm">
                                            Set up the coding exercise and test cases
                                        </Text>
                                    </div>
                                </Group>

                                <Divider />

                                <TextInput
                                    label="Main Function Name"
                                    placeholder="e.g., calculateSum"
                                    description="The main function students need to implement"
                                    required
                                    size="md"
                                    {...form.getInputProps('mainFunction')}
                                />

                                <Textarea
                                    label="Code Template"
                                    placeholder="// Write your starter code template here..."
                                    description="Starting code that students will see"
                                    required
                                    minRows={8}
                                    maxRows={15}
                                    size="md"
                                    {...form.getInputProps('template')}
                                />

                                {/* Test Cases */}
                                <div>
                                    <Group justify="space-between" mb="md">
                                        <div>
                                            <Text fw={500}>Test Cases</Text>
                                            <Text size="sm" c="dimmed">
                                                Define test cases to validate student solutions
                                            </Text>
                                        </div>
                                        <Button
                                            leftSection={<IconTestPipe size={16} />}
                                            variant="light"
                                            onClick={addTestCase}
                                        >
                                            Add Test Case
                                        </Button>
                                    </Group>

                                    <Stack gap="md">
                                        {form.values.testCases.map((testCase, index) => (
                                            <Card key={testCase.id} withBorder p="md">
                                                <Stack gap="sm">
                                                    <Group justify="space-between">
                                                        <Group>
                                                            <Text fw={500} size="sm">Test Case {index + 1}</Text>
                                                            {testCase.isHidden && (
                                                                <Badge size="xs" color="orange" variant="light">
                                                                    Hidden
                                                                </Badge>
                                                            )}
                                                        </Group>
                                                        <Group>
                                                            <ActionIcon
                                                                variant="subtle"
                                                                onClick={() => {
                                                                    const newTestCases = [...form.values.testCases]
                                                                    if (!newTestCases[index].testCode) {
                                                                        newTestCases[index].testCode = generateTestTemplate(
                                                                            form.values.language,
                                                                            form.values.mainFunction,
                                                                            testCase.name
                                                                        )
                                                                    }
                                                                    form.setFieldValue('testCases', newTestCases)
                                                                }}
                                                                title="Generate test template"
                                                            >
                                                                <IconCode size={16} />
                                                            </ActionIcon>
                                                            <ActionIcon
                                                                color="red"
                                                                variant="subtle"
                                                                onClick={() => removeTestCase(index)}
                                                            >
                                                                <IconX size={16} />
                                                            </ActionIcon>
                                                        </Group>
                                                    </Group>

                                                    <Grid>
                                                        <Grid.Col span={6}>
                                                            <TextInput
                                                                label="Test Name"
                                                                placeholder="Test description"
                                                                size="sm"
                                                                value={testCase.name}
                                                                onChange={(e) => {
                                                                    const newTestCases = [...form.values.testCases]
                                                                    newTestCases[index].name = e.target.value
                                                                    form.setFieldValue('testCases', newTestCases)
                                                                }}
                                                            />
                                                        </Grid.Col>
                                                        <Grid.Col span={6}>
                                                            <Switch
                                                                label="Hidden from students"
                                                                description="Students won't see this test case"
                                                                checked={testCase.isHidden || false}
                                                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
                                                                    const newTestCases = [...form.values.testCases]
                                                                    newTestCases[index].isHidden = e.currentTarget.checked
                                                                    form.setFieldValue('testCases', newTestCases)
                                                                }}
                                                            />
                                                        </Grid.Col>
                                                    </Grid>

                                                    <Textarea
                                                        label="Description"
                                                        placeholder="What this test validates"
                                                        size="sm"
                                                        minRows={1}
                                                        value={testCase.description || ''}
                                                        onChange={(e) => {
                                                            const newTestCases = [...form.values.testCases]
                                                            newTestCases[index].description = e.target.value
                                                            form.setFieldValue('testCases', newTestCases)
                                                        }}
                                                    />

                                                    <Grid>
                                                        <Grid.Col span={6}>
                                                            <JsonInput
                                                                label="Input"
                                                                placeholder='{"param1": "value1"}'
                                                                size="sm"
                                                                minRows={2}
                                                                value={typeof testCase.input === 'string' ? testCase.input : JSON.stringify(testCase.input)}
                                                                onChange={(value) => {
                                                                    const newTestCases = [...form.values.testCases]
                                                                    try {
                                                                        newTestCases[index].input = JSON.parse(value)
                                                                    } catch {
                                                                        newTestCases[index].input = value
                                                                    }
                                                                    form.setFieldValue('testCases', newTestCases)
                                                                }}
                                                            />
                                                        </Grid.Col>
                                                        <Grid.Col span={6}>
                                                            <JsonInput
                                                                label="Expected Output"
                                                                placeholder='"expected result"'
                                                                size="sm"
                                                                minRows={2}
                                                                value={typeof testCase.expectedOutput === 'string' ? testCase.expectedOutput : JSON.stringify(testCase.expectedOutput)}
                                                                onChange={(value) => {
                                                                    const newTestCases = [...form.values.testCases]
                                                                    try {
                                                                        newTestCases[index].expectedOutput = JSON.parse(value)
                                                                    } catch {
                                                                        newTestCases[index].expectedOutput = value
                                                                    }
                                                                    form.setFieldValue('testCases', newTestCases)
                                                                }}
                                                            />
                                                        </Grid.Col>
                                                    </Grid>

                                                    <Divider label="Unit Test Code" labelPosition="left" />

                                                    <Textarea
                                                        label="Unit Test Implementation"
                                                        placeholder="Write the actual unit test code here..."
                                                        description={`Write the unit test code in ${form.values.language || 'your chosen language'}`}
                                                        size="sm"
                                                        minRows={6}
                                                        maxRows={12}
                                                        value={testCase.testCode || ''}
                                                        onChange={(e) => {
                                                            const newTestCases = [...form.values.testCases]
                                                            newTestCases[index].testCode = e.target.value
                                                            form.setFieldValue('testCases', newTestCases)
                                                        }}
                                                        styles={{
                                                            input: {
                                                                fontFamily: 'Monaco, Menlo, "Ubuntu Mono", monospace',
                                                                fontSize: '13px'
                                                            }
                                                        }}
                                                    />

                                                    {testCase.testCode && (
                                                        <Alert color="blue" variant="light" icon={<IconInfoCircle size={16} />}>
                                                            <Text size="xs">
                                                                This unit test will be executed to validate student solutions.
                                                                Make sure it properly tests the <Code>{form.values.mainFunction}</Code> function.
                                                            </Text>
                                                        </Alert>
                                                    )}
                                                </Stack>
                                            </Card>
                                        ))}

                                        {form.values.testCases.length === 0 && (
                                            <Card withBorder p="xl" radius="md">
                                                <Center>
                                                    <Stack align="center" gap="md">
                                                        <ThemeIcon size="xl" color="gray" variant="light">
                                                            <IconFlask size={24} />
                                                        </ThemeIcon>
                                                        <div style={{ textAlign: 'center' }}>
                                                            <Text fw={500}>No test cases yet</Text>
                                                            <Text size="sm" c="dimmed">
                                                                Add test cases to validate student solutions
                                                            </Text>
                                                        </div>
                                                        <Button
                                                            leftSection={<IconTestPipe size={16} />}
                                                            onClick={addTestCase}
                                                        >
                                                            Add First Test Case
                                                        </Button>
                                                    </Stack>
                                                </Center>
                                            </Card>
                                        )}
                                    </Stack>
                                </div>

                                {/* Additional Resources */}
                                <div>
                                    <Group justify="space-between" mb="md">
                                        <div>
                                            <Text fw={500}>Additional Resources</Text>
                                            <Text size="sm" c="dimmed">
                                                Optional resources to help students learn
                                            </Text>
                                        </div>
                                        <Button
                                            leftSection={<IconLink size={16} />}
                                            variant="light"
                                            onClick={addResource}
                                        >
                                            Add Resource
                                        </Button>
                                    </Group>

                                    <Stack gap="md">
                                        {form.values.additionalResources.map((resource, index) => (
                                            <Card key={index} withBorder p="md">
                                                <Grid>
                                                    <Grid.Col span={4}>
                                                        <TextInput
                                                            label="Title"
                                                            placeholder="Resource title"
                                                            size="sm"
                                                            value={resource.title}
                                                            onChange={(e) => {
                                                                const newResources = [...form.values.additionalResources]
                                                                newResources[index].title = e.target.value
                                                                form.setFieldValue('additionalResources', newResources)
                                                            }}
                                                        />
                                                    </Grid.Col>
                                                    <Grid.Col span={4}>
                                                        <TextInput
                                                            label="URL"
                                                            placeholder="https://..."
                                                            size="sm"
                                                            value={resource.url}
                                                            onChange={(e) => {
                                                                const newResources = [...form.values.additionalResources]
                                                                newResources[index].url = e.target.value
                                                                form.setFieldValue('additionalResources', newResources)
                                                            }}
                                                        />
                                                    </Grid.Col>
                                                    <Grid.Col span={3}>
                                                        <Select
                                                            label="Type"
                                                            data={RESOURCE_TYPES}
                                                            size="sm"
                                                            value={resource.type}
                                                            onChange={(value) => {
                                                                const newResources = [...form.values.additionalResources]
                                                                newResources[index].type = value as AdditionalResource['type']
                                                                form.setFieldValue('additionalResources', newResources)
                                                            }}
                                                        />
                                                    </Grid.Col>
                                                    <Grid.Col span={1}>
                                                        <Text size="sm" c="dimmed" mb={4}>Action</Text>
                                                        <ActionIcon
                                                            color="red"
                                                            variant="subtle"
                                                            onClick={() => removeResource(index)}
                                                        >
                                                            <IconX size={16} />
                                                        </ActionIcon>
                                                    </Grid.Col>
                                                </Grid>
                                            </Card>
                                        ))}
                                    </Stack>
                                </div>
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
                                            Review your lesson before creating
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
                                                    <div>
                                                        <Text fw={600} size="lg">{form.values.title || 'Untitled Lesson'}</Text>
                                                        <Text size="sm" c="dimmed">Lesson #{form.values.order}</Text>
                                                    </div>
                                                </Group>

                                                {form.values.content && (
                                                    <Box>
                                                        <Text size="sm" fw={500} mb="xs">Content Preview:</Text>
                                                        <Code block>
                                                            {form.values.content.substring(0, 200)}
                                                            {form.values.content.length > 200 && '...'}
                                                        </Code>
                                                    </Box>
                                                )}

                                                <Group gap="xs">
                                                    {selectedLanguage && (
                                                        <Badge color="blue" variant="light">
                                                            {selectedLanguage.label}
                                                        </Badge>
                                                    )}
                                                    <Badge color="green" variant="light">
                                                        {form.values.testCases.length} Test Cases
                                                        {form.values.testCases.filter(tc => tc.isHidden).length > 0 &&
                                                            ` (${form.values.testCases.filter(tc => tc.isHidden).length} hidden)`
                                                        }
                                                    </Badge>
                                                    <Badge color="orange" variant="light">
                                                        {form.values.additionalResources.length} Resources
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
                                                        <IconCode size={14} />
                                                    </ThemeIcon>
                                                    <div>
                                                        <Text size="sm" fw={500}>Main Function</Text>
                                                        <Text size="xs" c="dimmed">{form.values.mainFunction || 'Not set'}</Text>
                                                    </div>
                                                </Group>
                                            </Card>

                                            <Card withBorder radius="md" p="md">
                                                <Group>
                                                    <ThemeIcon size="sm" color="green" variant="light">
                                                        <IconFlask size={14} />
                                                    </ThemeIcon>
                                                    <div>
                                                        <Text size="sm" fw={500}>Ready to Test</Text>
                                                        <Text size="xs" c="dimmed">
                                                            {form.values.testCases.length} test cases configured
                                                        </Text>
                                                    </div>
                                                </Group>
                                            </Card>
                                        </Stack>
                                    </Grid.Col>
                                </Grid>

                                <Alert icon={<IconInfoCircle size={16} />} color="blue" variant="light">
                                    <Text size="sm">
                                        After creating your lesson, students will be able to access it from the course page.
                                        You can always edit the lesson content and test cases later.
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
                                onClick={() => navigate(`/courses/${courseId}`)}
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
                                    leftSection={<IconCheck size={16} />}
                                >
                                    Create Lesson
                                </Button>
                            )}
                        </Group>
                    </Group>
                </form>
            </Stack>
        </Container>
    )
}

export default CreateLessonPage
