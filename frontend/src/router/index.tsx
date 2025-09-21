import { createBrowserRouter, Navigate } from 'react-router-dom'
import { lazy, Suspense } from 'react'
import { Center, Loader } from '@mantine/core'
import ProtectedRoute from './ProtectedRoute'
import PublicRoute from './PublicRoute'

// Lazy load components
const LoginPage = lazy(() => import('../pages/auth/LoginPage'))
const RegisterPage = lazy(() => import('../pages/auth/RegisterPage'))
const ForgotPasswordPage = lazy(() => import('../pages/auth/ForgotPasswordPage'))
const OAuthCallbackPage = lazy(() => import('../pages/auth/OAuthCallbackPage'))
const DashboardPage = lazy(() => import('../pages/dashboard/DashboardPage'))
const CoursesPage = lazy(() => import('../pages/courses/CoursesPage'))
const CourseDetailPage = lazy(() => import('../pages/courses/CourseDetailPage'))
const LessonPage = lazy(() => import('../pages/lessons/LessonPage'))
const ProfilePage = lazy(() => import('../pages/profile/ProfilePage'))
const AdminDashboard = lazy(() => import('../pages/admin/AdminDashboard'))
const NotFoundPage = lazy(() => import('../pages/NotFoundPage'))

// Loading component
const PageLoader = () => (
    <Center h="100vh">
        <Loader size="lg" />
    </Center>
)

// Wrapper component for lazy loading
const LazyWrapper = ({ children }: { children: React.ReactNode }) => (
    <Suspense fallback={<PageLoader />}>
        {children}
    </Suspense>
)

export const router = createBrowserRouter([
    {
        path: '/',
        element: <Navigate to="/dashboard" replace />,
    },
    {
        path: '/login',
        element: (
            <PublicRoute>
                <LazyWrapper>
                    <LoginPage />
                </LazyWrapper>
            </PublicRoute>
        ),
    },
    {
        path: '/register',
        element: (
            <PublicRoute>
                <LazyWrapper>
                    <RegisterPage />
                </LazyWrapper>
            </PublicRoute>
        ),
    },
    {
        path: '/forgot-password',
        element: (
            <PublicRoute>
                <LazyWrapper>
                    <ForgotPasswordPage />
                </LazyWrapper>
            </PublicRoute>
        ),
    },
    {
        path: '/auth/callback/:provider',
        element: (
            <PublicRoute>
                <LazyWrapper>
                    <OAuthCallbackPage />
                </LazyWrapper>
            </PublicRoute>
        ),
    },
    {
        path: '/dashboard',
        element: (
            <ProtectedRoute>
                <LazyWrapper>
                    <DashboardPage />
                </LazyWrapper>
            </ProtectedRoute>
        ),
    },
    {
        path: '/courses',
        element: (
            <ProtectedRoute>
                <LazyWrapper>
                    <CoursesPage />
                </LazyWrapper>
            </ProtectedRoute>
        ),
    },
    {
        path: '/courses/:courseId',
        element: (
            <ProtectedRoute>
                <LazyWrapper>
                    <CourseDetailPage />
                </LazyWrapper>
            </ProtectedRoute>
        ),
    },
    {
        path: '/courses/:courseId/lessons/:lessonId',
        element: (
            <ProtectedRoute>
                <LazyWrapper>
                    <LessonPage />
                </LazyWrapper>
            </ProtectedRoute>
        ),
    },
    {
        path: '/profile',
        element: (
            <ProtectedRoute>
                <LazyWrapper>
                    <ProfilePage />
                </LazyWrapper>
            </ProtectedRoute>
        ),
    },
    {
        path: '/admin',
        element: (
            <ProtectedRoute requiredRoles={['Admin', 'SuperAdmin']}>
                <LazyWrapper>
                    <AdminDashboard />
                </LazyWrapper>
            </ProtectedRoute>
        ),
    },
    {
        path: '*',
        element: (
            <LazyWrapper>
                <NotFoundPage />
            </LazyWrapper>
        ),
    },
])