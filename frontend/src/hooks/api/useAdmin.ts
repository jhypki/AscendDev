import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../../lib/api'

export interface AdminStats {
    totalUsers: number
    activeUsers: number
    newRegistrations: number
    totalCourses: number
    publishedCourses: number
    totalLessons: number
    systemHealth: 'healthy' | 'warning' | 'critical'
    serverUptime: number
}

export interface UserManagement {
    id: string
    email: string
    firstName: string
    lastName: string
    roles: string[]
    isActive: boolean
    lastLogin: string
    createdAt: string
    coursesEnrolled: number
    lessonsCompleted: number
}

export interface CourseAnalytics {
    courseId: string
    title: string
    enrollments: number
    completions: number
    averageRating: number
    totalLessons: number
    language: string
    status: string
    createdAt: string
}

export interface SystemAnalytics {
    userGrowth: Array<{
        date: string
        users: number
        courses: number
        lessons: number
    }>
    topCourses: Array<{
        name: string
        enrollments: number
    }>
}

export interface PaginatedUserResponse {
    users: UserManagement[]
    totalCount: number
    totalPages: number
    currentPage: number
    pageSize: number
}

// Get admin dashboard statistics
export const useAdminStats = () => {
    return useQuery({
        queryKey: ['admin', 'stats'],
        queryFn: async (): Promise<AdminStats> => {
            const response = await api.get('/admin/stats')
            return response.data
        },
        staleTime: 2 * 60 * 1000, // 2 minutes
    })
}

// Get user management data
export const useUserManagement = (page: number = 1, pageSize: number = 10, search?: string) => {
    return useQuery({
        queryKey: ['admin', 'users', page, pageSize, search],
        queryFn: async (): Promise<PaginatedUserResponse> => {
            const params = new URLSearchParams({
                page: page.toString(),
                pageSize: pageSize.toString(),
            })

            if (search) {
                params.append('search', search)
            }

            const response = await api.get(`/admin/users?${params.toString()}`)
            return response.data
        },
        staleTime: 5 * 60 * 1000, // 5 minutes
    })
}

// Get course analytics
export const useCourseAnalytics = () => {
    return useQuery({
        queryKey: ['admin', 'course-analytics'],
        queryFn: async (): Promise<CourseAnalytics[]> => {
            const response = await api.get('/admin/course-analytics')
            return response.data
        },
        staleTime: 10 * 60 * 1000, // 10 minutes
    })
}

// Update user status
export const useUpdateUserStatus = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({ userId, isActive }: { userId: string, isActive: boolean }) => {
            const response = await api.put(`/admin/users/${userId}/status`, { isActive })
            return response.data
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin', 'users'] })
        },
    })
}

// Update user roles
export const useUpdateUserRoles = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async ({ userId, roles }: { userId: string, roles: string[] }) => {
            const response = await api.put(`/admin/users/${userId}/roles`, { roles })
            return response.data
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin', 'users'] })
        },
    })
}

// Get system analytics
export const useSystemAnalytics = () => {
    return useQuery({
        queryKey: ['admin', 'system-analytics'],
        queryFn: async (): Promise<SystemAnalytics> => {
            const response = await api.get('/admin/system-analytics')
            return response.data
        },
        staleTime: 15 * 60 * 1000, // 15 minutes
    })
}