import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../../lib/api'
import type {
    CodeReview,
    CodeReviewComment,
    CreateCodeReviewRequest,
    UpdateCodeReviewRequest,
    CreateCodeReviewCommentRequest,
    UpdateCodeReviewCommentRequest
} from '../../types/submission'

// Get code reviews for a lesson
export const useCodeReviewsByLesson = (lessonId: string, page = 1, pageSize = 20) => {
    return useQuery({
        queryKey: ['code-reviews', 'lesson', lessonId, page, pageSize],
        queryFn: async (): Promise<CodeReview[]> => {
            const response = await api.get(`/codereviews/lesson/${lessonId}?page=${page}&pageSize=${pageSize}`)
            return response.data
        },
        enabled: !!lessonId,
    })
}

// Get code reviews where current user is the reviewer
export const useMyReviews = (page = 1, pageSize = 20) => {
    return useQuery({
        queryKey: ['code-reviews', 'my-reviews', page, pageSize],
        queryFn: async (): Promise<CodeReview[]> => {
            const response = await api.get(`/codereviews/my-reviews?page=${page}&pageSize=${pageSize}`)
            return response.data
        },
    })
}

// Get code reviews where current user is the reviewee (submissions being reviewed)
export const useMySubmissionsUnderReview = (page = 1, pageSize = 20) => {
    return useQuery({
        queryKey: ['code-reviews', 'my-submissions', page, pageSize],
        queryFn: async (): Promise<CodeReview[]> => {
            const response = await api.get(`/codereviews/my-submissions?page=${page}&pageSize=${pageSize}`)
            return response.data
        },
    })
}

// Get pending code reviews (available for review)
export const usePendingReviews = (page = 1, pageSize = 20) => {
    return useQuery({
        queryKey: ['code-reviews', 'pending', page, pageSize],
        queryFn: async (): Promise<CodeReview[]> => {
            const response = await api.get(`/codereviews/pending?page=${page}&pageSize=${pageSize}`)
            return response.data
        },
    })
}

// Get a specific code review
export const useCodeReview = (codeReviewId: string) => {
    return useQuery({
        queryKey: ['code-reviews', codeReviewId],
        queryFn: async (): Promise<CodeReview> => {
            const response = await api.get(`/codereviews/${codeReviewId}`)
            return response.data
        },
        enabled: !!codeReviewId,
    })
}

// Get existing code review for a submission by current user
export const useMyCodeReviewForSubmission = (submissionId: number) => {
    return useQuery({
        queryKey: ['code-reviews', 'submission', submissionId, 'my-review'],
        queryFn: async (): Promise<CodeReview | null> => {
            try {
                const response = await api.get(`/codereviews/submission/${submissionId}/my-review`)
                return response.data
            } catch (error) {
                if (error && typeof error === 'object' && 'response' in error) {
                    const axiosError = error as { response?: { status?: number } }
                    if (axiosError.response?.status === 404) {
                        return null
                    }
                }
                throw error
            }
        },
        enabled: !!submissionId,
    })
}

// Get all code reviews for a submission (for viewing comments from all reviewers)
export const useCodeReviewsForSubmission = (submissionId: number) => {
    return useQuery({
        queryKey: ['code-reviews', 'submission', submissionId, 'all'],
        queryFn: async (): Promise<CodeReview[]> => {
            const response = await api.get(`/codereviews/submission/${submissionId}/all`)
            return response.data
        },
        enabled: !!submissionId,
    })
}

// Create a new code review
export const useCreateCodeReview = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (request: CreateCodeReviewRequest): Promise<CodeReview> => {
            const response = await api.post('/codereviews', request)
            return response.data
        },
        onSuccess: () => {
            // Invalidate related queries
            queryClient.invalidateQueries({ queryKey: ['code-reviews'] })
        },
    })
}

// Update a code review
export const useUpdateCodeReview = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({ id, request }: { id: string; request: UpdateCodeReviewRequest }): Promise<CodeReview> => {
            const response = await api.put(`/codereviews/${id}`, request)
            return response.data
        },
        onSuccess: (_, { id }) => {
            // Invalidate related queries
            queryClient.invalidateQueries({ queryKey: ['code-reviews'] })
            queryClient.invalidateQueries({ queryKey: ['code-reviews', id] })
        },
    })
}

// Delete a code review
export const useDeleteCodeReview = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (codeReviewId: string): Promise<void> => {
            await api.delete(`/codereviews/${codeReviewId}`)
        },
        onSuccess: () => {
            // Invalidate related queries
            queryClient.invalidateQueries({ queryKey: ['code-reviews'] })
        },
    })
}

// Get comments for a code review
export const useCodeReviewComments = (codeReviewId: string) => {
    return useQuery({
        queryKey: ['code-reviews', codeReviewId, 'comments'],
        queryFn: async (): Promise<CodeReviewComment[]> => {
            const response = await api.get(`/codereviews/${codeReviewId}/comments`)
            return response.data
        },
        enabled: !!codeReviewId,
    })
}

// Get all comments for a submission (from all code reviews)
export const useAllCommentsForSubmission = (submissionId: number) => {
    return useQuery({
        queryKey: ['code-reviews', 'submission', submissionId, 'all-comments'],
        queryFn: async (): Promise<CodeReviewComment[]> => {
            const response = await api.get(`/codereviews/submission/${submissionId}/comments`)
            return response.data
        },
        enabled: !!submissionId,
    })
}

// Create a comment on a code review
export const useCreateCodeReviewComment = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({
            codeReviewId,
            request
        }: {
            codeReviewId: string;
            request: CreateCodeReviewCommentRequest
        }): Promise<CodeReviewComment> => {
            const response = await api.post(`/codereviews/${codeReviewId}/comments`, request)
            return response.data
        },
        onSuccess: (_, { codeReviewId }) => {
            // Invalidate related queries
            queryClient.invalidateQueries({ queryKey: ['code-reviews', codeReviewId, 'comments'] })
            queryClient.invalidateQueries({ queryKey: ['code-reviews', codeReviewId] })

            // Get the submission ID from the code review to invalidate submission-specific queries
            const codeReviewData = queryClient.getQueryData(['code-reviews', codeReviewId]) as CodeReview | undefined
            if (codeReviewData?.submissionId) {
                // Invalidate the all-comments query for the submission
                queryClient.invalidateQueries({ queryKey: ['code-reviews', 'submission', codeReviewData.submissionId, 'all-comments'] })
                // Also invalidate all code reviews for the submission to update comment counts
                queryClient.invalidateQueries({ queryKey: ['code-reviews', 'submission', codeReviewData.submissionId, 'all'] })
                queryClient.invalidateQueries({ queryKey: ['code-reviews', 'my-submissions'] })
            }

            // Fallback: invalidate all submission-related queries if we can't get specific submission ID
            queryClient.invalidateQueries({ queryKey: ['code-reviews', 'submission'] })
        },
    })
}

// Update a comment
export const useUpdateCodeReviewComment = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({
            codeReviewId,
            commentId,
            request
        }: {
            codeReviewId: string;
            commentId: string;
            request: UpdateCodeReviewCommentRequest
        }): Promise<CodeReviewComment> => {
            const response = await api.put(`/codereviews/${codeReviewId}/comments/${commentId}`, request)
            return response.data
        },
        onSuccess: (_, { codeReviewId }) => {
            // Invalidate related queries
            queryClient.invalidateQueries({ queryKey: ['code-reviews', codeReviewId, 'comments'] })
        },
    })
}

// Delete a comment
export const useDeleteCodeReviewComment = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({
            codeReviewId,
            commentId
        }: {
            codeReviewId: string;
            commentId: string
        }): Promise<void> => {
            await api.delete(`/codereviews/${codeReviewId}/comments/${commentId}`)
        },
        onSuccess: (_, { codeReviewId }) => {
            // Invalidate related queries
            queryClient.invalidateQueries({ queryKey: ['code-reviews', codeReviewId, 'comments'] })
            queryClient.invalidateQueries({ queryKey: ['code-reviews', codeReviewId] })

            // Get the submission ID from the code review to invalidate submission-specific queries
            const codeReviewData = queryClient.getQueryData(['code-reviews', codeReviewId]) as CodeReview | undefined
            if (codeReviewData?.submissionId) {
                // Invalidate the all-comments query for the submission
                queryClient.invalidateQueries({ queryKey: ['code-reviews', 'submission', codeReviewData.submissionId, 'all-comments'] })
                // Also invalidate all code reviews for the submission to update comment counts
                queryClient.invalidateQueries({ queryKey: ['code-reviews', 'submission', codeReviewData.submissionId, 'all'] })
                queryClient.invalidateQueries({ queryKey: ['code-reviews', 'my-submissions'] })
            }

            // Fallback: invalidate all submission-related queries if we can't get specific submission ID
            queryClient.invalidateQueries({ queryKey: ['code-reviews', 'submission'] })
        },
    })
}

// Resolve a comment
export const useResolveCodeReviewComment = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({
            codeReviewId,
            commentId
        }: {
            codeReviewId: string;
            commentId: string
        }): Promise<void> => {
            await api.patch(`/codereviews/${codeReviewId}/comments/${commentId}/resolve`)
        },
        onSuccess: (_, { codeReviewId }) => {
            // Invalidate related queries
            queryClient.invalidateQueries({ queryKey: ['code-reviews', codeReviewId, 'comments'] })
            queryClient.invalidateQueries({ queryKey: ['code-reviews', codeReviewId] })
        },
    })
}