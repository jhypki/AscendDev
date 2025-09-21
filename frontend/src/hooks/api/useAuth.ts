import { useMutation, useQuery } from '@tanstack/react-query'
import api from '../../lib/api'
import { useAppDispatch } from '../../store/hooks'
import { loginStart, loginSuccess, loginFailure, logout } from '../../store/slices/authSlice'
import { queryClient } from '../../lib/queryClient'

interface LoginRequest {
    email: string
    password: string
}

interface RegisterRequest {
    email: string
    password: string
    firstName: string
    lastName: string
}

interface AuthResult {
    isSuccess: boolean
    accessToken: string
    refreshToken: string
    user: {
        id: string
        email: string
        username: string
        firstName?: string
        lastName?: string
        profilePictureUrl?: string
        isEmailVerified: boolean
        roles: string[]
        bio?: string
        provider?: string
        createdAt: string
        lastLogin?: string
        fullName: string
    }
    errorMessage?: string
    errors?: string[]
}

export const useLogin = () => {
    const dispatch = useAppDispatch()

    return useMutation({
        mutationFn: async (credentials: LoginRequest): Promise<AuthResult> => {
            dispatch(loginStart())
            const response = await api.post('/auth/login', credentials)
            return response.data
        },
        onSuccess: (data) => {
            if (data.isSuccess) {
                dispatch(loginSuccess({
                    user: data.user,
                    token: data.accessToken,
                    refreshToken: data.refreshToken
                }))
                queryClient.invalidateQueries({ queryKey: ['user'] })
            } else {
                const message = data.errorMessage || data.errors?.join(', ') || 'Login failed'
                dispatch(loginFailure(message))
            }
        },
        onError: (error: unknown) => {
            const errorWithResponse = error as { response?: { data?: { message?: string } } }
            const message = errorWithResponse.response?.data?.message || 'Login failed'
            dispatch(loginFailure(message))
        },
    })
}

export const useRegister = () => {
    const dispatch = useAppDispatch()

    return useMutation({
        mutationFn: async (userData: RegisterRequest): Promise<AuthResult> => {
            const response = await api.post('/auth/register', userData)
            return response.data
        },
        onSuccess: (data) => {
            if (data.isSuccess) {
                dispatch(loginSuccess({
                    user: data.user,
                    token: data.accessToken,
                    refreshToken: data.refreshToken
                }))
                queryClient.invalidateQueries({ queryKey: ['user'] })
            }
        },
    })
}

export const useLogout = () => {
    const dispatch = useAppDispatch()

    return useMutation({
        mutationFn: async () => {
            // Get refresh token from localStorage to send to server
            const refreshToken = localStorage.getItem('refreshToken')
            await api.post('/auth/logout', { refreshToken })
        },
        onSuccess: () => {
            dispatch(logout())
            queryClient.clear()
            // Clear any OAuth-related session storage
            sessionStorage.clear()
        },
        onError: () => {
            // Even if logout fails on server, clear local state
            dispatch(logout())
            queryClient.clear()
            // Clear any OAuth-related session storage
            sessionStorage.clear()
        },
    })
}

export const useRefreshToken = () => {
    return useMutation({
        mutationFn: async (refreshToken: string) => {
            const response = await api.post('/auth/refresh', { refreshToken })
            return response.data
        },
    })
}

export const useCurrentUser = () => {
    return useQuery({
        queryKey: ['user', 'current'],
        queryFn: async () => {
            const response = await api.get('/auth/me')
            return response.data
        },
        enabled: false, // Only fetch when explicitly called
    })
}