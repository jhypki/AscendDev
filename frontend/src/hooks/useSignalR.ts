import { useEffect, useCallback } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { notifications } from '@mantine/notifications'
import { useSelector } from 'react-redux'
import { signalRService } from '../services/signalRService'
import type { RootState } from '../store'
import type { Notification } from '../types/notification'

export function useSignalR() {
    const queryClient = useQueryClient()
    const token = useSelector((state: RootState) => state.auth.token)
    const isAuthenticated = useSelector((state: RootState) => state.auth.isAuthenticated)

    const handleNotification = useCallback((notification: Notification) => {
        // Show toast notification
        notifications.show({
            title: notification.title,
            message: notification.message,
            color: notification.type === 'CodeReview' ? 'blue' : 'gray',
            autoClose: 5000,
        })

        // Invalidate notification queries to refresh the UI
        queryClient.invalidateQueries({ queryKey: ['notifications'] })
    }, [queryClient])

    useEffect(() => {
        if (!isAuthenticated || !token) {
            signalRService.disconnect()
            return
        }

        const connectSignalR = async () => {
            try {
                await signalRService.connect(token)

                // Subscribe to notifications
                const unsubscribe = signalRService.onNotification(handleNotification)

                return unsubscribe
            } catch (error) {
                console.error('Failed to connect to SignalR:', error)
            }
        }

        const unsubscribePromise = connectSignalR()

        return () => {
            unsubscribePromise.then(unsubscribe => {
                if (unsubscribe) {
                    unsubscribe()
                }
            })
        }
    }, [isAuthenticated, token, handleNotification])

    useEffect(() => {
        // Cleanup on unmount
        return () => {
            signalRService.disconnect()
        }
    }, [])

    return {
        isConnected: signalRService.isConnected(),
    }
}