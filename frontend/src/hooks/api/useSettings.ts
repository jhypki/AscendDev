import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../../lib/api'
import type { UserSettings, UpdateUserSettingsRequest, UpdatePrivacySettingsRequest } from '../../types/settings'

// Get user settings
export const useUserSettings = () => {
    return useQuery({
        queryKey: ['userSettings'],
        queryFn: async (): Promise<UserSettings> => {
            const response = await api.get('/UserSettings')
            return response.data
        }
    })
}

// Update user settings
export const useUpdateUserSettings = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (settings: UpdateUserSettingsRequest): Promise<UserSettings> => {
            const response = await api.put('/UserSettings', settings)
            return response.data
        },
        onSuccess: (data) => {
            queryClient.setQueryData(['userSettings'], data)
            queryClient.invalidateQueries({ queryKey: ['userProfile'] })
        }
    })
}

// Update privacy settings
export const useUpdatePrivacySettings = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (settings: UpdatePrivacySettingsRequest): Promise<void> => {
            await api.put('/UserSettings/privacy', settings)
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['userSettings'] })
            queryClient.invalidateQueries({ queryKey: ['userProfile'] })
        }
    })
}

// Delete user settings (reset to defaults)
export const useDeleteUserSettings = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (): Promise<void> => {
            await api.delete('/UserSettings')
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['userSettings'] })
            queryClient.invalidateQueries({ queryKey: ['userProfile'] })
        }
    })
}

// Check if public submissions are enabled for a user
export const usePublicSubmissionsEnabled = (userId: string) => {
    return useQuery({
        queryKey: ['publicSubmissionsEnabled', userId],
        queryFn: async (): Promise<boolean> => {
            const response = await api.get(`/UserSettings/${userId}/public-submissions-enabled`)
            return response.data
        },
        enabled: !!userId
    })
}