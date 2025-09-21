import { createSlice, type PayloadAction } from '@reduxjs/toolkit'

export type ColorScheme = 'light' | 'dark' | 'auto'

export interface UiState {
    colorScheme: ColorScheme
    sidebarOpened: boolean
    loading: boolean
    notifications: Notification[]
    modals: {
        [key: string]: boolean
    }
}

export interface Notification {
    id: string
    type: 'success' | 'error' | 'warning' | 'info'
    title: string
    message?: string
    autoClose?: boolean | number
}

const initialState: UiState = {
    colorScheme: (localStorage.getItem('colorScheme') as ColorScheme) || 'auto',
    sidebarOpened: true,
    loading: false,
    notifications: [],
    modals: {},
}

export const uiSlice = createSlice({
    name: 'ui',
    initialState,
    reducers: {
        setColorScheme: (state, action: PayloadAction<ColorScheme>) => {
            state.colorScheme = action.payload
            localStorage.setItem('colorScheme', action.payload)
        },
        toggleSidebar: (state) => {
            state.sidebarOpened = !state.sidebarOpened
        },
        setSidebarOpened: (state, action: PayloadAction<boolean>) => {
            state.sidebarOpened = action.payload
        },
        setLoading: (state, action: PayloadAction<boolean>) => {
            state.loading = action.payload
        },
        addNotification: (state, action: PayloadAction<Omit<Notification, 'id'>>) => {
            const notification: Notification = {
                ...action.payload,
                id: Date.now().toString() + Math.random().toString(36).substr(2, 9),
            }
            state.notifications.push(notification)
        },
        removeNotification: (state, action: PayloadAction<string>) => {
            state.notifications = state.notifications.filter(
                (notification) => notification.id !== action.payload
            )
        },
        clearNotifications: (state) => {
            state.notifications = []
        },
        openModal: (state, action: PayloadAction<string>) => {
            state.modals[action.payload] = true
        },
        closeModal: (state, action: PayloadAction<string>) => {
            state.modals[action.payload] = false
        },
        toggleModal: (state, action: PayloadAction<string>) => {
            state.modals[action.payload] = !state.modals[action.payload]
        },
    },
})

export const {
    setColorScheme,
    toggleSidebar,
    setSidebarOpened,
    setLoading,
    addNotification,
    removeNotification,
    clearNotifications,
    openModal,
    closeModal,
    toggleModal,
} = uiSlice.actions

export default uiSlice.reducer