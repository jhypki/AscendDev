import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../../lib/api'
import type {
    Course,
    Lesson,
    CreateCourseRequest,
    UpdateCourseRequest,
    CourseFilters,
    CoursesResponse
} from '../../types/course'

// Query keys for better cache management
export const courseKeys = {
    all: ['courses'] as const,
    lists: () => [...courseKeys.all, 'list'] as const,
    list: (filters: CourseFilters) => [...courseKeys.lists(), filters] as const,
    details: () => [...courseKeys.all, 'detail'] as const,
    detail: (id: string) => [...courseKeys.details(), id] as const,
    lessons: (courseId: string) => [...courseKeys.detail(courseId), 'lessons'] as const,
}

// Get all courses with optional filtering and pagination
export const useCourses = (
    filters: CourseFilters = {},
    page: number = 1,
    pageSize: number = 12
) => {
    return useQuery({
        queryKey: [...courseKeys.list(filters), page, pageSize],
        queryFn: async (): Promise<CoursesResponse> => {
            const params = new URLSearchParams()

            if (filters.search) params.append('search', filters.search)
            if (filters.language) params.append('language', filters.language)
            if (filters.status) params.append('status', filters.status)
            if (filters.tags?.length) {
                filters.tags.forEach(tag => params.append('tags', tag))
            }
            params.append('page', page.toString())
            params.append('pageSize', pageSize.toString())

            const response = await api.get(`/courses?${params.toString()}`)

            // The API now returns a paginated response directly
            return response.data
        },
        staleTime: 5 * 60 * 1000, // 5 minutes
    })
}

// Get a single course by ID
export const useCourse = (courseId: string, enabled: boolean = true) => {
    return useQuery({
        queryKey: courseKeys.detail(courseId),
        queryFn: async (): Promise<Course> => {
            const response = await api.get(`/courses/${courseId}`)
            return response.data
        },
        enabled: enabled && !!courseId,
        staleTime: 5 * 60 * 1000, // 5 minutes
    })
}

// Get lessons for a specific course
export const useCourseLessons = (courseId: string, enabled: boolean = true) => {
    return useQuery({
        queryKey: courseKeys.lessons(courseId),
        queryFn: async (): Promise<Lesson[]> => {
            try {
                const response = await api.get(`/courses/${courseId}/lessons`)
                return response.data
            } catch (error) {
                // If no lessons found (404), return empty array instead of throwing error
                if (error && typeof error === 'object' && 'response' in error) {
                    const axiosError = error as { response: { status: number } }
                    if (axiosError.response?.status === 404) {
                        return []
                    }
                }
                throw error
            }
        },
        enabled: enabled && !!courseId,
        staleTime: 5 * 60 * 1000, // 5 minutes
    })
}

// Create a new course
export const useCreateCourse = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (courseData: CreateCourseRequest): Promise<Course> => {
            const response = await api.post('/courses', courseData)
            return response.data
        },
        onSuccess: (newCourse) => {
            // Invalidate and refetch courses list
            queryClient.invalidateQueries({ queryKey: courseKeys.lists() })

            // Add the new course to the cache
            queryClient.setQueryData(courseKeys.detail(newCourse.id), newCourse)
        },
    })
}

// Update an existing course
export const useUpdateCourse = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({
            courseId,
            courseData
        }: {
            courseId: string
            courseData: UpdateCourseRequest
        }): Promise<Course> => {
            const response = await api.put(`/courses/${courseId}`, courseData)
            return response.data
        },
        onSuccess: (updatedCourse) => {
            // Update the specific course in cache
            queryClient.setQueryData(courseKeys.detail(updatedCourse.id), updatedCourse)

            // Invalidate courses list to reflect changes
            queryClient.invalidateQueries({ queryKey: courseKeys.lists() })
        },
    })
}

// Delete a course
export const useDeleteCourse = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (courseId: string): Promise<void> => {
            await api.delete(`/courses/${courseId}`)
        },
        onSuccess: (_, courseId) => {
            // Remove the course from cache
            queryClient.removeQueries({ queryKey: courseKeys.detail(courseId) })

            // Invalidate courses list
            queryClient.invalidateQueries({ queryKey: courseKeys.lists() })
        },
    })
}

// Get popular courses
export const usePopularCourses = (limit: number = 6) => {
    return useQuery({
        queryKey: ['courses', 'popular', limit],
        queryFn: async (): Promise<Course[]> => {
            const response = await api.get(`/courses/popular?limit=${limit}`)
            return response.data
        },
        staleTime: 10 * 60 * 1000, // 10 minutes
    })
}

// Get courses by language
export const useCoursesByLanguage = (language: string) => {
    return useQuery({
        queryKey: ['courses', 'by-language', language],
        queryFn: async (): Promise<Course[]> => {
            const response = await api.get(`/courses?language=${language}`)
            return response.data
        },
        enabled: !!language,
        staleTime: 5 * 60 * 1000, // 5 minutes
    })
}

// Search courses
export const useSearchCourses = (query: string, enabled: boolean = true) => {
    return useQuery({
        queryKey: ['courses', 'search', query],
        queryFn: async (): Promise<Course[]> => {
            const response = await api.get(`/courses/search?q=${encodeURIComponent(query)}`)
            return response.data
        },
        enabled: enabled && query.length > 2,
        staleTime: 2 * 60 * 1000, // 2 minutes
    })
}