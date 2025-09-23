import { useState, useEffect } from 'react'
import {
    Container,
    Grid,
    Stack,
    Text,
    Paper,
    Group,
    Button,
    Badge,
    Divider, Alert,
    LoadingOverlay
} from '@mantine/core'
import {
    IconBook,
    IconChartBar,
    IconInfoCircle,
    IconExternalLink,
    IconCheck,
    IconMessage
} from '@tabler/icons-react'
import { useParams, useNavigate } from 'react-router-dom'
import { notifications } from '@mantine/notifications'
import { CodeEditor } from './CodeEditor'
import { TestResults } from './TestResults'
import { ProgressTracker } from './ProgressTracker'
import { MarkdownRenderer } from './MarkdownRenderer'
import { useLesson, useCourseProgress, useCompleteLesson } from '../../hooks/api/useLessons'
import { useCourse } from '../../hooks/api/useCourses'
import { useDiscussionCount } from '../../hooks/api/useDiscussions'
import { CommentSection } from '../social/CommentSection'
import type { TestResult } from '../../types/lesson'
import type { Discussion } from '../../types/discussion'

export const LessonViewer = () => {
    const { courseId, lessonId } = useParams<{ courseId: string; lessonId: string }>()
    const navigate = useNavigate()
    const [activeTab, setActiveTab] = useState<string>('lesson')
    const [currentCode, setCurrentCode] = useState('')
    const [testResult, setTestResult] = useState<TestResult | null>(null)

    // API hooks
    const { data: course, isLoading: courseLoading } = useCourse(courseId!, !!courseId)
    const { data: lesson, isLoading: lessonLoading } = useLesson(courseId!, lessonId!, !!courseId && !!lessonId)
    const { data: progress, isLoading: progressLoading } = useCourseProgress(courseId!, !!courseId)
    const { data: discussionCount } = useDiscussionCount(lessonId)
    const completeLessonMutation = useCompleteLesson()

    // Set initial code when lesson loads
    useEffect(() => {
        if (lesson && !currentCode) {
            setCurrentCode(lesson.template || '')
        }
    }, [lesson, currentCode])

    const handleCodeChange = (code: string) => {
        setCurrentCode(code)
    }

    const handleTestResults = (results: TestResult) => {
        setTestResult(results)

        // Show notification based on test results
        if (results.success) {
            notifications.show({
                title: 'Tests Passed!',
                message: 'All tests passed successfully. Great job!',
                color: 'green',
                icon: <IconCheck size={16} />
            })
        } else {
            notifications.show({
                title: 'Tests Failed',
                message: 'Some tests failed. Check the results and try again.',
                color: 'red'
            })
        }
    }

    const handleCompleteLesson = async () => {
        if (!courseId || !lessonId) return

        try {
            await completeLessonMutation.mutateAsync({ courseId, lessonId })
            notifications.show({
                title: 'Lesson Completed!',
                message: 'You have successfully completed this lesson.',
                color: 'green',
                icon: <IconCheck size={16} />
            })
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to mark lesson as complete.',
                color: 'red'
            })
        }
    }

    const handlePinDiscussion = (discussion: Discussion) => {
        // TODO: Implement pin/unpin discussion for lessons
        console.log('Pin discussion:', discussion)
    }

    const handleLockDiscussion = (discussion: Discussion) => {
        // TODO: Implement lock/unlock discussion for lessons
        console.log('Lock discussion:', discussion)
    }

    const getCurrentLessonProgress = () => {
        if (!progress || !Array.isArray(progress) || !lessonId) return null
        return progress.find(p => p.lessonId === lessonId)
    }

    const getNextLesson = () => {
        if (!course?.lessonSummaries || !lessonId) return null
        const currentIndex = course.lessonSummaries.findIndex(l => l.id === lessonId)
        if (currentIndex >= 0 && currentIndex < course.lessonSummaries.length - 1) {
            return course.lessonSummaries[currentIndex + 1]
        }
        return null
    }

    const handleNextLesson = () => {
        const nextLesson = getNextLesson()
        if (nextLesson && courseId) {
            navigate(`/courses/${courseId}/lessons/${nextLesson.id}`)
        }
    }

    if (courseLoading || lessonLoading || progressLoading) {
        return (
            <Container size="xl" py="xl" pos="relative">
                <LoadingOverlay visible />
            </Container>
        )
    }

    if (!course || !lesson) {
        return (
            <Container size="xl" py="xl">
                <Alert color="red" title="Error" icon={<IconInfoCircle size={16} />}>
                    Lesson not found or you don't have access to this content.
                </Alert>
            </Container>
        )
    }

    const currentProgress = getCurrentLessonProgress()
    const nextLesson = getNextLesson()
    const isCompleted = currentProgress?.completed || false

    return (
        <Container size="xl" py="xl">
            <Stack gap="lg">
                {/* Header */}
                <Paper withBorder p="md">
                    <Group justify="space-between" align="flex-start">
                        <Stack gap="xs">
                            <Group gap="xs" align="center">
                                <Text size="sm" c="dimmed">{course.title}</Text>
                                <Text size="sm" c="dimmed">•</Text>
                                <Badge size="sm" color="blue" variant="light">
                                    {lesson.language}
                                </Badge>
                                {isCompleted && (
                                    <Badge size="sm" color="green" variant="filled">
                                        Completed
                                    </Badge>
                                )}
                            </Group>
                            <Text size="xl" fw={700}>{lesson.title}</Text>
                        </Stack>

                        <Group gap="sm">
                            {testResult?.success && !isCompleted && (
                                <Button
                                    variant="filled"
                                    color="green"
                                    leftSection={<IconCheck size={16} />}
                                    onClick={handleCompleteLesson}
                                    loading={completeLessonMutation.isPending}
                                >
                                    Complete Lesson
                                </Button>
                            )}
                            {nextLesson && (
                                <Button
                                    variant="light"
                                    rightSection={<IconExternalLink size={16} />}
                                    onClick={handleNextLesson}
                                >
                                    Next: {nextLesson.title}
                                </Button>
                            )}
                        </Group>
                    </Group>
                </Paper>

                {/* Main Content - Split Layout */}
                <Grid>
                    {/* Left Side - Lesson Content */}
                    <Grid.Col span={{ base: 12, lg: 5 }}>
                        <Paper withBorder h="calc(100vh - 200px)" style={{ overflow: 'hidden', display: 'flex', flexDirection: 'column' }}>
                            <div style={{ padding: 'var(--mantine-spacing-md)', paddingBottom: 0 }}>
                                <Group justify="space-between" align="center" mb="md">
                                    <Group gap="xs" align="center">
                                        <IconBook size={20} />
                                        <Text size="lg" fw={600}>Lesson Content</Text>
                                    </Group>
                                    <Group gap="xs">
                                        <Button
                                            size="xs"
                                            variant="light"
                                            leftSection={<IconChartBar size={14} />}
                                            onClick={() => setActiveTab(activeTab === 'progress' ? 'lesson' : 'progress')}
                                        >
                                            {activeTab === 'progress' ? 'Back to Lesson' : 'View Progress'}
                                        </Button>
                                        <Button
                                            size="xs"
                                            variant="light"
                                            leftSection={<IconMessage size={14} />}
                                            onClick={() => setActiveTab(activeTab === 'discussions' ? 'lesson' : 'discussions')}
                                        >
                                            {activeTab === 'discussions' ? 'Back to Lesson' : `Discussions${discussionCount ? ` (${discussionCount})` : ''}`}
                                        </Button>
                                    </Group>
                                </Group>
                                <Divider />
                            </div>

                            {activeTab === 'progress' ? (
                                <div style={{ flex: 1, overflow: 'auto', padding: 'var(--mantine-spacing-md)' }}>
                                    {progress && Array.isArray(progress) && (
                                        <ProgressTracker
                                            progress={progress}
                                            currentLessonId={lessonId}
                                        />
                                    )}
                                </div>
                            ) : activeTab === 'discussions' ? (
                                <div style={{ flex: 1, overflow: 'auto', padding: 'var(--mantine-spacing-md)' }}>
                                    <CommentSection
                                        lessonId={lessonId}
                                        onPinDiscussion={handlePinDiscussion}
                                        onLockDiscussion={handleLockDiscussion}
                                        canModerate={false} // TODO: Add proper role checking
                                    />
                                </div>
                            ) : (
                                <div style={{ flex: 1, overflow: 'auto', padding: 'var(--mantine-spacing-md)' }}>
                                    <Stack gap="lg">
                                        {/* Lesson Content */}
                                        <MarkdownRenderer
                                            content={lesson.content}
                                        />

                                        {/* Test Cases */}
                                        {lesson.testCases && lesson.testCases.length > 0 && (
                                            <div>
                                                <Text size="md" fw={600} mb="sm">Test Cases</Text>
                                                <Stack gap="sm">
                                                    {lesson.testCases.map((testCase, index) => (
                                                        <Paper key={testCase.id} withBorder p="sm">
                                                            <Stack gap="xs">
                                                                <Text size="sm" fw={500}>
                                                                    Test {index + 1}: {testCase.name}
                                                                </Text>
                                                                {testCase.description && (
                                                                    <Text size="sm" c="dimmed">
                                                                        {testCase.description}
                                                                    </Text>
                                                                )}
                                                                <Group gap="md">
                                                                    <Text size="xs">
                                                                        <Text span fw={500}>Input:</Text> {JSON.stringify(testCase.input)}
                                                                    </Text>
                                                                    <Text size="xs">
                                                                        <Text span fw={500}>Expected:</Text> {JSON.stringify(testCase.expectedOutput)}
                                                                    </Text>
                                                                </Group>
                                                            </Stack>
                                                        </Paper>
                                                    ))}
                                                </Stack>
                                            </div>
                                        )}

                                        {/* Additional Resources */}
                                        {lesson.additionalResources && lesson.additionalResources.length > 0 && (
                                            <div>
                                                <Text size="md" fw={600} mb="sm">Additional Resources</Text>
                                                <Stack gap="xs">
                                                    {lesson.additionalResources.map((resource, index) => (
                                                        <Group key={index} gap="xs">
                                                            <IconExternalLink size={14} />
                                                            <Text
                                                                size="sm"
                                                                component="a"
                                                                href={resource.url}
                                                                target="_blank"
                                                                style={{ textDecoration: 'none' }}
                                                            >
                                                                {resource.title}
                                                            </Text>
                                                            <Badge size="xs" variant="light">
                                                                {resource.type}
                                                            </Badge>
                                                        </Group>
                                                    ))}
                                                </Stack>
                                            </div>
                                        )}

                                        {/* Tags */}
                                        {lesson.tags && lesson.tags.length > 0 && (
                                            <div>
                                                <Text size="md" fw={600} mb="sm">Tags</Text>
                                                <Group gap="xs">
                                                    {lesson.tags.map((tag, index) => (
                                                        <Badge key={index} size="sm" variant="light">
                                                            {tag}
                                                        </Badge>
                                                    ))}
                                                </Group>
                                            </div>
                                        )}
                                    </Stack>
                                </div>
                            )}
                        </Paper>
                    </Grid.Col>

                    {/* Right Side - Code Editor */}
                    <Grid.Col span={{ base: 12, lg: 7 }}>
                        <Stack gap="lg" h="calc(100vh - 200px)" style={{ overflow: 'hidden' }}>
                            <div style={{
                                flex: testResult ? '1 1 60%' : '1 1 100%',
                                minHeight: 0,
                                transition: 'flex 0.3s ease'
                            }}>
                                <CodeEditor
                                    courseId={courseId!}
                                    lessonId={lessonId!}
                                    language={lesson.language}
                                    initialCode={lesson.template}
                                    template={lesson.template}
                                    onCodeChange={handleCodeChange}
                                    onTestResults={handleTestResults}
                                />
                            </div>

                            {testResult && (
                                <div style={{
                                    flex: '0 0 auto',
                                    maxHeight: '40%',
                                    overflow: 'auto',
                                    border: '1px solid var(--mantine-color-default-border)',
                                    borderRadius: 'var(--mantine-radius-md)',
                                    backgroundColor: 'var(--mantine-color-body)'
                                }}>
                                    <div style={{ padding: '1px' }}>
                                        <TestResults
                                            testResult={testResult}
                                            onClose={() => setTestResult(null)}
                                        />
                                    </div>
                                </div>
                            )}
                        </Stack>
                    </Grid.Col>
                </Grid>
            </Stack>
        </Container>
    )
}

export default LessonViewer