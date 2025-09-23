import {
    Modal,
    Switch,
    Button,
    Stack,
    Group,
    Text,
    Alert,
    LoadingOverlay,
    Divider,
    Title
} from '@mantine/core'
import { useForm } from '@mantine/form'
import { notifications } from '@mantine/notifications'
import { IconCheck, IconX, IconEye, IconMail, IconTrophy, IconChartBar } from '@tabler/icons-react'
import { useUpdateProfileSettings } from '../../hooks/api/useUserProfile'
import type { UserProfile, UserProfileSettings } from '../../types/profile'

interface ProfileSettingsModalProps {
    opened: boolean
    onClose: () => void
    profile: UserProfile
}

const ProfileSettingsModal = ({ opened, onClose, profile }: ProfileSettingsModalProps) => {
    const updateSettingsMutation = useUpdateProfileSettings()

    const form = useForm<UserProfileSettings>({
        initialValues: {
            isProfilePublic: profile.settings?.showProfile ?? true,
            showEmail: profile.settings?.showProfile ?? false,
            showProgress: profile.settings?.showProfile ?? true,
            showAchievements: profile.settings?.showProfile ?? true
        }
    })

    const handleSubmit = async (values: UserProfileSettings) => {
        try {
            await updateSettingsMutation.mutateAsync(values)

            notifications.show({
                title: 'Settings Updated',
                message: 'Your privacy settings have been successfully updated.',
                color: 'green',
                icon: <IconCheck size={16} />
            })

            onClose()
        } catch {
            notifications.show({
                title: 'Update Failed',
                message: 'Failed to update settings. Please try again.',
                color: 'red',
                icon: <IconX size={16} />
            })
        }
    }

    const handleClose = () => {
        form.reset()
        onClose()
    }

    return (
        <Modal
            opened={opened}
            onClose={handleClose}
            title="Profile Privacy Settings"
            size="md"
            centered
        >
            <LoadingOverlay visible={updateSettingsMutation.isPending} />

            <form onSubmit={form.onSubmit(handleSubmit)}>
                <Stack gap="lg">
                    <Alert color="blue" title="Privacy Information">
                        These settings control what information is visible to other users when they view your profile.
                    </Alert>

                    {/* Profile Visibility */}
                    <Stack gap="md">
                        <Title order={5}>Profile Visibility</Title>

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
                                {...form.getInputProps('isProfilePublic', { type: 'checkbox' })}
                            />
                        </Group>

                        <Text size="xs" c="dimmed">
                            When disabled, your profile will only be visible to you and administrators.
                        </Text>
                    </Stack>

                    <Divider />

                    {/* Contact Information */}
                    <Stack gap="md">
                        <Title order={5}>Contact Information</Title>

                        <Group justify="space-between">
                            <Group gap="sm">
                                <IconMail size={20} />
                                <Stack gap={0}>
                                    <Text size="sm" fw={500}>Show Email Address</Text>
                                    <Text size="xs" c="dimmed">
                                        Display your email address on your public profile
                                    </Text>
                                </Stack>
                            </Group>
                            <Switch
                                {...form.getInputProps('showEmail', { type: 'checkbox' })}
                                disabled={!form.values.isProfilePublic}
                            />
                        </Group>

                        {!form.values.isProfilePublic && (
                            <Text size="xs" c="dimmed">
                                This setting is disabled because your profile is private.
                            </Text>
                        )}
                    </Stack>

                    <Divider />

                    {/* Learning Progress */}
                    <Stack gap="md">
                        <Title order={5}>Learning Progress</Title>

                        <Group justify="space-between">
                            <Group gap="sm">
                                <IconChartBar size={20} />
                                <Stack gap={0}>
                                    <Text size="sm" fw={500}>Show Progress Statistics</Text>
                                    <Text size="xs" c="dimmed">
                                        Display course completion, lessons completed, and learning streaks
                                    </Text>
                                </Stack>
                            </Group>
                            <Switch
                                {...form.getInputProps('showProgress', { type: 'checkbox' })}
                                disabled={!form.values.isProfilePublic}
                            />
                        </Group>
                    </Stack>

                    <Divider />

                    {/* Achievements */}
                    <Stack gap="md">
                        <Title order={5}>Achievements</Title>

                        <Group justify="space-between">
                            <Group gap="sm">
                                <IconTrophy size={20} />
                                <Stack gap={0}>
                                    <Text size="sm" fw={500}>Show Achievements</Text>
                                    <Text size="xs" c="dimmed">
                                        Display your earned achievements and badges
                                    </Text>
                                </Stack>
                            </Group>
                            <Switch
                                {...form.getInputProps('showAchievements', { type: 'checkbox' })}
                                disabled={!form.values.isProfilePublic}
                            />
                        </Group>

                        {!form.values.isProfilePublic && (
                            <Text size="xs" c="dimmed">
                                These settings are disabled because your profile is private.
                            </Text>
                        )}
                    </Stack>

                    {/* Error Display */}
                    {updateSettingsMutation.error && (
                        <Alert color="red" title="Update Failed">
                            {updateSettingsMutation.error instanceof Error
                                ? updateSettingsMutation.error.message
                                : 'An unexpected error occurred'
                            }
                        </Alert>
                    )}

                    {/* Action Buttons */}
                    <Group justify="flex-end" mt="md">
                        <Button
                            variant="light"
                            onClick={handleClose}
                            disabled={updateSettingsMutation.isPending}
                        >
                            Cancel
                        </Button>
                        <Button
                            type="submit"
                            loading={updateSettingsMutation.isPending}
                        >
                            Save Settings
                        </Button>
                    </Group>
                </Stack>
            </form>
        </Modal>
    )
}

export default ProfileSettingsModal