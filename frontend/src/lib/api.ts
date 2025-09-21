import axios from 'axios'
import { store } from '../store'
import { logout, refreshTokenSuccess } from '../store/slices/authSlice'

const api = axios.create({
    baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5171/api',
    timeout: 10000,
    headers: {
        'Content-Type': 'application/json',
    },
})

// Request interceptor for auth tokens
api.interceptors.request.use(
    (config) => {
        const token = store.getState().auth.token
        if (token) {
            config.headers.Authorization = `Bearer ${token}`
        }
        return config
    },
    (error) => {
        return Promise.reject(error)
    }
)

// Response interceptor for error handling and token refresh
api.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config

        if (error.response?.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true

            const refreshToken = store.getState().auth.refreshToken
            if (refreshToken) {
                try {
                    const response = await axios.post(
                        `${import.meta.env.VITE_API_URL || 'http://localhost:5171/api'}/auth/refresh`,
                        { refreshToken }
                    )

                    const data = response.data
                    if (data.isSuccess) {
                        store.dispatch(refreshTokenSuccess({
                            token: data.accessToken,
                            refreshToken: data.refreshToken
                        }))
                    } else {
                        // Refresh failed, logout user
                        store.dispatch(logout())
                        window.location.href = '/login'
                        return Promise.reject(new Error(data.errorMessage || 'Token refresh failed'))
                    }

                    // Retry the original request with new token
                    originalRequest.headers.Authorization = `Bearer ${data.accessToken}`
                    return api(originalRequest)
                } catch (refreshError) {
                    // Refresh failed, logout user
                    store.dispatch(logout())
                    window.location.href = '/login'
                    return Promise.reject(refreshError)
                }
            } else {
                // No refresh token, logout user
                store.dispatch(logout())
                window.location.href = '/login'
            }
        }

        return Promise.reject(error)
    }
)

export default api