import { useEffect, useState, useRef } from 'react'
import { useParams, useSearchParams, useNavigate } from 'react-router-dom'
import { Center, Loader, Text, Stack, Alert } from '@mantine/core'
import { IconAlertCircle } from '@tabler/icons-react'
import { useOAuthCallback } from '../../hooks/api/useOAuth'

const OAuthCallbackPage = () => {
    const { provider } = useParams<{ provider: string }>()
    const [searchParams] = useSearchParams()
    const navigate = useNavigate()
    const oauthCallback = useOAuthCallback()
    const [error, setError] = useState<string | null>(null)
    const [isProcessing, setIsProcessing] = useState(false)
    const hasProcessed = useRef(false)

    useEffect(() => {
        // Prevent multiple executions
        if (hasProcessed.current || isProcessing) {
            return
        }

        const handleCallback = async () => {
            // Mark as processing to prevent multiple executions
            hasProcessed.current = true
            setIsProcessing(true)

            if (!provider) {
                setError('Invalid OAuth provider')
                return
            }

            const code = searchParams.get('code')
            const state = searchParams.get('state')
            const errorParam = searchParams.get('error')
            const errorDescription = searchParams.get('error_description')

            // Check for OAuth errors
            if (errorParam) {
                const message = errorDescription || `OAuth error: ${errorParam}`
                setError(message)
                return
            }

            if (!code) {
                setError('Authorization code not received')
                return
            }

            // Prevent code reuse - check if this code has already been processed
            const processedCodeKey = `oauth_processed_${provider}_${code.substring(0, 10)}`
            if (sessionStorage.getItem(processedCodeKey)) {
                setError('This authorization code has already been used. Please try logging in again.')
                return
            }

            // Mark this code as being processed
            sessionStorage.setItem(processedCodeKey, 'true')

            // Verify state parameter
            const storedState = sessionStorage.getItem(`oauth_state_${provider}`)
            if (state && storedState && state !== storedState) {
                setError('Invalid state parameter - possible CSRF attack')
                return
            }

            // Clean up stored state
            if (storedState) {
                sessionStorage.removeItem(`oauth_state_${provider}`)
            }

            try {
                // Use the same redirect URI format as in OAuthButton
                const redirectUri = `${window.location.origin}/auth/callback/${provider}`

                const result = await oauthCallback.mutateAsync({
                    provider,
                    code,
                    state: state || undefined,
                    redirectUri
                })

                if (result.isSuccess) {
                    // Clean up the processed code marker on success
                    sessionStorage.removeItem(processedCodeKey)
                    // Redirect to dashboard on success
                    navigate('/dashboard', { replace: true })
                } else {
                    setError(result.errorMessage || 'OAuth login failed')
                }
            } catch (err) {
                console.error('OAuth callback error:', err)
                setError('Failed to complete OAuth login. Please try again.')
            } finally {
                setIsProcessing(false)
            }
        }

        handleCallback()
    }, []) // Empty dependency array to run only once

    if (error) {
        return (
            <Center h="100vh" bg="gray.0">
                <Stack align="center" gap="md" maw={400}>
                    <Alert
                        icon={<IconAlertCircle size="1rem" />}
                        color="red"
                        variant="light"
                        title="OAuth Error"
                    >
                        {error}
                    </Alert>
                    <Text size="sm" c="dimmed" ta="center">
                        You can close this window and try logging in again.
                    </Text>
                </Stack>
            </Center>
        )
    }

    return (
        <Center h="100vh" bg="gray.0">
            <Stack align="center" gap="md">
                <Loader size="lg" />
                <Text size="lg" fw={500}>
                    Completing OAuth login...
                </Text>
                <Text size="sm" c="dimmed" ta="center">
                    Please wait while we verify your {provider} account
                </Text>
            </Stack>
        </Center>
    )
}

export default OAuthCallbackPage