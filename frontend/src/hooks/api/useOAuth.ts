import { useMutation } from '@tanstack/react-query'
import api from '../../lib/api'
import { useAppDispatch } from '../../store/hooks'
import { loginSuccess, loginFailure } from '../../store/slices/authSlice'
import { queryClient } from '../../lib/queryClient'
import { notifications } from '@mantine/notifications'

interface OAuthAuthorizationUrl {
    url: string
    state: string
}

interface OAuthCallbackRequest {
    provider: string
    code: string
    state?: string
    redirectUri?: string
}

interface AuthResult {
    isSuccess: boolean
    accessToken?: string
    refreshToken?: string
    user?: {
        id: string
        email: string
        firstName: string
        lastName: string
        roles: string[]
        isEmailVerified: boolean
    }
    errorMessage?: string
    errors?: string[]
}

export const useOAuthAuthorize = () => {
    return useMutation({
        mutationFn: async ({ provider, redirectUri }: { provider: string; redirectUri?: string }): Promise<OAuthAuthorizationUrl> => {
            const params = new URLSearchParams()
            if (redirectUri) {
                params.append('redirectUri', redirectUri)
            }

            const url = `/auth/oauth/${provider}/authorize${params.toString() ? `?${params.toString()}` : ''}`
            const response = await api.get(url)
            return response.data
        },
        onError: (error: unknown) => {
            const errorWithResponse = error as { response?: { data?: { message?: string } } }
            notifications.show({
                title: 'OAuth Error',
                message: errorWithResponse.response?.data?.message || 'Failed to get OAuth authorization URL',
                color: 'red',
            })
        },
    })
}

export const useOAuthCallback = () => {
    const dispatch = useAppDispatch()

    return useMutation({
        mutationFn: async (request: OAuthCallbackRequest): Promise<AuthResult> => {
            const formData = new FormData()
            formData.append('code', request.code)
            if (request.state) formData.append('state', request.state)
            if (request.redirectUri) formData.append('redirectUri', request.redirectUri)

            const response = await api.post(`/auth/oauth/${request.provider}/callback`, formData, {
                headers: {
                    'Content-Type': 'multipart/form-data',
                },
            })
            return response.data
        },
        onSuccess: (data) => {
            if (data.isSuccess && data.user && data.accessToken && data.refreshToken) {
                dispatch(loginSuccess({
                    user: data.user,
                    token: data.accessToken,
                    refreshToken: data.refreshToken
                }))
                queryClient.invalidateQueries({ queryKey: ['user'] })
                notifications.show({
                    title: 'Success',
                    message: 'Successfully logged in with OAuth!',
                    color: 'green',
                })
            } else {
                const message = data.errorMessage || data.errors?.join(', ') || 'OAuth login failed'
                dispatch(loginFailure(message))
                notifications.show({
                    title: 'OAuth Login Failed',
                    message,
                    color: 'red',
                })
            }
        },
        onError: (error: unknown) => {
            const errorWithResponse = error as { response?: { data?: { message?: string } } }
            const message = errorWithResponse.response?.data?.message || 'OAuth callback failed'
            dispatch(loginFailure(message))
            notifications.show({
                title: 'OAuth Error',
                message,
                color: 'red',
            })
        },
    })
}

export const useOAuthLink = () => {
    return useMutation({
        mutationFn: async ({ provider, code, state, redirectUri }: OAuthCallbackRequest) => {
            const response = await api.post(`/auth/oauth/${provider}/link`, {
                code,
                state,
                redirectUri
            })
            return response.data
        },
        onSuccess: () => {
            notifications.show({
                title: 'Success',
                message: 'OAuth account linked successfully!',
                color: 'green',
            })
        },
        onError: (error: unknown) => {
            const errorWithResponse = error as { response?: { data?: { message?: string } } }
            notifications.show({
                title: 'Link Failed',
                message: errorWithResponse.response?.data?.message || 'Failed to link OAuth account',
                color: 'red',
            })
        },
    })
}

export const useOAuthUnlink = () => {
    return useMutation({
        mutationFn: async (provider: string) => {
            const response = await api.delete(`/auth/oauth/${provider}/unlink`)
            return response.data
        },
        onSuccess: () => {
            notifications.show({
                title: 'Success',
                message: 'OAuth account unlinked successfully!',
                color: 'green',
            })
        },
        onError: (error: unknown) => {
            const errorWithResponse = error as { response?: { data?: { message?: string } } }
            notifications.show({
                title: 'Unlink Failed',
                message: errorWithResponse.response?.data?.message || 'Failed to unlink OAuth account',
                color: 'red',
            })
        },
    })
}

export const useLinkedProviders = () => {
    return useMutation({
        mutationFn: async (): Promise<{ providers: string[] }> => {
            const response = await api.get('/auth/oauth/linked')
            return response.data
        },
    })
}