import {
    Modal,
    TextInput,
    Textarea,
    Button,
    Stack,
    Group,
    Avatar,
    FileInput,
    Alert,
    LoadingOverlay,
    Card,
    Text,
    Box,
    Divider,
    useMantineColorScheme,
    useMantineTheme,
    ActionIcon,
    Tooltip
} from '@mantine/core'
import { useForm } from '@mantine/form'
import { notifications } from '@mantine/notifications'
import { IconUpload, IconCheck, IconX, IconUser, IconEdit, IconPhoto } from '@tabler/icons-react'
import { useState } from 'react'
import { useUpdateProfile } from '../../hooks/api/useUserProfile'
import type { UserProfile, UpdateUserProfileRequest } from '../../types/profile'

interface EditProfileModalProps {
    opened: boolean
    onClose: () => void
    profile: UserProfile
}

const EditProfileModal = ({ opened, onClose, profile }: EditProfileModalProps) => {
    const [profileImage, setProfileImage] = useState<File | null>(null)
    const [imagePreview, setImagePreview] = useState<string | null>(null)

    const updateProfileMutation = useUpdateProfile()

    const form = useForm<UpdateUserProfileRequest>({
        initialValues: {
            firstName: profile.firstName || '',
            lastName: profile.lastName || '',
            bio: profile.bio || '',
            profilePictureUrl: profile.profilePictureUrl || ''
        },
        validate: {
            firstName: (value) => {
                if (!value || value.trim().length === 0) {
                    return 'First name is required'
                }
                if (value.trim().length < 2) {
                    return 'First name must be at least 2 characters'
                }
                if (value.trim().length > 50) {
                    return 'First name must be less than 50 characters'
                }
                return null
            },
            lastName: (value) => {
                if (!value || value.trim().length === 0) {
                    return 'Last name is required'
                }
                if (value.trim().length < 2) {
                    return 'Last name must be at least 2 characters'
                }
                if (value.trim().length > 50) {
                    return 'Last name must be less than 50 characters'
                }
                return null
            },
            bio: (value) => {
                if (value && value.length > 500) {
                    return 'Bio must be less than 500 characters'
                }
                return null
            }
        }
    })

    const handleImageChange = (file: File | null) => {
        setProfileImage(file)
        if (file) {
            const reader = new FileReader()
            reader.onload = (e) => {
                setImagePreview(e.target?.result as string)
            }
            reader.readAsDataURL(file)
        } else {
            setImagePreview(null)
        }
    }

    const uploadImage = async (): Promise<string> => {
        // This would typically upload to a cloud storage service
        // For now, we'll simulate an upload and return a placeholder URL
        return new Promise((resolve) => {
            setTimeout(() => {
                resolve(`https://api.dicebear.com/7.x/initials/svg?seed=${profile.fullName || profile.username || 'user'}`)
            }, 1000)
        })
    }

    const handleSubmit = async (values: UpdateUserProfileRequest) => {
        try {
            let profilePictureUrl = values.profilePictureUrl

            // Upload new profile image if selected
            if (profileImage) {
                profilePictureUrl = await uploadImage()
            }

            await updateProfileMutation.mutateAsync({
                ...values,
                profilePictureUrl
            })

            notifications.show({
                title: 'Profile Updated',
                message: 'Your profile has been successfully updated.',
                color: 'green',
                icon: <IconCheck size={16} />
            })

            onClose()
        } catch {
            notifications.show({
                title: 'Update Failed',
                message: 'Failed to update profile. Please try again.',
                color: 'red',
                icon: <IconX size={16} />
            })
        }
    }

    const handleClose = () => {
        form.reset()
        setProfileImage(null)
        setImagePreview(null)
        onClose()
    }

    const { colorScheme } = useMantineColorScheme()
    const theme = useMantineTheme()
    const isDark = colorScheme === 'dark'

    return (
        <Modal
            opened={opened}
            onClose={handleClose}
            title={
                <Group gap="sm">
                    <ActionIcon variant="light" size="lg" color="blue">
                        <IconEdit size={20} />
                    </ActionIcon>
                    <Text size="lg" fw={600}>Edit Profile</Text>
                </Group>
            }
            size="lg"
            centered
            radius="lg"
            overlayProps={{
                backgroundOpacity: 0.55,
                blur: 3,
            }}
            styles={{
                content: {
                    background: isDark
                        ? `linear-gradient(135deg, ${theme.colors.dark[7]} 0%, ${theme.colors.dark[6]} 100%)`
                        : `linear-gradient(135deg, ${theme.colors.blue[0]} 0%, ${theme.colors.indigo[0]} 100%)`,
                },
                header: {
                    background: 'transparent',
                    borderBottom: `1px solid ${isDark ? theme.colors.dark[4] : theme.colors.gray[2]}`,
                }
            }}
        >
            <LoadingOverlay visible={updateProfileMutation.isPending} />

            <form onSubmit={form.onSubmit(handleSubmit)}>
                <Stack gap="lg">
                    {/* Profile Picture Section */}
                    <Card
                        padding="xl"
                        radius="lg"
                        withBorder
                        bg={isDark ? theme.colors.dark[6] : 'white'}
                        style={{ border: `1px solid ${isDark ? theme.colors.dark[4] : theme.colors.gray[2]}` }}
                    >
                        <Stack gap="md" align="center">
                            <Group gap="sm" align="center">
                                <IconPhoto size={20} color={theme.colors.blue[6]} />
                                <Text size="sm" fw={600} c="dimmed">PROFILE PICTURE</Text>
                            </Group>

                            <Box pos="relative">
                                <Avatar
                                    src={imagePreview || profile.profilePictureUrl}
                                    size={120}
                                    radius="lg"
                                    style={{
                                        border: `4px solid ${isDark ? theme.colors.dark[4] : 'white'}`,
                                        boxShadow: isDark ? '0 8px 24px rgba(0,0,0,0.4)' : '0 8px 24px rgba(0,0,0,0.15)'
                                    }}
                                >
                                    {profile.fullName?.charAt(0)?.toUpperCase() || profile.username?.charAt(0)?.toUpperCase() || '?'}
                                </Avatar>

                                <Tooltip label="Upload new picture">
                                    <ActionIcon
                                        pos="absolute"
                                        bottom={-5}
                                        right={-5}
                                        size="lg"
                                        variant="gradient"
                                        gradient={{ from: 'blue', to: 'indigo' }}
                                        radius="xl"
                                        style={{ boxShadow: '0 4px 12px rgba(0,0,0,0.2)' }}
                                    >
                                        <IconUpload size={16} />
                                    </ActionIcon>
                                </Tooltip>
                            </Box>

                            <FileInput
                                placeholder="Choose a new profile picture"
                                accept="image/*"
                                leftSection={<IconUpload size={16} />}
                                onChange={handleImageChange}
                                clearable
                                style={{ width: '100%', maxWidth: 300 }}
                                styles={{
                                    input: {
                                        borderRadius: theme.radius.md,
                                        border: `1px solid ${isDark ? theme.colors.dark[4] : theme.colors.gray[3]}`,
                                    }
                                }}
                            />
                        </Stack>
                    </Card>

                    {/* Personal Information Section */}
                    <Card
                        padding="xl"
                        radius="lg"
                        withBorder
                        bg={isDark ? theme.colors.dark[6] : 'white'}
                        style={{ border: `1px solid ${isDark ? theme.colors.dark[4] : theme.colors.gray[2]}` }}
                    >
                        <Stack gap="lg">
                            <Group gap="sm" align="center">
                                <IconUser size={20} color={theme.colors.blue[6]} />
                                <Text size="sm" fw={600} c="dimmed">PERSONAL INFORMATION</Text>
                            </Group>

                            <Group grow>
                                <TextInput
                                    label="First Name"
                                    placeholder="Enter your first name"
                                    required
                                    size="md"
                                    styles={{
                                        input: {
                                            borderRadius: theme.radius.md,
                                            border: `1px solid ${isDark ? theme.colors.dark[4] : theme.colors.gray[3]}`,
                                        }
                                    }}
                                    {...form.getInputProps('firstName')}
                                />
                                <TextInput
                                    label="Last Name"
                                    placeholder="Enter your last name"
                                    required
                                    size="md"
                                    styles={{
                                        input: {
                                            borderRadius: theme.radius.md,
                                            border: `1px solid ${isDark ? theme.colors.dark[4] : theme.colors.gray[3]}`,
                                        }
                                    }}
                                    {...form.getInputProps('lastName')}
                                />
                            </Group>

                            <Textarea
                                label="Bio"
                                placeholder="Tell us about yourself... What are your interests? What are you learning?"
                                minRows={4}
                                maxRows={8}
                                size="md"
                                styles={{
                                    input: {
                                        borderRadius: theme.radius.md,
                                        border: `1px solid ${isDark ? theme.colors.dark[4] : theme.colors.gray[3]}`,
                                    }
                                }}
                                {...form.getInputProps('bio')}
                            />
                        </Stack>
                    </Card>

                    {/* Error Display */}
                    {updateProfileMutation.error && (
                        <Alert
                            color="red"
                            title="Update Failed"
                            radius="lg"
                            styles={{
                                root: {
                                    border: `1px solid ${theme.colors.red[3]}`,
                                }
                            }}
                        >
                            {updateProfileMutation.error instanceof Error
                                ? updateProfileMutation.error.message
                                : 'An unexpected error occurred'
                            }
                        </Alert>
                    )}

                    <Divider color={isDark ? theme.colors.dark[4] : theme.colors.gray[2]} />

                    {/* Action Buttons */}
                    <Group justify="flex-end" gap="md">
                        <Button
                            variant="light"
                            onClick={handleClose}
                            disabled={updateProfileMutation.isPending}
                            size="md"
                            radius="md"
                        >
                            Cancel
                        </Button>
                        <Button
                            type="submit"
                            loading={updateProfileMutation.isPending}
                            variant="gradient"
                            gradient={{ from: 'blue', to: 'indigo' }}
                            size="md"
                            radius="md"
                            leftSection={<IconCheck size={16} />}
                        >
                            Save Changes
                        </Button>
                    </Group>
                </Stack>
            </form>
        </Modal>
    )
}

export default EditProfileModal