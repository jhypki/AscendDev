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
    Divider,
    Progress,
    Box
} from '@mantine/core'
import OAuthButton from '../../components/auth/OAuthButton'
import { useForm } from '@mantine/form'
import { IconAlertCircle, IconUserPlus, IconCheck, IconX } from '@tabler/icons-react'
import { Link, useNavigate } from 'react-router-dom'
import { useRegister } from '../../hooks/api/useAuth'
import { useState } from 'react'
import { notifications } from '@mantine/notifications'

interface RegisterFormValues {
    firstName: string
    lastName: string
    email: string
    password: string
    confirmPassword: string
}

const getPasswordStrength = (password: string): number => {
    let strength = 0
    if (password.length >= 8) strength += 25
    if (/[a-z]/.test(password)) strength += 25
    if (/[A-Z]/.test(password)) strength += 25
    if (/[0-9]/.test(password) || /[^A-Za-z0-9]/.test(password)) strength += 25
    return strength
}

const getPasswordColor = (strength: number): string => {
    if (strength < 50) return 'red'
    if (strength < 75) return 'yellow'
    return 'green'
}

const PasswordRequirement = ({ meets, label }: { meets: boolean; label: string }) => (
    <Text c={meets ? 'teal' : 'red'} size="sm">
        {meets ? <IconCheck size="0.9rem" /> : <IconX size="0.9rem" />} {label}
    </Text>
)

const RegisterPage = () => {
    const navigate = useNavigate()
    const registerMutation = useRegister()
    const [passwordFocused, setPasswordFocused] = useState(false)

    const form = useForm<RegisterFormValues>({
        mode: 'uncontrolled',
        initialValues: {
            firstName: '',
            lastName: '',
            email: '',
            password: '',
            confirmPassword: '',
        },
        validate: {
            firstName: (value) => {
                if (!value.trim()) return 'First name is required'
                if (value.trim().length < 2) return 'First name must be at least 2 characters'
                if (!/^[a-zA-Z\s]+$/.test(value)) return 'First name can only contain letters and spaces'
                return null
            },
            lastName: (value) => {
                if (!value.trim()) return 'Last name is required'
                if (value.trim().length < 2) return 'Last name must be at least 2 characters'
                if (!/^[a-zA-Z\s]+$/.test(value)) return 'Last name can only contain letters and spaces'
                return null
            },
            email: (value) => {
                if (!value) return 'Email is required'
                if (!/^\S+@\S+\.\S+$/.test(value)) return 'Invalid email format'
                return null
            },
            password: (value) => {
                if (!value) return 'Password is required'
                if (value.length < 8) return 'Password must be at least 8 characters'
                if (!/(?=.*[a-z])/.test(value)) return 'Password must contain at least one lowercase letter'
                if (!/(?=.*[A-Z])/.test(value)) return 'Password must contain at least one uppercase letter'
                if (!/(?=.*[0-9])/.test(value)) return 'Password must contain at least one number'
                return null
            },
            confirmPassword: (value, values) => {
                if (!value) return 'Please confirm your password'
                if (value !== values.password) return 'Passwords do not match'
                return null
            },
        },
        validateInputOnBlur: true,
    })

    const handleSubmit = async (values: RegisterFormValues) => {
        try {
            const result = await registerMutation.mutateAsync({
                firstName: values.firstName.trim(),
                lastName: values.lastName.trim(),
                email: values.email.trim(),
                password: values.password,
            })

            if (result.isSuccess) {
                notifications.show({
                    title: 'Account Created Successfully!',
                    message: result.message || 'Please check your email to verify your account.',
                    color: 'green',
                    autoClose: 8000,
                })
                // Navigate to email verification page instead of dashboard
                navigate('/auth/verify-email')
            } else {
                notifications.show({
                    title: 'Registration Failed',
                    message: result.errorMessage || result.errors?.join(', ') || 'Failed to create account',
                    color: 'red',
                })
            }
        } catch {
            // Error is already handled in the mutation
        }
    }

    const passwordValue = form.getValues().password
    const passwordStrength = getPasswordStrength(passwordValue)

    return (
        <Center h="100vh">
            <Container size="sm" w={480}>
                <Paper shadow="md" p="xl" radius="md" withBorder w="100%">
                    <Stack align="center" gap="md">
                        <IconUserPlus size={48} color="var(--mantine-color-blue-6)" />
                        <Title order={2} ta="center">Create Account</Title>
                        <Text c="dimmed" ta="center" size="sm">
                            Join AscendDev and start your learning journey
                        </Text>
                    </Stack>

                    <Divider my="lg" />

                    <form onSubmit={form.onSubmit(handleSubmit)}>
                        <Stack gap="md">
                            <Group grow>
                                <TextInput
                                    label="First Name"
                                    placeholder="Enter your first name"
                                    required
                                    key={form.key('firstName')}
                                    {...form.getInputProps('firstName')}
                                    disabled={registerMutation.isPending}
                                />

                                <TextInput
                                    label="Last Name"
                                    placeholder="Enter your last name"
                                    required
                                    key={form.key('lastName')}
                                    {...form.getInputProps('lastName')}
                                    disabled={registerMutation.isPending}
                                />
                            </Group>

                            <TextInput
                                label="Email"
                                placeholder="Enter your email"
                                required
                                key={form.key('email')}
                                {...form.getInputProps('email')}
                                disabled={registerMutation.isPending}
                            />

                            <Box>
                                <PasswordInput
                                    label="Password"
                                    placeholder="Create a strong password"
                                    required
                                    key={form.key('password')}
                                    {...form.getInputProps('password')}
                                    disabled={registerMutation.isPending}
                                    onFocus={() => setPasswordFocused(true)}
                                    onBlur={() => setPasswordFocused(false)}
                                />

                                {passwordFocused && passwordValue && (
                                    <Box mt="xs">
                                        <Progress
                                            value={passwordStrength}
                                            color={getPasswordColor(passwordStrength)}
                                            size="sm"
                                            mb="xs"
                                        />
                                        <Stack gap={4}>
                                            <PasswordRequirement
                                                meets={passwordValue.length >= 8}
                                                label="At least 8 characters"
                                            />
                                            <PasswordRequirement
                                                meets={/[a-z]/.test(passwordValue)}
                                                label="Contains lowercase letter"
                                            />
                                            <PasswordRequirement
                                                meets={/[A-Z]/.test(passwordValue)}
                                                label="Contains uppercase letter"
                                            />
                                            <PasswordRequirement
                                                meets={/[0-9]/.test(passwordValue)}
                                                label="Contains number"
                                            />
                                        </Stack>
                                    </Box>
                                )}
                            </Box>

                            <PasswordInput
                                label="Confirm Password"
                                placeholder="Confirm your password"
                                required
                                key={form.key('confirmPassword')}
                                {...form.getInputProps('confirmPassword')}
                                disabled={registerMutation.isPending}
                            />

                            {registerMutation.error && (
                                <Alert
                                    icon={<IconAlertCircle size="1rem" />}
                                    color="red"
                                    variant="light"
                                >
                                    {registerMutation.error instanceof Error
                                        ? registerMutation.error.message
                                        : String(registerMutation.error) || 'An error occurred during registration'
                                    }
                                </Alert>
                            )}

                            <Button
                                type="submit"
                                fullWidth
                                loading={registerMutation.isPending}
                                leftSection={<IconUserPlus size="1rem" />}
                            >
                                Create Account
                            </Button>
                        </Stack>
                    </form>

                    <Divider my="lg" label="Or continue with" labelPosition="center" />

                    <Stack gap="md">
                        <OAuthButton provider="google" />
                        <OAuthButton provider="github" />
                    </Stack>

                    <Divider my="lg" />

                    <Group justify="center" gap="xs">
                        <Text size="sm" c="dimmed">
                            Already have an account?
                        </Text>
                        <Anchor component={Link} to="/login" size="sm">
                            Sign in
                        </Anchor>
                    </Group>
                </Paper>
            </Container>
        </Center>
    )
}

export default RegisterPage