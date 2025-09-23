import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../../lib/api';
import type {
    Discussion,
    CreateDiscussionRequest,
    UpdateDiscussionRequest, CreateDiscussionReplyRequest,
    UpdateDiscussionReplyRequest
} from '../../types/discussion';

// Discussion queries
export const useDiscussions = (lessonId?: string, courseId?: string, page = 1, pageSize = 20) => {
    const endpoint = lessonId
        ? `/discussions/lesson/${lessonId}`
        : courseId
            ? `/courses/${courseId}/coursediscussions`
            : null;

    return useQuery({
        queryKey: ['discussions', lessonId, courseId, page, pageSize],
        queryFn: async () => {
            if (!endpoint) throw new Error('Either lessonId or courseId must be provided');
            const response = await api.get(endpoint, { params: { page, pageSize } });
            return response.data.data as Discussion[];
        },
        enabled: !!endpoint,
    });
};

export const useDiscussion = (id: string, lessonId?: string, courseId?: string) => {
    const endpoint = lessonId
        ? `/discussions/${id}`
        : courseId
            ? `/courses/${courseId}/coursediscussions/${id}`
            : `/discussions/${id}`;

    return useQuery({
        queryKey: ['discussion', id],
        queryFn: async () => {
            const response = await api.get(endpoint);
            return response.data.data as Discussion;
        },
        enabled: !!id,
    });
};

export const useDiscussionCount = (lessonId?: string, courseId?: string) => {
    const endpoint = lessonId
        ? `/discussions/lesson/${lessonId}/count`
        : courseId
            ? `/courses/${courseId}/coursediscussions/count`
            : null;

    return useQuery({
        queryKey: ['discussionCount', lessonId, courseId],
        queryFn: async () => {
            if (!endpoint) throw new Error('Either lessonId or courseId must be provided');
            try {
                const response = await api.get(endpoint);
                return response.data.data as number;
            } catch (error) {
                // If no discussions found (404), return 0
                if (error && typeof error === 'object' && 'response' in error) {
                    const axiosError = error as { response: { status: number } }
                    if (axiosError.response?.status === 404) {
                        return 0;
                    }
                }
                throw error;
            }
        },
        enabled: !!endpoint,
    });
};

export const usePinnedDiscussions = (lessonId?: string, courseId?: string) => {
    const endpoint = lessonId
        ? `/discussions/lesson/${lessonId}/pinned`
        : null; // Course pinned discussions would need to be implemented

    return useQuery({
        queryKey: ['pinnedDiscussions', lessonId, courseId],
        queryFn: async () => {
            if (!endpoint) throw new Error('Pinned discussions only supported for lessons currently');
            const response = await api.get(endpoint);
            return response.data.data as Discussion[];
        },
        enabled: !!endpoint,
    });
};

// Discussion mutations
export const useCreateDiscussion = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (data: CreateDiscussionRequest) => {
            const endpoint = data.lessonId
                ? '/discussions'
                : data.courseId
                    ? `/courses/${data.courseId}/coursediscussions`
                    : '/discussions';

            const response = await api.post(endpoint, data);
            return response.data.data as Discussion;
        },
        onSuccess: (_, variables) => {
            // Invalidate relevant queries
            if (variables.lessonId) {
                queryClient.invalidateQueries({ queryKey: ['discussions', variables.lessonId] });
                queryClient.invalidateQueries({ queryKey: ['discussionCount', variables.lessonId] });
            }
            if (variables.courseId) {
                queryClient.invalidateQueries({ queryKey: ['discussions', undefined, variables.courseId] });
                queryClient.invalidateQueries({ queryKey: ['discussionCount', undefined, variables.courseId] });
            }
        },
    });
};

export const useUpdateDiscussion = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({ id, data, lessonId, courseId }: {
            id: string;
            data: UpdateDiscussionRequest;
            lessonId?: string;
            courseId?: string;
        }) => {
            const endpoint = lessonId
                ? `/discussions/${id}`
                : courseId
                    ? `/courses/${courseId}/coursediscussions/${id}`
                    : `/discussions/${id}`;

            const response = await api.put(endpoint, data);
            return response.data.data as Discussion;
        },
        onSuccess: (result, variables) => {
            // Update the specific discussion in cache
            queryClient.setQueryData(['discussion', variables.id], result);

            // Invalidate list queries
            if (variables.lessonId) {
                queryClient.invalidateQueries({ queryKey: ['discussions', variables.lessonId] });
            }
            if (variables.courseId) {
                queryClient.invalidateQueries({ queryKey: ['discussions', undefined, variables.courseId] });
            }
        },
    });
};

export const useDeleteDiscussion = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({ id, lessonId, courseId }: {
            id: string;
            lessonId?: string;
            courseId?: string;
        }) => {
            const endpoint = lessonId
                ? `/discussions/${id}`
                : courseId
                    ? `/courses/${courseId}/coursediscussions/${id}`
                    : `/discussions/${id}`;

            const response = await api.delete(endpoint);
            return response.data.data as boolean;
        },
        onSuccess: (_, variables) => {
            // Remove from cache
            queryClient.removeQueries({ queryKey: ['discussion', variables.id] });

            // Invalidate list queries
            if (variables.lessonId) {
                queryClient.invalidateQueries({ queryKey: ['discussions', variables.lessonId] });
                queryClient.invalidateQueries({ queryKey: ['discussionCount', variables.lessonId] });
            }
            if (variables.courseId) {
                queryClient.invalidateQueries({ queryKey: ['discussions', undefined, variables.courseId] });
                queryClient.invalidateQueries({ queryKey: ['discussionCount', undefined, variables.courseId] });
            }
        },
    });
};

// Like/Unlike mutations
export const useLikeDiscussion = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (discussionId: string) => {
            const response = await api.post(`/discussions/${discussionId}/like`);
            return response.data.data as boolean;
        },
        onSuccess: (_, discussionId) => {
            // Invalidate discussion queries to refresh like count and status
            queryClient.invalidateQueries({ queryKey: ['discussion', discussionId] });
            queryClient.invalidateQueries({ queryKey: ['discussions'] });
        },
    });
};

export const useUnlikeDiscussion = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (discussionId: string) => {
            const response = await api.delete(`/discussions/${discussionId}/like`);
            return response.data.data as boolean;
        },
        onSuccess: (_, discussionId) => {
            // Invalidate discussion queries to refresh like count and status
            queryClient.invalidateQueries({ queryKey: ['discussion', discussionId] });
            queryClient.invalidateQueries({ queryKey: ['discussions'] });
        },
    });
};

// Discussion reply mutations
export const useCreateDiscussionReply = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({
            discussionId,
            data,
        }: {
            discussionId: string;
            data: CreateDiscussionReplyRequest;
        }) => {
            const response = await api.post(`/discussions/${discussionId}/replies`, data);
            return response.data.data;
        },
        onSuccess: (_, variables) => {
            queryClient.invalidateQueries({ queryKey: ['discussion', variables.discussionId] });
            queryClient.invalidateQueries({ queryKey: ['discussions'] });
        },
    });
};

export const useUpdateDiscussionReply = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({
            replyId,
            data,
            discussionId,
        }: {
            replyId: string;
            data: UpdateDiscussionReplyRequest;
            discussionId: string;
        }) => {
            const response = await api.put(`/discussions/replies/${replyId}`, data);
            return response.data.data;
        },
        onSuccess: (_, variables) => {
            queryClient.invalidateQueries({ queryKey: ['discussion', variables.discussionId] });
            queryClient.invalidateQueries({ queryKey: ['discussions'] });
        },
    });
};

export const useDeleteDiscussionReply = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({
            replyId,
            discussionId,
        }: {
            replyId: string;
            discussionId: string;
        }) => {
            const response = await api.delete(`/discussions/replies/${replyId}`);
            return response.data.data as boolean;
        },
        onSuccess: (_, variables) => {
            queryClient.invalidateQueries({ queryKey: ['discussion', variables.discussionId] });
            queryClient.invalidateQueries({ queryKey: ['discussions'] });
        },
    });
};