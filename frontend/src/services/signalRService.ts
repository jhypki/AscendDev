import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import type { Notification } from '../types/notification'

class SignalRService {
    private connection: HubConnection | null = null
    private listeners: Map<string, ((notification: Notification) => void)[]> = new Map()

    async connect(token: string): Promise<void> {
        if (this.connection?.state === 'Connected') {
            return
        }

        this.connection = new HubConnectionBuilder()
            .withUrl(`${import.meta.env.VITE_API_URL || 'http://localhost:5171'}/hubs/notifications`, {
                accessTokenFactory: () => token,
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build()

        // Set up event handlers
        this.connection.on('ReceiveNotification', (notification: Notification) => {
            this.notifyListeners('notification', notification)
        })

        this.connection.onreconnecting(() => {
            console.log('SignalR: Reconnecting...')
        })

        this.connection.onreconnected(() => {
            console.log('SignalR: Reconnected')
        })

        this.connection.onclose(() => {
            console.log('SignalR: Connection closed')
        })

        try {
            await this.connection.start()
            console.log('SignalR: Connected')
        } catch (error) {
            console.error('SignalR: Connection failed', error)
            throw error
        }
    }

    async disconnect(): Promise<void> {
        if (this.connection) {
            await this.connection.stop()
            this.connection = null
            this.listeners.clear()
        }
    }

    onNotification(callback: (notification: Notification) => void): () => void {
        const eventName = 'notification'
        if (!this.listeners.has(eventName)) {
            this.listeners.set(eventName, [])
        }
        this.listeners.get(eventName)!.push(callback)

        // Return unsubscribe function
        return () => {
            const callbacks = this.listeners.get(eventName)
            if (callbacks) {
                const index = callbacks.indexOf(callback)
                if (index > -1) {
                    callbacks.splice(index, 1)
                }
            }
        }
    }

    private notifyListeners(eventName: string, data: Notification): void {
        const callbacks = this.listeners.get(eventName)
        if (callbacks) {
            callbacks.forEach(callback => callback(data))
        }
    }

    isConnected(): boolean {
        return this.connection?.state === 'Connected'
    }
}

export const signalRService = new SignalRService()