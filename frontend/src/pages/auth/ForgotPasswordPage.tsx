import {
    Container,
    Paper,
    Title,
    Text,
    Center,
    Stack,
    TextInput,
    Button,
    Anchor,
    Alert,
    Divider
} from '@mantine/core'
import { useForm } from '@mantine/form'
import { IconAlertCircle, IconMail, IconArrowLeft, IconCheck } from '@tabler/icons-react'
import { Link } from 'react-router-dom'
import { useMutation } from '@tanstack/react-query'
import { useState } from 'react'
import { notifications } from '@mantine/notifications'
import api from '../../lib/api'

interface ForgotPasswordFormValues {
    email: string
}

const ForgotPasswordPage = () => {
    const [emailSent, setEmailSent] = useState(false)

    const forgotPasswordMutation = useMutation({
        mutationFn: async (email: string) => {
            const response = await api.post('/auth/forgot-password', { email })
            return response.data
        },
        onSuccess: () => {
            setEmailSent(true)
            notifications.show({
                title: 'Email Sent',
                message: 'If an account with that email exists, we\'ve sent password reset instructions.',
                color: 'green',
            })
        },
        onError: (error: unknown) => {
            const errorWithResponse = error as { response?: { data?: { message?: string } } }
            notifications.show({
                title: 'Error',
                message: errorWithResponse.response?.data?.message || 'Failed to send reset email',
                color: 'red',
            })
        },
    })

    const form = useForm<ForgotPasswordFormValues>({
        mode: 'uncontrolled',
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
        validateInputOnBlur: true,
    })

    const handleSubmit = async (values: ForgotPasswordFormValues) => {
        await forgotPasswordMutation.mutateAsync(values.email)
    }

    if (emailSent) {
        return (
            <Center h="100vh">
                <Container size="sm">
                    <Paper shadow="md" p="xl" radius="md" withBorder>
                        <Stack align="center" gap="md">
                            <IconCheck size={48} color="var(--mantine-color-green-6)" />
                            <Title order={2} ta="center">Check Your Email</Title>
                            <Text c="dimmed" ta="center" size="sm">
                                We've sent password reset instructions to your email address.
                                Please check your inbox and follow the link to reset your password.
                            </Text>
                        </Stack>

                        <Divider my="lg" />

                        <Stack gap="md">
                            <Text size="sm" c="dimmed" ta="center">
                                Didn't receive the email? Check your spam folder or try again.
                            </Text>

                            <Button
                                variant="outline"
                                fullWidth
                                onClick={() => setEmailSent(false)}
                            >
                                Try Again
                            </Button>

                            <Anchor component={Link} to="/login" size="sm" ta="center">
                                <IconArrowLeft size="1rem" /> Back to Login
                            </Anchor>
                        </Stack>
                    </Paper>
                </Container>
            </Center>
        )
    }

    return (
        <Center h="100vh">
            <Container size="sm">
                <Paper shadow="md" p="xl" radius="md" withBorder>
                    <Stack align="center" gap="md">
                        <IconMail size={48} color="var(--mantine-color-blue-6)" />
                        <Title order={2} ta="center">Forgot Password?</Title>
                        <Text c="dimmed" ta="center" size="sm">
                            Enter your email address and we'll send you instructions to reset your password.
                        </Text>
                    </Stack>

                    <Divider my="lg" />

                    <form onSubmit={form.onSubmit(handleSubmit)}>
                        <Stack gap="md">
                            <TextInput
                                label="Email"
                                placeholder="Enter your email address"
                                required
                                key={form.key('email')}
                                {...form.getInputProps('email')}
                                disabled={forgotPasswordMutation.isPending}
                            />

                            {!!forgotPasswordMutation.error && (
                                <Alert
                                    icon={<IconAlertCircle size="1rem" />}
                                    color="red"
                                    variant="light"
                                >
                                    {forgotPasswordMutation.error instanceof Error
                                        ? forgotPasswordMutation.error.message
                                        : 'An error occurred'
                                    }
                                </Alert>
                            )}

                            <Button
                                type="submit"
                                fullWidth
                                loading={forgotPasswordMutation.isPending}
                                leftSection={<IconMail size="1rem" />}
                            >
                                Send Reset Instructions
                            </Button>
                        </Stack>
                    </form>

                    <Divider my="lg" />

                    <Stack gap="xs" align="center">
                        <Anchor component={Link} to="/login" size="sm">
                            <IconArrowLeft size="1rem" /> Back to Login
                        </Anchor>

                        <Text size="sm" c="dimmed">
                            Don't have an account?{' '}
                            <Anchor component={Link} to="/register" size="sm">
                                Sign up
                            </Anchor>
                        </Text>
                    </Stack>
                </Paper>
            </Container>
        </Center>
    )
}

export default ForgotPasswordPage