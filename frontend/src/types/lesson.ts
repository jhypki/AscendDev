export interface CodeExecutionResult {
    success: boolean
    stdout: string
    stderr: string
    exitCode: number
    executionTimeMs: number
    compilationOutput: string
}

export interface TestResult {
    success: boolean
    testResults: TestCaseResult[]
    executionTimeMs: number
    compilationOutput?: string
    errorMessage?: string
    keywordValidation?: {
        isValid: boolean
        errors: Array<string | {
            keyword: string
            errorMessage: string
            errorType: string
            expectedOccurrences: number
            actualOccurrences: number
        }>
        matches: Array<{
            keyword: string
            lineNumber: number
            columnStart: number
            columnEnd: number
            matchedText: string
        }>
        validationMessage: string
    }
    performance?: {
        executionTimeMs: number
        containerStartupTimeMs: number
        testFrameworkTimeMs: number
        testCount: number
        averageTestTimeMs: number
        additionalMetrics: {
            containerStatus: string
            containerExitCode: number
            containerRunTimeMs: number
            memoryLimitMb: number
        }
    }
}

export interface TestCaseResult {
    testName: string
    name?: string // Keep for backward compatibility
    passed: boolean
    input?: unknown
    expectedOutput?: unknown
    actualOutput?: unknown
    errorMessage?: string
    message?: string // New field from API
}

export interface RunTestsRequest {
    lessonId: string
    code: string
}

export interface RunCodeRequest {
    language: string
    code: string
}

export interface LessonProgress {
    lessonId: string
    courseId: string
    completed: boolean
    completedAt?: string
    attempts: number
    bestScore?: number
    timeSpent: number
}

export interface Submission {
    id: string
    lessonId: string
    userId: string
    code: string
    language: string
    status: 'pending' | 'passed' | 'failed'
    score?: number
    testResults?: TestResult
    submittedAt: string
    feedback?: string
}

export interface LessonStats {
    totalAttempts: number
    successfulAttempts: number
    averageScore: number
    averageTimeSpent: number
    completionRate: number
}