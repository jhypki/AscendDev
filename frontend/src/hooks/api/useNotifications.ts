import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../../lib/api'
import type { Notification } from '../../types/notification'

// API functions
const fetchNotifications = async (page = 1, pageSize = 20): Promise<Notification[]> => {
    const response = await api.get(`/notifications?page=${page}&pageSize=${pageSize}`)
    return response.data
}

const fetchUnreadNotifications = async (): Promise<Notification[]> => {
    const response = await api.get('/notifications/unread')
    return response.data
}

const fetchUnreadCount = async (): Promise<number> => {
    const response = await api.get('/notifications/unread/count')
    return response.data
}

const markAsRead = async (id: string): Promise<void> => {
    await api.put(`/notifications/${id}/read`)
}

const markAllAsRead = async (): Promise<void> => {
    await api.put('/notifications/read-all')
}

const deleteNotification = async (id: string): Promise<void> => {
    await api.delete(`/notifications/${id}`)
}

// Hooks
export const useNotifications = (page = 1, pageSize = 20) => {
    return useQuery({
        queryKey: ['notifications', page, pageSize],
        queryFn: () => fetchNotifications(page, pageSize),
    })
}

export const useUnreadNotifications = () => {
    return useQuery({
        queryKey: ['notifications', 'unread'],
        queryFn: fetchUnreadNotifications,
        refetchInterval: 30000, // Refetch every 30 seconds
    })
}

export const useUnreadCount = () => {
    return useQuery({
        queryKey: ['notifications', 'unread', 'count'],
        queryFn: fetchUnreadCount,
        refetchInterval: 30000, // Refetch every 30 seconds
    })
}

export const useMarkAsRead = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: markAsRead,
        onSuccess: () => {
            // Invalidate and refetch notification queries
            queryClient.invalidateQueries({ queryKey: ['notifications'] })
        },
    })
}

export const useMarkAllAsRead = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: markAllAsRead,
        onSuccess: () => {
            // Invalidate and refetch notification queries
            queryClient.invalidateQueries({ queryKey: ['notifications'] })
        },
    })
}

export const useDeleteNotification = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: deleteNotification,
        onSuccess: () => {
            // Invalidate and refetch notification queries
            queryClient.invalidateQueries({ queryKey: ['notifications'] })
        },
    })
}