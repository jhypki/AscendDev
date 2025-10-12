import React from 'react'
import {
    Container,
    Stack,
    Title,
    Card,
    Switch,
    Group,
    Text,
    Button,
    Alert,
    LoadingOverlay,
    Divider,
    ActionIcon,
    Modal,
    Center,
    Loader
} from '@mantine/core'
import { useForm } from '@mantine/form'
import { useDisclosure } from '@mantine/hooks'
import { notifications } from '@mantine/notifications'
import {
    IconSettings,
    IconEye,
    IconMail,
    IconShare,
    IconTrash,
    IconCheck,
    IconX,
    IconShield,
    IconBell,
    IconWorld,
    IconMailCheck
} from '@tabler/icons-react'
import {
    useUserSettings,
    useUpdateUserSettings,
    useDeleteUserSettings
} from '../../hooks/api/useSettings'
import { useResendVerification } from '../../hooks/api/useAuth'
import { useAppSelector } from '../../store/hooks'
import type { UpdateUserSettingsRequest } from '../../types/settings'

const SettingsPage = () => {
    const [deleteModalOpened, { open: openDeleteModal, close: closeDeleteModal }] = useDisclosure(false)

    const { data: settings, isLoading, error } = useUserSettings()
    const updateSettingsMutation = useUpdateUserSettings()
    const deleteSettingsMutation = useDeleteUserSettings()
    const resendVerificationMutation = useResendVerification()
    const { user } = useAppSelector((state) => state.auth)

    const form = useForm<UpdateUserSettingsRequest>({
        initialValues: {
            publicSubmissions: settings?.publicSubmissions ?? false,
            showProfile: settings?.showProfile ?? true,
            emailOnCodeReview: settings?.emailOnCodeReview ?? true,
            emailOnDiscussionReply: settings?.emailOnDiscussionReply ?? true
        }
    })

    // Update form when settings data loads
    React.useEffect(() => {
        if (settings) {
            form.setValues({
                publicSubmissions: settings.publicSubmissions,
                showProfile: settings.showProfile,
                emailOnCodeReview: settings.emailOnCodeReview,
                emailOnDiscussionReply: settings.emailOnDiscussionReply
            })
        }
    }, [settings])

    const handleSubmit = async (values: UpdateUserSettingsRequest) => {
        try {
            await updateSettingsMutation.mutateAsync(values)

            notifications.show({
                title: 'Settings Updated',
                message: 'Your settings have been successfully updated.',
                color: 'green',
                icon: <IconCheck size={16} />
            })
        } catch {
            notifications.show({
                title: 'Update Failed',
                message: 'Failed to update settings. Please try again.',
                color: 'red',
                icon: <IconX size={16} />
            })
        }
    }

    const handleResetSettings = async () => {
        try {
            await deleteSettingsMutation.mutateAsync()

            notifications.show({
                title: 'Settings Reset',
                message: 'Your settings have been reset to defaults.',
                color: 'blue',
                icon: <IconCheck size={16} />
            })

            closeDeleteModal()
        } catch {
            notifications.show({
                title: 'Reset Failed',
                message: 'Failed to reset settings. Please try again.',
                color: 'red',
                icon: <IconX size={16} />
            })
        }
    }

    const handleResendVerification = async () => {
        if (!user?.email) {
            notifications.show({
                title: 'Error',
                message: 'User email not found. Please try logging in again.',
                color: 'red',
                icon: <IconX size={16} />
            })
            return
        }

        try {
            await resendVerificationMutation.mutateAsync(user.email)
            notifications.show({
                title: 'Verification Email Sent',
                message: 'A new verification email has been sent to your email address.',
                color: 'green',
                icon: <IconCheck size={16} />
            })
        } catch {
            notifications.show({
                title: 'Failed to Send Email',
                message: 'Unable to send verification email. Please try again later.',
                color: 'red',
                icon: <IconX size={16} />
            })
        }
    }

    if (isLoading) {
        return (
            <Container size="md" py="xl">
                <Center>
                    <Loader size="lg" />
                </Center>
            </Container>
        )
    }

    if (error) {
        return (
            <Container size="md" py="xl">
                <Alert
                    color="red"
                    title="Error Loading Settings"
                    icon={<IconX size={16} />}
                >
                    {error instanceof Error ? error.message : 'Failed to load settings'}
                </Alert>
            </Container>
        )
    }

    return (
        <Container size="md" py="xl">
            <Stack gap="xl">
                {/* Header */}
                <Group justify="space-between" align="center">
                    <Group gap="sm">
                        <IconSettings size={32} />
                        <Title order={1}>Settings</Title>
                    </Group>
                    <ActionIcon
                        variant="light"
                        color="red"
                        size="lg"
                        onClick={openDeleteModal}
                        title="Reset to defaults"
                    >
                        <IconTrash size={18} />
                    </ActionIcon>
                </Group>

                <form onSubmit={form.onSubmit(handleSubmit)}>
                    <Stack gap="lg">
                        {/* Privacy Settings */}
                        <Card shadow="sm" padding="lg" radius="md" withBorder>
                            <LoadingOverlay visible={updateSettingsMutation.isPending} />

                            <Stack gap="md">
                                <Group gap="sm">
                                    <IconShield size={24} />
                                    <Title order={3}>Privacy Settings</Title>
                                </Group>

                                <Text size="sm" c="dimmed">
                                    Control what information is visible to other users and how your data is shared.
                                </Text>

                                <Divider />

                                {/* Show Profile */}
                                <Group justify="space-between">
                                    <Group gap="sm">
                                        <IconEye size={20} />
                                        <Stack gap={0}>
                                            <Text size="sm" fw={500}>Public Profile</Text>
                                            <Text size="xs" c="dimmed">
                                                Allow other users to find and view your profile
                                            </Text>
                                        </Stack>
                                    </Group>
                                    <Switch
                                        {...form.getInputProps('showProfile', { type: 'checkbox' })}
                                    />
                                </Group>

                                {/* Public Submissions */}
                                <Group justify="space-between">
                                    <Group gap="sm">
                                        <IconShare size={20} />
                                        <Stack gap={0}>
                                            <Text size="sm" fw={500}>Public Submissions</Text>
                                            <Text size="xs" c="dimmed">
                                                Allow your code submissions to be visible to other users
                                            </Text>
                                        </Stack>
                                    </Group>
                                    <Switch
                                        {...form.getInputProps('publicSubmissions', { type: 'checkbox' })}
                                        disabled={!form.values.showProfile}
                                    />
                                </Group>

                                {!form.values.showProfile && (
                                    <Alert color="yellow">
                                        Public submissions are disabled because your profile is private.
                                    </Alert>
                                )}
                            </Stack>
                        </Card>

                        {/* Notification Settings */}
                        <Card shadow="sm" padding="lg" radius="md" withBorder>
                            <Stack gap="md">
                                <Group gap="sm">
                                    <IconBell size={24} />
                                    <Title order={3}>Email Notifications</Title>
                                </Group>

                                <Text size="sm" c="dimmed">
                                    Choose when you want to receive email notifications about platform activities.
                                </Text>

                                <Divider />

                                {/* Code Review Notifications */}
                                <Group justify="space-between">
                                    <Group gap="sm">
                                        <IconMail size={20} />
                                        <Stack gap={0}>
                                            <Text size="sm" fw={500}>Code Review Notifications</Text>
                                            <Text size="xs" c="dimmed">
                                                Receive emails when someone reviews your code
                                            </Text>
                                        </Stack>
                                    </Group>
                                    <Switch
                                        {...form.getInputProps('emailOnCodeReview', { type: 'checkbox' })}
                                    />
                                </Group>

                                {/* Discussion Reply Notifications */}
                                <Group justify="space-between">
                                    <Group gap="sm">
                                        <IconWorld size={20} />
                                        <Stack gap={0}>
                                            <Text size="sm" fw={500}>Discussion Reply Notifications</Text>
                                            <Text size="xs" c="dimmed">
                                                Receive emails when someone replies to your discussions
                                            </Text>
                                        </Stack>
                                    </Group>
                                    <Switch
                                        {...form.getInputProps('emailOnDiscussionReply', { type: 'checkbox' })}
                                    />
                                </Group>
                            </Stack>
                        </Card>

                        {/* Account Security */}
                        <Card shadow="sm" padding="lg" radius="md" withBorder>
                            <Stack gap="md">
                                <Group gap="sm">
                                    <IconShield size={24} />
                                    <Title order={3}>Account Security</Title>
                                </Group>

                                <Text size="sm" c="dimmed">
                                    Manage your account security settings and email verification status.
                                </Text>

                                <Divider />

                                {/* Email Verification Status */}
                                <Group justify="space-between">
                                    <Group gap="sm">
                                        <IconMailCheck size={20} />
                                        <Stack gap={0}>
                                            <Text size="sm" fw={500}>Email Verification</Text>
                                            <Text size="xs" c="dimmed">
                                                {user?.isEmailVerified
                                                    ? 'Your email address is verified'
                                                    : 'Your email address needs verification'
                                                }
                                            </Text>
                                        </Stack>
                                    </Group>
                                    {user?.isEmailVerified ? (
                                        <Text size="sm" c="green" fw={500}>
                                            ✓ Verified
                                        </Text>
                                    ) : (
                                        <Button
                                            size="xs"
                                            variant="light"
                                            color="blue"
                                            onClick={handleResendVerification}
                                            loading={resendVerificationMutation.isPending}
                                            leftSection={<IconMail size={14} />}
                                        >
                                            Resend Verification
                                        </Button>
                                    )}
                                </Group>

                                {!user?.isEmailVerified && (
                                    <Alert color="yellow">
                                        Please verify your email address to access all platform features.
                                        Check your inbox for the verification email.
                                    </Alert>
                                )}
                            </Stack>
                        </Card>

                        {/* Error Display */}
                        {updateSettingsMutation.error && (
                            <Alert color="red" title="Update Failed">
                                {updateSettingsMutation.error instanceof Error
                                    ? updateSettingsMutation.error.message
                                    : 'An unexpected error occurred'
                                }
                            </Alert>
                        )}

                        {/* Save Button */}
                        <Group justify="flex-end">
                            <Button
                                type="submit"
                                loading={updateSettingsMutation.isPending}
                                leftSection={<IconCheck size={16} />}
                            >
                                Save Settings
                            </Button>
                        </Group>
                    </Stack>
                </form>
            </Stack>

            {/* Reset Confirmation Modal */}
            <Modal
                opened={deleteModalOpened}
                onClose={closeDeleteModal}
                title="Reset Settings"
                centered
            >
                <Stack gap="md">
                    <Text>
                        Are you sure you want to reset all settings to their default values?
                        This action cannot be undone.
                    </Text>

                    <Group justify="flex-end">
                        <Button
                            variant="light"
                            onClick={closeDeleteModal}
                            disabled={deleteSettingsMutation.isPending}
                        >
                            Cancel
                        </Button>
                        <Button
                            color="red"
                            onClick={handleResetSettings}
                            loading={deleteSettingsMutation.isPending}
                            leftSection={<IconTrash size={16} />}
                        >
                            Reset Settings
                        </Button>
                    </Group>
                </Stack>
            </Modal>
        </Container>
    )
}

export default SettingsPage