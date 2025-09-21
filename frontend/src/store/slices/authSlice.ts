import { createSlice, type PayloadAction } from '@reduxjs/toolkit'

export interface User {
    id: string
    email: string
    username: string
    firstName?: string
    lastName?: string
    profilePictureUrl?: string
    isEmailVerified: boolean
    userRoles: string[]
    bio?: string
    provider?: string
    createdAt: string
    lastLogin?: string
    fullName: string
}

export interface AuthState {
    user: User | null
    token: string | null
    refreshToken: string | null
    isAuthenticated: boolean
    loading: boolean
    error: string | null
}

// Helper function to safely parse user data from localStorage
const getUserFromStorage = (): User | null => {
    try {
        const userData = localStorage.getItem('user')
        return userData ? JSON.parse(userData) : null
    } catch {
        return null
    }
}

const initialState: AuthState = {
    user: getUserFromStorage(),
    token: localStorage.getItem('token'),
    refreshToken: localStorage.getItem('refreshToken'),
    isAuthenticated: !!localStorage.getItem('token'),
    loading: false,
    error: null,
}

export const authSlice = createSlice({
    name: 'auth',
    initialState,
    reducers: {
        loginStart: (state) => {
            state.loading = true
            state.error = null
        },
        loginSuccess: (state, action: PayloadAction<{ user: User; token: string; refreshToken: string }>) => {
            state.user = action.payload.user
            state.token = action.payload.token
            state.refreshToken = action.payload.refreshToken
            state.isAuthenticated = true
            state.loading = false
            state.error = null

            // Persist tokens and user data to localStorage
            localStorage.setItem('token', action.payload.token)
            localStorage.setItem('refreshToken', action.payload.refreshToken)
            localStorage.setItem('user', JSON.stringify(action.payload.user))
        },
        loginFailure: (state, action: PayloadAction<string>) => {
            state.user = null
            state.token = null
            state.refreshToken = null
            state.isAuthenticated = false
            state.loading = false
            state.error = action.payload

            // Clear tokens and user data from localStorage
            localStorage.removeItem('token')
            localStorage.removeItem('refreshToken')
            localStorage.removeItem('user')
        },
        logout: (state) => {
            state.user = null
            state.token = null
            state.refreshToken = null
            state.isAuthenticated = false
            state.loading = false
            state.error = null

            // Clear tokens and user data from localStorage
            localStorage.removeItem('token')
            localStorage.removeItem('refreshToken')
            localStorage.removeItem('user')

            // Clear OAuth-related session storage
            const keysToRemove = []
            for (let i = 0; i < sessionStorage.length; i++) {
                const key = sessionStorage.key(i)
                if (key && (key.startsWith('oauth_state_') || key.startsWith('oauth_processed_'))) {
                    keysToRemove.push(key)
                }
            }
            keysToRemove.forEach(key => sessionStorage.removeItem(key))
        },
        refreshTokenSuccess: (state, action: PayloadAction<{ token: string; refreshToken: string }>) => {
            state.token = action.payload.token
            state.refreshToken = action.payload.refreshToken

            // Update tokens in localStorage
            localStorage.setItem('token', action.payload.token)
            localStorage.setItem('refreshToken', action.payload.refreshToken)
        },
        clearError: (state) => {
            state.error = null
        },
        updateUser: (state, action: PayloadAction<Partial<User>>) => {
            if (state.user) {
                state.user = { ...state.user, ...action.payload }
                // Persist updated user data to localStorage
                localStorage.setItem('user', JSON.stringify(state.user))
            }
        },
        setUser: (state, action: PayloadAction<User>) => {
            state.user = action.payload
            // Persist user data to localStorage
            localStorage.setItem('user', JSON.stringify(action.payload))
        },
    },
})

export const {
    loginStart,
    loginSuccess,
    loginFailure,
    logout,
    refreshTokenSuccess,
    clearError,
    updateUser,
    setUser,
} = authSlice.actions

export default authSlice.reducer