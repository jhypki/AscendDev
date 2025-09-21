import { createApi, fetchBaseQuery, type BaseQueryFn, type FetchArgs, type FetchBaseQueryError } from '@reduxjs/toolkit/query/react'
import type { RootState } from '../index'

interface RefreshTokenResponse {
    token: string
    refreshToken: string
}

const baseQuery = fetchBaseQuery({
    baseUrl: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
    prepareHeaders: (headers, { getState }) => {
        const token = (getState() as RootState).auth.token
        if (token) {
            headers.set('authorization', `Bearer ${token}`)
        }
        headers.set('content-type', 'application/json')
        return headers
    },
})

const baseQueryWithReauth: BaseQueryFn<
    string | FetchArgs,
    unknown,
    FetchBaseQueryError
> = async (args, api, extraOptions) => {
    let result = await baseQuery(args, api, extraOptions)

    if (result.error && result.error.status === 401) {
        // Try to get a new token
        const refreshToken = (api.getState() as RootState).auth.refreshToken
        if (refreshToken) {
            const refreshResult = await baseQuery(
                {
                    url: '/auth/refresh',
                    method: 'POST',
                    body: { refreshToken },
                },
                api,
                extraOptions
            )

            if (refreshResult.data) {
                // Store the new token
                const { token, refreshToken: newRefreshToken } = refreshResult.data as RefreshTokenResponse
                api.dispatch({
                    type: 'auth/refreshTokenSuccess',
                    payload: { token, refreshToken: newRefreshToken },
                })

                // Retry the original query with new token
                result = await baseQuery(args, api, extraOptions)
            } else {
                // Refresh failed, logout user
                api.dispatch({ type: 'auth/logout' })
            }
        } else {
            // No refresh token, logout user
            api.dispatch({ type: 'auth/logout' })
        }
    }

    return result
}

export const apiSlice = createApi({
    reducerPath: 'api',
    baseQuery: baseQueryWithReauth,
    tagTypes: [
        'User',
        'Course',
        'Lesson',
        'Submission',
        'Discussion',
        'CodeReview',
        'Analytics',
    ],
    endpoints: () => ({}),
})

export default apiSlice.reducer