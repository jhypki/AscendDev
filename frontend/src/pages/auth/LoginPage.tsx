import {
    Container,
    Paper,
    Title,
    Text,
    Center,
    Stack,
    TextInput,
    PasswordInput,
    Button,
    Anchor,
    Alert,
    Group,
    Divider
} from '@mantine/core'
import OAuthButton from '../../components/auth/OAuthButton'
import { useForm } from '@mantine/form'
import { IconAlertCircle, IconLogin } from '@tabler/icons-react'
import { Link, useNavigate } from 'react-router-dom'
import { useLogin } from '../../hooks/api/useAuth'
import { notifications } from '@mantine/notifications'

interface LoginFormValues {
    email: string
    password: string
}

const LoginPage = () => {
    const navigate = useNavigate()
    const loginMutation = useLogin()

    const form = useForm<LoginFormValues>({
        mode: 'uncontrolled',
        initialValues: {
            email: '',
            password: '',
        },
        validate: {
            email: (value) => {
                if (!value) return 'Email is required'
                if (!/^\S+@\S+\.\S+$/.test(value)) return 'Invalid email format'
                return null
            },
            password: (value) => {
                if (!value) return 'Password is required'
                if (value.length < 6) return 'Password must be at least 6 characters'
                return null
            },
        },
        validateInputOnBlur: true,
    })

    const handleSubmit = async (values: LoginFormValues) => {
        try {
            await loginMutation.mutateAsync(values)
            notifications.show({
                title: 'Success',
                message: 'Login successful! Welcome back.',
                color: 'green',
            })
            navigate('/dashboard')
        } catch {
            // Error is already handled in the mutation
        }
    }

    return (
        <Center h="100vh">
            <Container size="sm" w={480}>
                <Paper shadow="md" p="xl" radius="md" withBorder w="100%">
                    <Stack align="center" gap="md">
                        <IconLogin size={48} color="var(--mantine-color-blue-6)" />
                        <Title order={2} ta="center">Welcome back</Title>
                        <Text c="dimmed" ta="center" size="sm">
                            Sign in to your AscendDev account
                        </Text>
                    </Stack>

                    <Divider my="lg" />

                    <form onSubmit={form.onSubmit(handleSubmit)}>
                        <Stack gap="md">
                            <TextInput
                                label="Email"
                                placeholder="Enter your email"
                                required
                                key={form.key('email')}
                                {...form.getInputProps('email')}
                                disabled={loginMutation.isPending}
                            />

                            <PasswordInput
                                label="Password"
                                placeholder="Enter your password"
                                required
                                key={form.key('password')}
                                {...form.getInputProps('password')}
                                disabled={loginMutation.isPending}
                            />

                            {!!loginMutation.error && (
                                <Alert
                                    icon={<IconAlertCircle size="1rem" />}
                                    color="red"
                                    variant="light"
                                >
                                    {loginMutation.error instanceof Error
                                        ? loginMutation.error.message
                                        : 'An error occurred during login'
                                    }
                                </Alert>
                            )}

                            <Button
                                type="submit"
                                fullWidth
                                loading={loginMutation.isPending}
                                leftSection={<IconLogin size="1rem" />}
                            >
                                Sign In
                            </Button>
                        </Stack>
                    </form>

                    <Divider my="lg" label="Or continue with" labelPosition="center" />

                    <Stack gap="md">
                        <OAuthButton provider="google" />
                        <OAuthButton provider="github" />
                    </Stack>

                    <Divider my="lg" />

                    <Stack gap="xs" align="center">
                        <Group gap="xs">
                            <Text size="sm" c="dimmed">
                                Don't have an account?
                            </Text>
                            <Anchor component={Link} to="/register" size="sm">
                                Sign up
                            </Anchor>
                        </Group>

                        <Anchor component={Link} to="/forgot-password" size="sm" c="dimmed">
                            Forgot your password?
                        </Anchor>
                    </Stack>
                </Paper>
            </Container>
        </Center>
    )
}

export default LoginPage