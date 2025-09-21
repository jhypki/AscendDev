import { useEffect } from 'react'
import { useAppDispatch, useAppSelector } from '../store/hooks'
import { setUser } from '../store/slices/authSlice'
import api from '../lib/api'

export const useAuthRestore = () => {
    const dispatch = useAppDispatch()
    const { isAuthenticated, user, token } = useAppSelector((state) => state.auth)

    useEffect(() => {
        const restoreUserData = async () => {
            console.log('useAuthRestore - isAuthenticated:', isAuthenticated)
            console.log('useAuthRestore - token:', !!token)
            console.log('useAuthRestore - user:', user)
            console.log('useAuthRestore - user.userRoles:', user?.userRoles)

            // Fetch user data if we have a token but no user data, or if user data is missing roles
            if (isAuthenticated && token && (!user || !user.userRoles || user.userRoles.length === 0)) {
                console.log('useAuthRestore - Fetching user data from API')
                try {
                    const response = await api.get('/auth/me')
                    console.log('useAuthRestore - API response:', response.data)
                    if (response.data.isSuccess && response.data.user) {
                        console.log('useAuthRestore - Dispatching setUser with:', response.data.user)
                        dispatch(setUser(response.data.user))
                    }
                } catch (error) {
                    console.error('Failed to restore user data:', error)
                    // If the token is invalid, the API interceptor will handle logout
                }
            } else {
                console.log('useAuthRestore - Conditions not met, not fetching user data')
            }
        }

        restoreUserData()
    }, [isAuthenticated, token, user, dispatch])
}