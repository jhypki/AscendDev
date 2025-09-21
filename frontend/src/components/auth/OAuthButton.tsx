import { Button, type ButtonProps } from '@mantine/core'
import { IconBrandGoogle, IconBrandGithub } from '@tabler/icons-react'
import { useOAuthAuthorize } from '../../hooks/api/useOAuth'

interface OAuthButtonProps extends Omit<ButtonProps, 'onClick' | 'leftSection'> {
    provider: 'google' | 'github'
    redirectUri?: string
    onAuthStart?: () => void
}

const providerConfig = {
    google: {
        icon: IconBrandGoogle,
        label: 'Continue with Google',
        color: '#4285f4',
        variant: 'outline' as const,
    },
    github: {
        icon: IconBrandGithub,
        label: 'Continue with GitHub',
        color: '#333',
        variant: 'filled' as const,
    },
}

const OAuthButton = ({ provider, redirectUri, onAuthStart, ...props }: OAuthButtonProps) => {
    const oauthAuthorize = useOAuthAuthorize()
    const config = providerConfig[provider]
    const Icon = config.icon

    const handleClick = async () => {
        try {
            onAuthStart?.()

            // Use consistent redirect URI format
            const callbackUri = redirectUri || `${window.location.origin}/auth/callback/${provider}`

            const result = await oauthAuthorize.mutateAsync({
                provider,
                redirectUri: callbackUri
            })

            // Store state in sessionStorage for verification
            if (result.state) {
                sessionStorage.setItem(`oauth_state_${provider}`, result.state)
            }

            // Redirect to OAuth provider
            window.location.href = result.url
        } catch (error) {
            console.error(`OAuth ${provider} authorization failed:`, error)
        }
    }

    return (
        <Button
            {...props}
            variant={config.variant}
            color={provider === 'google' ? 'blue' : 'dark'}
            leftSection={<Icon size="1.2rem" />}
            onClick={handleClick}
            loading={oauthAuthorize.isPending}
            fullWidth
            styles={{
                root: {
                    backgroundColor: provider === 'google' ? 'white' : config.color,
                    color: provider === 'google' ? '#333' : 'white',
                    borderColor: provider === 'google' ? '#dadce0' : config.color,
                    '&:hover': {
                        backgroundColor: provider === 'google' ? '#f8f9fa' : '#24292e',
                        borderColor: provider === 'google' ? '#dadce0' : '#24292e',
                    },
                },
            }}
        >
            {config.label}
        </Button>
    )
}

export default OAuthButton