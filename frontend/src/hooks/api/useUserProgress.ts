import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../../lib/api';

export interface CourseProgress {
    courseId: string;
    userId: string;
    totalLessons: number;
    completedLessons: number;
    completionPercentage: number;
    completedLessonIds: string[];
}

export interface MarkLessonCompletedRequest {
    codeSolution?: string;
}

export interface MarkLessonCompletedResponse {
    lessonId: string;
    completedAt: string;
    message: string;
}

// Get course progress for the current user
export const useCourseProgress = (courseId: string) => {
    return useQuery({
        queryKey: ['courseProgress', courseId],
        queryFn: async (): Promise<CourseProgress> => {
            const response = await api.get(`/courses/${courseId}/progress`);
            return response.data;
        },
        enabled: !!courseId,
    });
};

// Mark a lesson as completed
export const useMarkLessonCompleted = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({
            courseId,
            lessonId,
            codeSolution,
        }: {
            courseId: string;
            lessonId: string;
            codeSolution?: string;
        }): Promise<MarkLessonCompletedResponse> => {
            const response = await api.post(
                `/courses/${courseId}/lessons/${lessonId}/complete`,
                { codeSolution }
            );
            return response.data;
        },
        onSuccess: (_, { courseId }) => {
            // Invalidate course progress to refetch updated data
            queryClient.invalidateQueries({ queryKey: ['courseProgress', courseId] });
            // Also invalidate courses list in case completion affects course display
            queryClient.invalidateQueries({ queryKey: ['courses'] });
        },
    });
};