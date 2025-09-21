import { QueryClient } from '@tanstack/react-query'

export const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            staleTime: 5 * 60 * 1000, // 5 minutes
            gcTime: 10 * 60 * 1000, // 10 minutes (formerly cacheTime)
            retry: (failureCount, error: unknown) => {
                // Don't retry on 4xx errors except 408, 429
                const errorWithStatus = error as { status?: number }
                if (errorWithStatus?.status && errorWithStatus.status >= 400 && errorWithStatus.status < 500 && ![408, 429].includes(errorWithStatus.status)) {
                    return false
                }
                // Retry up to 3 times for other errors
                return failureCount < 3
            },
            refetchOnWindowFocus: false,
            refetchOnReconnect: true,
        },
        mutations: {
            retry: (failureCount, error: unknown) => {
                // Don't retry mutations on 4xx errors
                const errorWithStatus = error as { status?: number }
                if (errorWithStatus?.status && errorWithStatus.status >= 400 && errorWithStatus.status < 500) {
                    return false
                }
                // Retry up to 2 times for other errors
                return failureCount < 2
            },
        },
    },
})