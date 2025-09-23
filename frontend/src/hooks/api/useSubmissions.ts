import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../../lib/api'
import type { PublicSubmission, Submission } from '../../types/submission'

// Get public submissions for a lesson
export const usePublicSubmissionsForLesson = (lessonId: string, limit = 50) => {
    return useQuery({
        queryKey: ['submissions', 'lesson', lessonId, 'public', limit],
        queryFn: async (): Promise<PublicSubmission[]> => {
            const response = await api.get(`/submissions/lesson/${lessonId}/public?limit=${limit}`)
            return response.data
        },
        enabled: !!lessonId,
    })
}

// Get submissions for review for a lesson
export const useSubmissionsForReview = (lessonId: string, limit = 50) => {
    return useQuery({
        queryKey: ['submissions', 'lesson', lessonId, 'for-review', limit],
        queryFn: async (): Promise<PublicSubmission[]> => {
            const response = await api.get(`/submissions/lesson/${lessonId}/for-review?limit=${limit}`)
            return response.data
        },
        enabled: !!lessonId,
    })
}

// Get a specific submission for review
export const useSubmissionForReview = (submissionId: number) => {
    return useQuery({
        queryKey: ['submissions', submissionId, 'for-review'],
        queryFn: async (): Promise<PublicSubmission> => {
            const response = await api.get(`/submissions/${submissionId}/for-review`)
            return response.data
        },
        enabled: !!submissionId,
    })
}

// Get current user's submissions
export const useMySubmissions = () => {
    return useQuery({
        queryKey: ['submissions', 'my'],
        queryFn: async (): Promise<Submission[]> => {
            const response = await api.get('/submissions/my')
            return response.data
        },
    })
}

// Get current user's submission statistics
export const useMySubmissionStats = () => {
    return useQuery({
        queryKey: ['submissions', 'my', 'stats'],
        queryFn: async (): Promise<{
            totalSubmissions: number
            passedSubmissions: number
            successRate: number
        }> => {
            const response = await api.get('/submissions/my/stats')
            return response.data
        },
    })
}

// Get a specific submission
export const useSubmission = (submissionId: number) => {
    return useQuery({
        queryKey: ['submissions', submissionId],
        queryFn: async (): Promise<Submission> => {
            const response = await api.get(`/submissions/${submissionId}`)
            return response.data
        },
        enabled: !!submissionId,
    })
}

// Delete a submission
export const useDeleteSubmission = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (submissionId: number): Promise<void> => {
            await api.delete(`/submissions/${submissionId}`)
        },
        onSuccess: () => {
            // Invalidate related queries
            queryClient.invalidateQueries({ queryKey: ['submissions'] })
        },
    })
}

// Delete multiple submissions
export const useBulkDeleteSubmissions = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (submissionIds: number[]): Promise<{ deletedCount: number }> => {
            const response = await api.delete('/submissions/bulk', { data: submissionIds })
            return response.data
        },
        onSuccess: () => {
            // Invalidate related queries
            queryClient.invalidateQueries({ queryKey: ['submissions'] })
        },
    })
}