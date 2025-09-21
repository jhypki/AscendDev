import { useQuery } from '@tanstack/react-query'
import api from '../../lib/api'

export interface DashboardStats {
    totalCourses: number
    completedCourses: number
    inProgressCourses: number
    totalLessons: number
    completedLessons: number
    streakDays: number
    totalStudyTime: number
    recentActivity: RecentActivity[]
}

export interface RecentActivity {
    id: string
    type: 'lesson_completed' | 'course_started' | 'course_completed'
    title: string
    courseTitle?: string
    timestamp: string
}

export interface UserProgress {
    courseId: string
    courseTitle: string
    totalLessons: number
    completedLessons: number
    completionPercentage: number
    lastAccessed: string
}

export interface LearningStreakData {
    date: string
    completed: number
}

// Get dashboard statistics
export const useDashboardStats = () => {
    return useQuery({
        queryKey: ['dashboard', 'stats'],
        queryFn: async (): Promise<DashboardStats> => {
            const response = await api.get('/dashboard/stats')
            return response.data
        },
        staleTime: 5 * 60 * 1000, // 5 minutes
    })
}

// Get user progress for all courses
export const useUserProgress = () => {
    return useQuery({
        queryKey: ['dashboard', 'progress'],
        queryFn: async (): Promise<UserProgress[]> => {
            const response = await api.get('/dashboard/progress')
            return response.data
        },
        staleTime: 5 * 60 * 1000, // 5 minutes
    })
}

// Get learning streak data
export const useLearningStreak = (days: number = 30) => {
    return useQuery({
        queryKey: ['dashboard', 'streak', days],
        queryFn: async (): Promise<LearningStreakData[]> => {
            const response = await api.get(`/dashboard/streak?days=${days}`)
            return response.data
        },
        staleTime: 10 * 60 * 1000, // 10 minutes
    })
}

// Get recent activity
export const useRecentActivity = (limit: number = 10) => {
    return useQuery({
        queryKey: ['dashboard', 'activity', limit],
        queryFn: async (): Promise<RecentActivity[]> => {
            const response = await api.get(`/dashboard/activity?limit=${limit}`)
            return response.data
        },
        staleTime: 5 * 60 * 1000, // 5 minutes
    })
}