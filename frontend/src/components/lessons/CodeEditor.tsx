import { useRef, useState, useEffect } from 'react'
import { Box, Button, Group, LoadingOverlay, Paper, Stack, Text, ActionIcon } from '@mantine/core'
import { IconPlayerPlay, IconCheck, IconX, IconReload } from '@tabler/icons-react'
import { useDebounce } from 'use-debounce'
import Editor from '@monaco-editor/react'
import type { editor } from 'monaco-editor'
import { useRunCode, useRunTests, useSaveUserCode, useUserCode, useResetUserCode } from '../../hooks/api/useLessons'
import type { CodeExecutionResult, TestResult } from '../../types/lesson'

interface CodeEditorProps {
    courseId: string
    lessonId: string
    language: string
    initialCode?: string
    template?: string
    onCodeChange?: (code: string) => void
    onTestResults?: (results: TestResult) => void
    readOnly?: boolean
}

export const CodeEditor = ({
    courseId,
    lessonId,
    language,
    initialCode = '',
    template = '',
    onCodeChange,
    onTestResults,
    readOnly = false
}: CodeEditorProps) => {
    const editorRef = useRef<editor.IStandaloneCodeEditor | null>(null)
    const [code, setCode] = useState(initialCode || template)
    const [executionResult, setExecutionResult] = useState<CodeExecutionResult | null>(null)
    const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false)
    const [isInitialized, setIsInitialized] = useState(false)

    // Debounce the code to avoid excessive API calls
    const [debouncedCode] = useDebounce(code, 2000) // 2 second delay

    const runCodeMutation = useRunCode()
    const runTestsMutation = useRunTests()
    const saveUserCodeMutation = useSaveUserCode()
    const resetUserCodeMutation = useResetUserCode()

    // Get user's saved code
    const { data: userCode, isLoading: isLoadingUserCode } = useUserCode(courseId, lessonId, !readOnly)

    // Initialize code when user code is loaded or when template is available
    useEffect(() => {
        if (!isInitialized && !isLoadingUserCode) {
            if (userCode?.code) {
                setCode(userCode.code)
            } else if (template) {
                setCode(template)
            }
            setIsInitialized(true)
        }
    }, [userCode?.code, template, isInitialized, isLoadingUserCode])

    // Save code when debounced code changes (but not during initialization)
    useEffect(() => {
        if (
            isInitialized &&
            debouncedCode &&
            debouncedCode !== (userCode?.code || template) &&
            !readOnly &&
            hasUnsavedChanges
        ) {
            saveUserCodeMutation.mutate({
                courseId,
                lessonId,
                code: debouncedCode
            }, {
                onSuccess: () => {
                    setHasUnsavedChanges(false)
                },
                onError: (error) => {
                    console.error('Failed to save code:', error)
                }
            })
        }
    }, [debouncedCode, courseId, lessonId, userCode?.code, template, readOnly, isInitialized, hasUnsavedChanges])

    const handleEditorDidMount = (editor: editor.IStandaloneCodeEditor) => {
        editorRef.current = editor
    }

    const handleEditorChange = (value: string | undefined) => {
        const newCode = value || ''
        setCode(newCode)
        setHasUnsavedChanges(true)
        onCodeChange?.(newCode)
    }

    const handleResetCode = async () => {
        try {
            await resetUserCodeMutation.mutateAsync({ courseId, lessonId })
            setCode(template)
            setHasUnsavedChanges(false)
            setIsInitialized(true) // Mark as initialized to prevent re-loading
            onCodeChange?.(template)
        } catch (error) {
            console.error('Failed to reset code:', error)
        }
    }

    const handleRunCode = async () => {
        if (!code.trim()) return

        try {
            const result = await runCodeMutation.mutateAsync({
                language,
                code
            })
            setExecutionResult(result)
        } catch (error) {
            console.error('Code execution failed:', error)
        }
    }

    const handleRunTests = async () => {
        if (!code.trim()) return

        try {
            const result = await runTestsMutation.mutateAsync({
                lessonId,
                code
            })
            setExecutionResult(null) // Clear execution results when running tests
            onTestResults?.(result)
        } catch (error) {
            console.error('Test execution failed:', error)
        }
    }

    const getLanguageForMonaco = (lang: string): string => {
        switch (lang.toLowerCase()) {
            case 'javascript':
            case 'js':
                return 'javascript'
            case 'typescript':
            case 'ts':
                return 'typescript'
            case 'python':
            case 'py':
                return 'python'
            case 'csharp':
            case 'c#':
                return 'csharp'
            case 'go':
                return 'go'
            default:
                return 'javascript'
        }
    }

    const isLoading = runCodeMutation.isPending || runTestsMutation.isPending

    return (
        <Stack gap="md" style={{ height: '100%', overflow: 'hidden' }}>
            <Paper withBorder pos="relative" style={{
                height: '100%',
                display: 'flex',
                flexDirection: 'column',
                overflow: 'hidden'
            }}>
                <LoadingOverlay visible={isLoading} />

                <div style={{
                    padding: 'var(--mantine-spacing-md)',
                    paddingBottom: 0,
                    flexShrink: 0
                }}>
                    <Group justify="space-between" align="center" mb="sm">
                        <Text size="sm" fw={500} c="dimmed">
                            Code Editor ({language})
                        </Text>
                        <Group gap="xs">
                            <Button
                                size="xs"
                                variant="light"
                                leftSection={<IconPlayerPlay size={14} />}
                                onClick={handleRunCode}
                                disabled={isLoading || !code.trim() || readOnly}
                                loading={runCodeMutation.isPending}
                            >
                                Run Code
                            </Button>
                            <Button
                                size="xs"
                                variant="filled"
                                leftSection={<IconCheck size={14} />}
                                onClick={handleRunTests}
                                disabled={isLoading || !code.trim() || readOnly}
                                loading={runTestsMutation.isPending}
                            >
                                Run Tests
                            </Button>
                            {!readOnly && (
                                <Button
                                    size="xs"
                                    variant="subtle"
                                    color="gray"
                                    leftSection={<IconReload size={14} />}
                                    onClick={handleResetCode}
                                    disabled={isLoading}
                                    loading={resetUserCodeMutation.isPending}
                                    title="Reset to default template"
                                >
                                    Reset
                                </Button>
                            )}
                        </Group>
                    </Group>
                </div>

                <Box style={{
                    flex: 1,
                    border: '1px solid var(--mantine-color-gray-3)',
                    margin: 'var(--mantine-spacing-md)',
                    marginTop: 0,
                    minHeight: 0,
                    overflow: 'hidden'
                }}>
                    <Editor
                        height="100%"
                        language={getLanguageForMonaco(language)}
                        value={code}
                        onChange={handleEditorChange}
                        onMount={handleEditorDidMount}
                        theme="vs-dark"
                        options={{
                            minimap: { enabled: false },
                            fontSize: 14,
                            wordWrap: 'on',
                            lineNumbers: 'on',
                            scrollBeyondLastLine: false,
                            automaticLayout: true,
                            tabSize: 2,
                            insertSpaces: true,
                            readOnly,
                            contextmenu: !readOnly,
                            quickSuggestions: !readOnly,
                            suggestOnTriggerCharacters: !readOnly,
                        }}
                    />
                </Box>
            </Paper>

            {/* Execution Results */}
            {executionResult && (
                <Paper withBorder p="md">
                    <Stack gap="xs">
                        <Group justify="space-between" align="center">
                            <Group gap="xs" align="center">
                                {executionResult.success ? (
                                    <IconCheck size={16} color="var(--mantine-color-green-6)" />
                                ) : (
                                    <IconX size={16} color="var(--mantine-color-red-6)" />
                                )}
                                <Text size="sm" fw={500}>
                                    Execution Result ({executionResult.executionTimeMs}ms)
                                </Text>
                            </Group>
                            <ActionIcon
                                variant="subtle"
                                color="gray"
                                onClick={() => setExecutionResult(null)}
                                size="sm"
                            >
                                <IconX size={16} />
                            </ActionIcon>
                        </Group>

                        {executionResult.stdout && (
                            <Box>
                                <Text size="xs" c="dimmed" mb={4}>Output:</Text>
                                <Paper bg="dark" p="xs">
                                    <Text size="xs" ff="monospace" c="green">
                                        {executionResult.stdout}
                                    </Text>
                                </Paper>
                            </Box>
                        )}

                        {executionResult.stderr && (
                            <Box>
                                <Text size="xs" c="dimmed" mb={4}>Error:</Text>
                                <Paper bg="dark" p="xs">
                                    <Text size="xs" ff="monospace" c="red">
                                        {executionResult.stderr}
                                    </Text>
                                </Paper>
                            </Box>
                        )}

                        {executionResult.compilationOutput && (
                            <Box>
                                <Text size="xs" c="dimmed" mb={4}>Compilation:</Text>
                                <Paper bg="dark" p="xs">
                                    <Text size="xs" ff="monospace" c="yellow">
                                        {executionResult.compilationOutput}
                                    </Text>
                                </Paper>
                            </Box>
                        )}
                    </Stack>
                </Paper>
            )}
        </Stack>
    )
}

export default CodeEditor