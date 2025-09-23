import { useRef, useState } from 'react'
import { Box, Button, Group, Paper, Stack, Text, ActionIcon, Select, useComputedColorScheme } from '@mantine/core'
import { IconPlayerPlay, IconCheck, IconX, IconTrash } from '@tabler/icons-react'
import Editor from '@monaco-editor/react'
import type { editor } from 'monaco-editor'
import { useRunCode } from '../../hooks/api/useLessons'
import type { CodeExecutionResult } from '../../types/lesson'

interface PlaygroundCodeEditorProps {
    onLanguageChange?: (language: string) => void
}

const SUPPORTED_LANGUAGES = [
    { value: 'javascript', label: 'JavaScript' },
    { value: 'typescript', label: 'TypeScript' },
    { value: 'python', label: 'Python' },
    { value: 'csharp', label: 'C#' },
    { value: 'go', label: 'Go' },
]

const DEFAULT_CODE_TEMPLATES = {
    javascript: `// Welcome to the JavaScript Playground!
// Write your code below and click "Run Code" to execute it.

console.log("Hello, World!");

// Example: Calculate factorial
function factorial(n) {
    if (n <= 1) return 1;
    return n * factorial(n - 1);
}

console.log("Factorial of 5:", factorial(5));`,

    typescript: `// Welcome to the TypeScript Playground!
// Write your code below and click "Run Code" to execute it.

console.log("Hello, World!");

// Example: Calculate factorial with types
function factorial(n: number): number {
    if (n <= 1) return 1;
    return n * factorial(n - 1);
}

console.log("Factorial of 5:", factorial(5));`,

    python: `# Welcome to the Python Playground!
# Write your code below and click "Run Code" to execute it.

print("Hello, World!")

# Example: Calculate factorial
def factorial(n):
    if n <= 1:
        return 1
    return n * factorial(n - 1)

print("Factorial of 5:", factorial(5))`,

    csharp: `// Welcome to the C# Playground!
// Write your code below and click "Run Code" to execute it.

using System;

class Program 
{
    static void Main() 
    {
        Console.WriteLine("Hello, World!");
        
        // Example: Calculate factorial
        Console.WriteLine("Factorial of 5: " + Factorial(5));
    }
    
    static int Factorial(int n) 
    {
        if (n <= 1) return 1;
        return n * Factorial(n - 1);
    }
}`,

    go: `// Welcome to the Go Playground!
// Write your code below and click "Run Code" to execute it.

package main

import "fmt"

func main() {
    fmt.Println("Hello, World!")
    
    // Example: Calculate factorial
    fmt.Printf("Factorial of 5: %d\\n", factorial(5))
}

func factorial(n int) int {
    if n <= 1 {
        return 1
    }
    return n * factorial(n-1)
}`
}

export const PlaygroundCodeEditor = ({ onLanguageChange }: PlaygroundCodeEditorProps) => {
    const editorRef = useRef<editor.IStandaloneCodeEditor | null>(null)
    const [language, setLanguage] = useState('javascript')
    const [code, setCode] = useState(DEFAULT_CODE_TEMPLATES.javascript)
    const [executionResult, setExecutionResult] = useState<CodeExecutionResult | null>(null)
    const colorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true })

    const runCodeMutation = useRunCode()

    const handleEditorDidMount = (editor: editor.IStandaloneCodeEditor) => {
        editorRef.current = editor
    }

    const handleEditorChange = (value: string | undefined) => {
        const newCode = value || ''
        setCode(newCode)
    }

    const handleLanguageChange = (newLanguage: string | null) => {
        if (!newLanguage) return

        setLanguage(newLanguage)
        setCode(DEFAULT_CODE_TEMPLATES[newLanguage as keyof typeof DEFAULT_CODE_TEMPLATES] || '')
        setExecutionResult(null)
        onLanguageChange?.(newLanguage)
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

    const handleClearCode = () => {
        setCode(DEFAULT_CODE_TEMPLATES[language as keyof typeof DEFAULT_CODE_TEMPLATES] || '')
        setExecutionResult(null)
    }

    const handleClearResults = () => {
        setExecutionResult(null)
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

    return (
        <Stack gap="md" style={{ height: '100%', overflow: 'hidden' }}>
            <Paper withBorder style={{
                height: '100%',
                display: 'flex',
                flexDirection: 'column',
                overflow: 'hidden'
            }}>

                <div style={{
                    padding: 'var(--mantine-spacing-md)',
                    paddingBottom: 0,
                    flexShrink: 0
                }}>
                    <Group justify="space-between" align="center" mb="sm">
                        <Group gap="md" align="center">
                            <Text size="sm" fw={500} c="dimmed">
                                Code Playground
                            </Text>
                            <Select
                                data={SUPPORTED_LANGUAGES}
                                value={language}
                                onChange={handleLanguageChange}
                                size="xs"
                                w={150}
                                comboboxProps={{ withinPortal: false }}
                            />
                        </Group>
                        <Group gap="xs">
                            <Button
                                size="xs"
                                variant="light"
                                leftSection={<IconPlayerPlay size={14} />}
                                onClick={handleRunCode}
                                disabled={!code.trim()}
                                loading={runCodeMutation.isPending}
                            >
                                Run Code
                            </Button>
                            <Button
                                size="xs"
                                variant="subtle"
                                color="gray"
                                leftSection={<IconTrash size={14} />}
                                onClick={handleClearCode}
                                title="Reset to template"
                            >
                                Reset
                            </Button>
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
                        theme={colorScheme === 'dark' ? 'vs-dark' : 'light'}
                        options={{
                            minimap: { enabled: false },
                            fontSize: 14,
                            wordWrap: 'on',
                            lineNumbers: 'on',
                            scrollBeyondLastLine: false,
                            automaticLayout: true,
                            tabSize: 2,
                            insertSpaces: true,
                            contextmenu: true,
                            quickSuggestions: true,
                            suggestOnTriggerCharacters: true,
                            readOnly: false, // Always keep editor editable
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
                                onClick={handleClearResults}
                                size="sm"
                            >
                                <IconX size={16} />
                            </ActionIcon>
                        </Group>

                        {executionResult.stdout && (
                            <Box>
                                <Text size="xs" c="dimmed" mb={4}>Output:</Text>
                                <Paper bg={colorScheme === 'dark' ? 'dark.8' : 'gray.0'} p="xs">
                                    <Text size="xs" ff="monospace" c="green" style={{ whiteSpace: 'pre-wrap' }}>
                                        {executionResult.stdout}
                                    </Text>
                                </Paper>
                            </Box>
                        )}

                        {executionResult.stderr && (
                            <Box>
                                <Text size="xs" c="dimmed" mb={4}>Error:</Text>
                                <Paper bg={colorScheme === 'dark' ? 'dark.8' : 'gray.0'} p="xs">
                                    <Text size="xs" ff="monospace" c="red" style={{ whiteSpace: 'pre-wrap' }}>
                                        {executionResult.stderr}
                                    </Text>
                                </Paper>
                            </Box>
                        )}

                        {executionResult.compilationOutput && (
                            <Box>
                                <Text size="xs" c="dimmed" mb={4}>Compilation:</Text>
                                <Paper bg={colorScheme === 'dark' ? 'dark.8' : 'gray.0'} p="xs">
                                    <Text size="xs" ff="monospace" c="yellow" style={{ whiteSpace: 'pre-wrap' }}>
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

export default PlaygroundCodeEditor