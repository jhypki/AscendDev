import { Box, Group, Paper, Progress, Stack, Text, Badge, Collapse, ActionIcon } from '@mantine/core'
import { IconCheck, IconX, IconChevronDown, IconChevronRight } from '@tabler/icons-react'
import { useState } from 'react'
import type { TestResult, TestCaseResult } from '../../types/lesson'

interface TestResultsProps {
    testResult: TestResult
    showDetails?: boolean
    onClose?: () => void
}

interface TestCaseItemProps {
    testCase: TestCaseResult
    index: number
}

const TestCaseItem = ({ testCase, index }: TestCaseItemProps) => {
    const [opened, setOpened] = useState(false)

    return (
        <Paper
            withBorder
            p="sm"
            style={{
                borderColor: testCase.passed
                    ? 'var(--mantine-color-green-6)'
                    : 'var(--mantine-color-red-6)',
                borderWidth: '2px'
            }}
        >
            <Group
                justify="space-between"
                align="center"
                style={{ cursor: 'pointer' }}
                onClick={() => setOpened(!opened)}
            >
                <Group gap="xs" align="center">
                    {testCase.passed ? (
                        <IconCheck size={16} color="var(--mantine-color-green-6)" />
                    ) : (
                        <IconX size={16} color="var(--mantine-color-red-6)" />
                    )}
                    <Text size="sm" fw={500}>
                        Test {index + 1}: {testCase.testName || testCase.name || 'Unnamed Test'}
                    </Text>
                    <Badge
                        size="xs"
                        color={testCase.passed ? 'green' : 'red'}
                        variant="light"
                    >
                        {testCase.passed ? 'PASSED' : 'FAILED'}
                    </Badge>
                </Group>

                {opened ? (
                    <IconChevronDown size={16} />
                ) : (
                    <IconChevronRight size={16} />
                )}
            </Group>

            <Collapse in={opened}>
                <Stack gap="xs" mt="sm">
                    {(testCase.errorMessage || testCase.message) && (
                        <Box>
                            <Text size="xs" c="dimmed" mb={4}>Message:</Text>
                            <Paper withBorder p="xs">
                                <Text size="xs" ff="monospace" c={testCase.passed ? "green" : "red"}>
                                    {testCase.message || testCase.errorMessage}
                                </Text>
                            </Paper>
                        </Box>
                    )}

                    {(testCase.input !== undefined || testCase.expectedOutput !== undefined || testCase.actualOutput !== undefined) && (
                        <Group gap="md" grow>
                            {testCase.input !== undefined && (
                                <Box>
                                    <Text size="xs" c="dimmed" mb={4}>Input:</Text>
                                    <Paper withBorder p="xs">
                                        <Text size="xs" ff="monospace">
                                            {JSON.stringify(testCase.input, null, 2)}
                                        </Text>
                                    </Paper>
                                </Box>
                            )}

                            {testCase.expectedOutput !== undefined && (
                                <Box>
                                    <Text size="xs" c="dimmed" mb={4}>Expected Output:</Text>
                                    <Paper withBorder p="xs">
                                        <Text size="xs" ff="monospace">
                                            {JSON.stringify(testCase.expectedOutput, null, 2)}
                                        </Text>
                                    </Paper>
                                </Box>
                            )}

                            {!testCase.passed && testCase.actualOutput !== undefined && (
                                <Box>
                                    <Text size="xs" c="dimmed" mb={4}>Actual Output:</Text>
                                    <Paper withBorder p="xs">
                                        <Text size="xs" ff="monospace">
                                            {JSON.stringify(testCase.actualOutput, null, 2)}
                                        </Text>
                                    </Paper>
                                </Box>
                            )}
                        </Group>
                    )}
                </Stack>
            </Collapse>
        </Paper>
    )
}

export const TestResults = ({ testResult, showDetails = true, onClose }: TestResultsProps) => {
    const passedTests = testResult.testResults?.filter(test => test.passed).length || 0
    const totalTests = testResult.testResults?.length || 0
    const successRate = totalTests > 0 ? (passedTests / totalTests) * 100 : 0

    return (
        <Paper p="md">
            <Stack gap="md">
                {/* Header */}
                <Group justify="space-between" align="center">
                    <Group gap="xs" align="center">
                        {testResult.success ? (
                            <IconCheck size={20} color="var(--mantine-color-green-6)" />
                        ) : (
                            <IconX size={20} color="var(--mantine-color-red-6)" />
                        )}
                        <Text size="lg" fw={600}>
                            Test Results
                        </Text>
                        <Badge
                            size="sm"
                            color={testResult.success ? 'green' : 'red'}
                            variant="filled"
                        >
                            {testResult.success ? 'ALL PASSED' : 'SOME FAILED'}
                        </Badge>
                    </Group>

                    <Group gap="md">
                        <Text size="sm" c="dimmed">
                            {passedTests}/{totalTests} passed
                        </Text>
                        {testResult.performance?.pureTestExecutionTimeMs && (
                            <Text size="sm" c="dimmed">
                                {Math.round(testResult.performance.pureTestExecutionTimeMs)}ms
                            </Text>
                        )}
                        {onClose && (
                            <ActionIcon
                                variant="subtle"
                                color="gray"
                                onClick={onClose}
                                size="sm"
                            >
                                <IconX size={16} />
                            </ActionIcon>
                        )}
                    </Group>
                </Group>

                {/* Progress Bar */}
                <Box>
                    <Group justify="space-between" mb="xs">
                        <Text size="sm" fw={500}>Success Rate</Text>
                        <Text size="sm" fw={500}>{successRate.toFixed(1)}%</Text>
                    </Group>
                    <Progress
                        value={successRate}
                        color={successRate === 100 ? 'green' : successRate >= 50 ? 'yellow' : 'red'}
                        size="lg"
                        radius="md"
                    />
                </Box>

                {/* Error Message */}
                {testResult.errorMessage && (
                    <Box>
                        <Text size="sm" c="dimmed" mb={4}>Error:</Text>
                        <Paper withBorder p="sm">
                            <Text size="sm" ff="monospace" c="red">
                                {testResult.errorMessage}
                            </Text>
                        </Paper>
                    </Box>
                )}

                {/* Compilation Output */}
                {testResult.compilationOutput && (
                    <Box>
                        <Text size="sm" c="dimmed" mb={4}>Compilation Output:</Text>
                        <Paper withBorder p="sm">
                            <Text size="sm" ff="monospace">
                                {testResult.compilationOutput}
                            </Text>
                        </Paper>
                    </Box>
                )}

                {/* Keyword Validation */}
                {testResult.keywordValidation && (
                    <Box>
                        <Text size="sm" fw={500} c="dimmed" mb="sm">Code Validation</Text>
                        <Paper
                            withBorder
                            p="sm"
                            style={{
                                backgroundColor: testResult.keywordValidation.isValid
                                    ? 'var(--mantine-color-green-light)'
                                    : 'var(--mantine-color-red-light)',
                                borderColor: testResult.keywordValidation.isValid
                                    ? 'var(--mantine-color-green-6)'
                                    : 'var(--mantine-color-red-6)'
                            }}
                        >
                            <Group gap="xs" align="center" mb="xs">
                                {testResult.keywordValidation.isValid ? (
                                    <IconCheck size={16} color="var(--mantine-color-green-6)" />
                                ) : (
                                    <IconX size={16} color="var(--mantine-color-red-6)" />
                                )}
                                <Text size="sm" fw={500}>
                                    {testResult.keywordValidation.validationMessage}
                                </Text>
                            </Group>
                            {testResult.keywordValidation.errors.length > 0 && (
                                <Stack gap="xs">
                                    {testResult.keywordValidation.errors.map((error, index) => {
                                        const errorText = typeof error === 'string'
                                            ? error
                                            : `${error.keyword}: ${error.errorMessage} (expected: ${error.expectedOccurrences}, found: ${error.actualOccurrences})`

                                        return (
                                            <Text key={index} size="xs" c="red">
                                                • {errorText}
                                            </Text>
                                        )
                                    })}
                                </Stack>
                            )}
                        </Paper>
                    </Box>
                )}

                {/* Performance Metrics */}
                {testResult.performance && (
                    <Box>
                        <Text size="sm" fw={500} c="dimmed" mb="sm">Performance Metrics</Text>
                        <Paper withBorder p="sm">
                            <Stack gap="xs">
                                <Group gap="md" wrap="wrap">
                                    {testResult.performance.pureTestExecutionTimeMs && (
                                        <Text size="xs">
                                            <Text span fw={500} c="green">Pure Execution:</Text> {Math.round(testResult.performance.pureTestExecutionTimeMs)}ms
                                        </Text>
                                    )}
                                    {testResult.performance.totalExecutionTimeMs && (
                                        <Text size="xs">
                                            <Text span fw={500}>Total Time:</Text> {testResult.performance.totalExecutionTimeMs}ms
                                        </Text>
                                    )}
                                    {testResult.performance.containerStartupTimeMs && (
                                        <Text size="xs">
                                            <Text span fw={500}>Container Startup:</Text> {testResult.performance.containerStartupTimeMs}ms
                                        </Text>
                                    )}
                                    {testResult.performance.averageTestTimeMs && (
                                        <Text size="xs">
                                            <Text span fw={500}>Avg Test Time:</Text> {testResult.performance.averageTestTimeMs.toFixed(1)}ms
                                        </Text>
                                    )}
                                </Group>

                                {testResult.performance.infrastructureOverheadMs && testResult.performance.infrastructureOverheadMs > 0 && (
                                    <Group gap="md" wrap="wrap">
                                        <Text size="xs">
                                            <Text span fw={500} c="orange">Infrastructure Overhead:</Text> {testResult.performance.infrastructureOverheadMs}ms
                                        </Text>
                                        {testResult.performance.containerCleanupTimeMs && (
                                            <Text size="xs">
                                                <Text span fw={500}>Cleanup:</Text> {testResult.performance.containerCleanupTimeMs}ms
                                            </Text>
                                        )}
                                        {testResult.performance.filePreparationTimeMs && (
                                            <Text size="xs">
                                                <Text span fw={500}>File Prep:</Text> {testResult.performance.filePreparationTimeMs}ms
                                            </Text>
                                        )}
                                    </Group>
                                )}

                                {testResult.performance.additionalMetrics?.memoryLimitMb && typeof testResult.performance.additionalMetrics.memoryLimitMb === 'number' && (
                                    <Text size="xs">
                                        <Text span fw={500}>Memory Limit:</Text> {testResult.performance.additionalMetrics.memoryLimitMb}MB
                                    </Text>
                                )}
                            </Stack>
                        </Paper>
                    </Box>
                )}

                {/* Test Cases */}
                {showDetails && testResult.testResults && testResult.testResults.length > 0 && (
                    <Stack gap="xs">
                        <Text size="sm" fw={500} c="dimmed">
                            Test Cases ({testResult.testResults.length})
                        </Text>
                        {testResult.testResults.map((testCase, index) => (
                            <TestCaseItem
                                key={index}
                                testCase={testCase}
                                index={index}
                            />
                        ))}
                    </Stack>
                )}

                {/* Summary for successful tests */}
                {testResult.success && totalTests > 0 && (
                    <Paper
                        withBorder
                        p="sm"
                        style={{
                            backgroundColor: 'var(--mantine-color-green-light)',
                            borderColor: 'var(--mantine-color-green-6)'
                        }}
                    >
                        <Group gap="xs" align="center">
                            <IconCheck size={16} color="var(--mantine-color-green-6)" />
                            <Text size="sm" c="green">
                                Congratulations! All {totalTests} test{totalTests !== 1 ? 's' : ''} passed successfully.
                            </Text>
                        </Group>
                    </Paper>
                )}
            </Stack>
        </Paper>
    )
}

export default TestResults