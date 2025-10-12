import {
    Container,
    Paper,
    Title,
    Text,
    Center,
    Stack,
    Button,
    Alert,
    Loader,
    Group,
    TextInput,
    Anchor
} from '@mantine/core'
import { useEffect, useState } from 'react'
import { useSearchParams, Link, useNavigate } from 'react-router-dom'
import { useVerifyEmail, useResendVerification } from '../../hooks/api/useAuth'
import { IconCheck, IconX, IconMail, IconAlertCircle } from '@tabler/icons-react'
import { notifications } from '@mantine/notifications'
import { useForm } from '@mantine/form'

const EmailVerificationPage = () => {
    const [searchParams] = useSearchParams()
    const navigate = useNavigate()
    const token = searchParams.get('token')
    const [verificationStatus, setVerificationStatus] = useState<'loading' | 'success' | 'error' | 'resend'>('loading')
    const [showResendForm, setShowResendForm] = useState(false)

    const verifyEmailMutation = useVerifyEmail()
    const resendVerificationMutation = useResendVerification()

    const resendForm = useForm({
        initialValues: {
            email: '',
        },
        validate: {
            email: (value) => {
                if (!value) return 'Email is required'
                if (!/^\S+@\S+\.\S+$/.test(value)) return 'Invalid email format'
                return null
            },
        },
    })

    useEffect(() => {
        if (token) {
            verifyEmailMutation.mutate(token, {
                onSuccess: (data) => {
                    if (data.message) {
                        setVerificationStatus('success')
                        notifications.show({
                            title: 'Email Verified!',
                            message: data.message,
                            color: 'green',
                            icon: <IconCheck size="1rem" />,
                        })
                        // Redirect to dashboard after 3 seconds
                        setTimeout(() => {
                            navigate('/dashboard')
                        }, 3000)
                    } else {
                        setVerificationStatus('error')
                    }
                },
                onError: () => {
                    setVerificationStatus('error')
                }
            })
        } else {
            setVerificationStatus('resend')
        }
    }, [token, verifyEmailMutation, navigate])

    const handleResendVerification = async (values: { email: string }) => {
        try {
            const result = await resendVerificationMutation.mutateAsync(values.email)
            if (result.message) {
                notifications.show({
                    title: 'Verification Email Sent',
                    message: result.message,
                    color: 'green',
                    icon: <IconMail size="1rem" />,
                })
                setShowResendForm(false)
            }
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to send verification email. Please try again.',
                color: 'red',
                icon: <IconX size="1rem" />,
            })
        }
    }

    const renderContent = () => {
        switch (verificationStatus) {
            case 'loading':
                return (
                    <Stack align="center" gap="md">
                        <Loader size="xl" />
                        <Title order={2} ta="center">Verifying Your Email</Title>
                        <Text c="dimmed" ta="center">
                            Please wait while we verify your email address...
                        </Text>
                    </Stack>
                )

            case 'success':
                return (
                    <Stack align="center" gap="md">
                        <IconCheck size={64} color="var(--mantine-color-green-6)" />
                        <Title order={2} ta="center" c="green">Email Verified Successfully!</Title>
                        <Text c="dimmed" ta="center">
                            Your email has been verified. You now have full access to AscendDev.
                        </Text>
                        <Text c="dimmed" ta="center" size="sm">
                            Redirecting to dashboard in a few seconds...
                        </Text>
                        <Button
                            component={Link}
                            to="/dashboard"
                            leftSection={<IconCheck size="1rem" />}
                        >
                            Go to Dashboard
                        </Button>
                    </Stack>
                )

            case 'error':
                return (
                    <Stack align="center" gap="md">
                        <IconX size={64} color="var(--mantine-color-red-6)" />
                        <Title order={2} ta="center" c="red">Verification Failed</Title>
                        <Text c="dimmed" ta="center">
                            The verification link is invalid or has expired.
                        </Text>
                        <Alert
                            icon={<IconAlertCircle size="1rem" />}
                            color="red"
                            variant="light"
                        >
                            This could happen if:
                            <ul style={{ margin: '8px 0', paddingLeft: '20px' }}>
                                <li>The verification link has expired (links expire after 24 hours)</li>
                                <li>The link has already been used</li>
                                <li>The link was corrupted during copy/paste</li>
                            </ul>
                        </Alert>
                        <Group>
                            <Button
                                variant="outline"
                                onClick={() => setShowResendForm(true)}
                                leftSection={<IconMail size="1rem" />}
                            >
                                Resend Verification Email
                            </Button>
                            <Button component={Link} to="/login">
                                Back to Login
                            </Button>
                        </Group>
                    </Stack>
                )

            case 'resend':
                return (
                    <Stack align="center" gap="md">
                        <IconMail size={64} color="var(--mantine-color-blue-6)" />
                        <Title order={2} ta="center">Email Verification Required</Title>
                        <Text c="dimmed" ta="center">
                            Please check your email for a verification link, or request a new one below.
                        </Text>
                        <Button
                            onClick={() => setShowResendForm(true)}
                            leftSection={<IconMail size="1rem" />}
                        >
                            Send Verification Email
                        </Button>
                        <Anchor component={Link} to="/login" size="sm">
                            Back to Login
                        </Anchor>
                    </Stack>
                )
        }
    }

    return (
        <Center h="100vh">
            <Container size="sm" w={480}>
                <Paper shadow="md" p="xl" radius="md" withBorder w="100%">
                    {renderContent()}

                    {showResendForm && (
                        <Stack mt="xl" gap="md">
                            <Title order={3} ta="center">Resend Verification Email</Title>
                            <form onSubmit={resendForm.onSubmit(handleResendVerification)}>
                                <Stack gap="md">
                                    <TextInput
                                        label="Email Address"
                                        placeholder="Enter your email address"
                                        required
                                        {...resendForm.getInputProps('email')}
                                        disabled={resendVerificationMutation.isPending}
                                    />

                                    {resendVerificationMutation.error && (
                                        <Alert
                                            icon={<IconAlertCircle size="1rem" />}
                                            color="red"
                                            variant="light"
                                        >
                                            Failed to send verification email. Please check the email address and try again.
                                        </Alert>
                                    )}

                                    <Group justify="center">
                                        <Button
                                            variant="outline"
                                            onClick={() => setShowResendForm(false)}
                                            disabled={resendVerificationMutation.isPending}
                                        >
                                            Cancel
                                        </Button>
                                        <Button
                                            type="submit"
                                            loading={resendVerificationMutation.isPending}
                                            leftSection={<IconMail size="1rem" />}
                                        >
                                            Send Verification Email
                                        </Button>
                                    </Group>
                                </Stack>
                            </form>
                        </Stack>
                    )}
                </Paper>
            </Container>
        </Center>
    )
}

export default EmailVerificationPage