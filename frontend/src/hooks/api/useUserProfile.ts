import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../../lib/api'
import type {
    UserProfile,
    PublicUserProfile,
    UpdateUserProfileRequest,
    UserProfileSettings,
    ActivityFeedRequest,
    ActivityFeedResponse,
    UserSearchRequest,
    UserSearchResponse
} from '../../types/profile'

// Get current user's profile
export const useMyProfile = () => {
    return useQuery({
        queryKey: ['profile', 'me'],
        queryFn: async (): Promise<UserProfile> => {
            const response = await api.get('/userprofile/me')
            return response.data.data
        },
    })
}

// Get user profile by ID
export const useUserProfile = (userId: string, enabled = true) => {
    return useQuery({
        queryKey: ['profile', userId],
        queryFn: async (): Promise<PublicUserProfile> => {
            const response = await api.get(`/userprofile/${userId}`)
            return response.data.data
        },
        enabled: enabled && !!userId,
    })
}

// Get user profile by username
export const useUserProfileByUsername = (username: string, enabled = true) => {
    return useQuery({
        queryKey: ['profile', 'username', username],
        queryFn: async (): Promise<PublicUserProfile> => {
            const response = await api.get(`/userprofile/username/${username}`)
            return response.data.data
        },
        enabled: enabled && !!username,
    })
}

// Update current user's profile
export const useUpdateProfile = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (profileData: UpdateUserProfileRequest): Promise<UserProfile> => {
            const response = await api.put('/userprofile/me', profileData)
            return response.data.data
        },
        onSuccess: (data) => {
            // Update the profile cache
            queryClient.setQueryData(['profile', 'me'], data)
            // Also update auth user data if it exists
            queryClient.invalidateQueries({ queryKey: ['user'] })
        },
    })
}

// Get activity feed
export const useActivityFeed = (request: ActivityFeedRequest = {}) => {
    return useQuery({
        queryKey: ['profile', 'activity', request],
        queryFn: async (): Promise<ActivityFeedResponse> => {
            const response = await api.post('/userprofile/activity', request)
            return response.data.data
        },
    })
}

// Search users
export const useUserSearch = () => {
    return useMutation({
        mutationFn: async (searchRequest: UserSearchRequest): Promise<UserSearchResponse> => {
            const response = await api.post('/userprofile/search', searchRequest)
            return response.data.data
        },
    })
}

// Update profile privacy settings
export const useUpdateProfileSettings = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (settings: UserProfileSettings): Promise<void> => {
            await api.put('/usersettings/privacy', settings)
        },
        onSuccess: () => {
            // Invalidate profile data to refetch with new privacy settings
            queryClient.invalidateQueries({ queryKey: ['profile'] })
        },
    })
}

// Get user achievements
export const useUserAchievements = (userId?: string) => {
    return useQuery({
        queryKey: ['profile', 'achievements', userId || 'me'],
        queryFn: async () => {
            const endpoint = userId ? `/userprofile/${userId}/achievements` : '/userprofile/me/achievements'
            const response = await api.get(endpoint)
            return response.data.data
        },
    })
}

// Get user statistics
export const useUserStatistics = (userId?: string) => {
    return useQuery({
        queryKey: ['profile', 'statistics', userId || 'me'],
        queryFn: async () => {
            const endpoint = userId ? `/userprofile/${userId}/statistics` : '/userprofile/me/statistics'
            const response = await api.get(endpoint)
            return response.data.data
        },
    })
}