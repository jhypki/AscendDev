import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../../lib/api'
import type { Lesson } from '../../types/course'
import type {
    CodeExecutionResult,
    TestResult,
    RunTestsRequest,
    RunCodeRequest,
    LessonProgress,
    Submission,
    LessonStats
} from '../../types/lesson'

// Query keys for better cache management
export const lessonKeys = {
    all: ['lessons'] as const,
    lists: () => [...lessonKeys.all, 'list'] as const,
    list: (courseId: string) => [...lessonKeys.lists(), courseId] as const,
    details: () => [...lessonKeys.all, 'detail'] as const,
    detail: (courseId: string, lessonId: string) => [...lessonKeys.details(), courseId, lessonId] as const,
    progress: (courseId: string) => [...lessonKeys.all, 'progress', courseId] as const,
    submissions: (lessonId: string) => [...lessonKeys.all, 'submissions', lessonId] as const,
    stats: (lessonId: string) => [...lessonKeys.all, 'stats', lessonId] as const,
}

// Get a specific lesson
export const useLesson = (courseId: string, lessonId: string, enabled: boolean = true) => {
    return useQuery({
        queryKey: lessonKeys.detail(courseId, lessonId),
        queryFn: async (): Promise<Lesson> => {
            const response = await api.get(`/courses/${courseId}/lessons/${lessonId}`)
            return response.data
        },
        enabled: enabled && !!courseId && !!lessonId,
        staleTime: 5 * 60 * 1000, // 5 minutes
    })
}

// Get lessons for a course (ordered)
export const useCourseLessonsOrdered = (courseId: string, enabled: boolean = true) => {
    return useQuery({
        queryKey: lessonKeys.list(courseId),
        queryFn: async (): Promise<Lesson[]> => {
            const response = await api.get(`/courses/${courseId}/lessons/ordered`)
            return response.data
        },
        enabled: enabled && !!courseId,
        staleTime: 5 * 60 * 1000, // 5 minutes
    })
}

// Get course progress
export const useCourseProgress = (courseId: string, enabled: boolean = true) => {
    return useQuery({
        queryKey: lessonKeys.progress(courseId),
        queryFn: async (): Promise<LessonProgress[]> => {
            const response = await api.get(`/courses/${courseId}/progress`)
            return response.data
        },
        enabled: enabled && !!courseId,
        staleTime: 2 * 60 * 1000, // 2 minutes
    })
}

// Run code execution
export const useRunCode = () => {
    return useMutation({
        mutationFn: async (request: RunCodeRequest): Promise<CodeExecutionResult> => {
            const response = await api.post('/codeexecution/run', request)
            return response.data
        },
    })
}

// Run tests for a lesson
export const useRunTests = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (request: RunTestsRequest): Promise<TestResult> => {
            const response = await api.post('/tests/run', request)
            return response.data
        },
        onSuccess: (_, variables) => {
            // Invalidate progress queries when tests are run
            queryClient.invalidateQueries({
                queryKey: lessonKeys.progress(variables.lessonId)
            })
        },
    })
}

// Complete a lesson
export const useCompleteLesson = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({ courseId, lessonId }: { courseId: string; lessonId: string }): Promise<void> => {
            await api.post(`/courses/${courseId}/lessons/${lessonId}/complete`)
        },
        onSuccess: (_, variables) => {
            // Invalidate progress queries
            queryClient.invalidateQueries({
                queryKey: lessonKeys.progress(variables.courseId)
            })
        },
    })
}

// Get lesson submissions
export const useLessonSubmissions = (lessonId: string, enabled: boolean = true) => {
    return useQuery({
        queryKey: lessonKeys.submissions(lessonId),
        queryFn: async (): Promise<Submission[]> => {
            const response = await api.get(`/submissions/lesson/${lessonId}/public`)
            return response.data
        },
        enabled: enabled && !!lessonId,
        staleTime: 2 * 60 * 1000, // 2 minutes
    })
}

// Get user's submissions
export const useMySubmissions = (enabled: boolean = true) => {
    return useQuery({
        queryKey: ['submissions', 'my'],
        queryFn: async (): Promise<Submission[]> => {
            const response = await api.get('/submissions/my')
            return response.data
        },
        enabled,
        staleTime: 2 * 60 * 1000, // 2 minutes
    })
}

// Save user code for a lesson
export const useSaveUserCode = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({ courseId, lessonId, code }: { courseId: string; lessonId: string; code: string }) => {
            const response = await api.post(`/courses/${courseId}/lessons/${lessonId}/user-code`, { code })
            return response.data
        },
        onSuccess: (_, variables) => {
            // Update the cache directly instead of invalidating to prevent refetch
            queryClient.setQueryData(['userCode', variables.courseId, variables.lessonId], {
                code: variables.code,
                updatedAt: new Date().toISOString()
            })
        },
    })
}

// Get user's saved code for a lesson
export const useUserCode = (courseId: string, lessonId: string, enabled: boolean = true) => {
    return useQuery({
        queryKey: ['userCode', courseId, lessonId],
        queryFn: async () => {
            const response = await api.get(`/courses/${courseId}/lessons/${lessonId}/user-code`)
            return response.data
        },
        enabled: enabled && !!courseId && !!lessonId,
        retry: false, // Don't retry if user code doesn't exist (404)
        staleTime: 0, // Always fetch fresh data
    })
}

// Reset user code for a lesson (delete saved code)
export const useResetUserCode = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({ courseId, lessonId }: { courseId: string; lessonId: string }) => {
            await api.delete(`/courses/${courseId}/lessons/${lessonId}/user-code`)
        },
        onSuccess: (_, variables) => {
            // Invalidate the user code query
            queryClient.invalidateQueries({
                queryKey: ['userCode', variables.courseId, variables.lessonId]
            })
        },
    })
}

// Get lesson statistics
export const useLessonStats = (lessonId: string, enabled: boolean = true) => {
    return useQuery({
        queryKey: lessonKeys.stats(lessonId),
        queryFn: async (): Promise<LessonStats> => {
            const response = await api.get(`/lessons/${lessonId}/stats`)
            return response.data
        },
        enabled: enabled && !!lessonId,
        staleTime: 10 * 60 * 1000, // 10 minutes
    })
}