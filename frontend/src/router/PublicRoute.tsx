import { Navigate, useLocation } from 'react-router-dom'
import { useAppSelector } from '../store/hooks'

interface PublicRouteProps {
    children: React.ReactNode
}

const PublicRoute = ({ children }: PublicRouteProps) => {
    const { isAuthenticated } = useAppSelector((state) => state.auth)
    const location = useLocation()

    if (isAuthenticated) {
        // Redirect to the page they were trying to visit or dashboard
        const locationState = location.state as { from?: { pathname: string } } | null
        const from = locationState?.from?.pathname || '/dashboard'
        return <Navigate to={from} replace />
    }

    return <>{children}</>
}

export default PublicRoute