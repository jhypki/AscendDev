import { Navigate, useLocation } from 'react-router-dom'
import { useAppSelector } from '../store/hooks'
import { AppLayout } from '../components/layout'

interface ProtectedRouteProps {
    children: React.ReactNode
    requiredRoles?: string[]
}

const ProtectedRoute = ({ children, requiredRoles = [] }: ProtectedRouteProps) => {
    const { isAuthenticated, user } = useAppSelector((state) => state.auth)
    const location = useLocation()

    if (!isAuthenticated) {
        // Redirect to login page with return url
        return <Navigate to="/login" state={{ from: location }} replace />
    }

    if (requiredRoles.length > 0 && user) {
        const hasRequiredRole = requiredRoles.some(role => user.userRoles.includes(role))
        if (!hasRequiredRole) {
            // Redirect to dashboard if user doesn't have required role
            return <Navigate to="/dashboard" replace />
        }
    }

    return <AppLayout>{children}</AppLayout>
}

export default ProtectedRoute