import {
    Card,
    Avatar,
    Text,
    Badge,
    Group,
    Stack,
    Button,
    ActionIcon,
    Tooltip,
    SimpleGrid,
    Box,
    Title,
    useMantineColorScheme,
    useMantineTheme
} from '@mantine/core'
import {
    IconEdit,
    IconMail,
    IconCalendar,
    IconTrophy,
    IconBook,
    IconClock,
    IconFlame,
    IconSettings,
    IconEye,
    IconEyeOff
} from '@tabler/icons-react'
import { useState } from 'react'
import dayjs from 'dayjs'
import type { UserProfile, PublicUserProfile } from '../../types/profile'

interface UserProfileCardProps {
    profile: UserProfile | PublicUserProfile
    isOwnProfile?: boolean
    onEdit?: () => void
    onSettingsClick?: () => void
}

const UserProfileCard = ({ profile, isOwnProfile = false, onEdit, onSettingsClick }: UserProfileCardProps) => {
    const [showEmail, setShowEmail] = useState(false)

    const isFullProfile = (p: UserProfile | PublicUserProfile): p is UserProfile => {
        return 'email' in p && 'statistics' in p
    }

    const fullProfile = isFullProfile(profile) ? profile : null


    const { colorScheme } = useMantineColorScheme()
    const theme = useMantineTheme()
    const isDark = colorScheme === 'dark'

    return (
        <Card
            shadow="md"
            padding="xl"
            radius="lg"
            withBorder
            style={{
                background: isDark
                    ? `linear-gradient(135deg, ${theme.colors.dark[7]} 0%, ${theme.colors.dark[6]} 100%)`
                    : `linear-gradient(135deg, ${theme.colors.blue[0]} 0%, ${theme.colors.indigo[0]} 100%)`,
                border: `1px solid ${isDark ? theme.colors.dark[4] : theme.colors.gray[2]}`
            }}
        >
            {/* Header Section */}
            <Group justify="space-between" mb="lg">
                <Group>
                    <Avatar
                        src={profile.profilePictureUrl}
                        size="xl"
                        radius="lg"
                        style={{
                            border: `3px solid ${isDark ? theme.colors.dark[5] : 'white'}`,
                            boxShadow: isDark ? '0 4px 12px rgba(0,0,0,0.3)' : '0 4px 12px rgba(0,0,0,0.1)'
                        }}
                    >
                        {profile.fullName?.charAt(0)?.toUpperCase() || profile.username?.charAt(0)?.toUpperCase() || '?'}
                    </Avatar>
                    <Stack gap="xs">
                        <Title order={2} size="h3">{profile.fullName || profile.username || 'Unknown User'}</Title>
                        <Text size="sm" c="dimmed" fw={500}>@{profile.username}</Text>
                        {fullProfile?.roles && fullProfile.roles.length > 0 && (
                            <Group gap="xs">
                                {fullProfile.roles.map((role: string) => (
                                    <Badge key={role} variant="filled" size="sm" color="blue">
                                        {role}
                                    </Badge>
                                ))}
                            </Group>
                        )}
                    </Stack>
                </Group>

                {isOwnProfile && (
                    <Group>
                        <Tooltip label="Profile Settings">
                            <ActionIcon
                                variant="light"
                                size="lg"
                                onClick={onSettingsClick}
                            >
                                <IconSettings size={18} />
                            </ActionIcon>
                        </Tooltip>
                        <Button
                            leftSection={<IconEdit size={16} />}
                            variant="gradient"
                            gradient={{ from: 'blue', to: 'indigo' }}
                            onClick={onEdit}
                        >
                            Edit Profile
                        </Button>
                    </Group>
                )}
            </Group>

            {/* Bio Section */}
            {profile.bio && (
                <Box mb="lg">
                    <Text size="sm" style={{ lineHeight: 1.6 }}>{profile.bio}</Text>
                </Box>
            )}

            {/* Contact Info */}
            <Group mb="lg" gap="lg">
                {fullProfile?.email && (isOwnProfile || fullProfile.settings?.showProfile) && (
                    <Group gap="xs">
                        <IconMail size={16} color={theme.colors.blue[6]} />
                        <Text size="sm" c="dimmed">
                            {showEmail ? fullProfile.email : '••••••@••••.com'}
                        </Text>
                        <ActionIcon
                            size="sm"
                            variant="subtle"
                            onClick={() => setShowEmail(!showEmail)}
                        >
                            {showEmail ? <IconEyeOff size={12} /> : <IconEye size={12} />}
                        </ActionIcon>
                    </Group>
                )}
                <Group gap="xs">
                    <IconCalendar size={16} color={theme.colors.blue[6]} />
                    <Text size="sm" c="dimmed">
                        Joined {dayjs('createdAt' in profile ? profile.createdAt : profile.joinedDate).format('MMMM YYYY')}
                    </Text>
                </Group>
            </Group>

            {/* Compact Statistics Grid */}
            {(isOwnProfile || (fullProfile?.settings?.showProfile !== false)) && fullProfile && (
                <SimpleGrid cols={4} spacing="md" mb="lg">
                    <Card
                        padding="md"
                        radius="md"
                        withBorder
                        bg={isDark ? theme.colors.dark[6] : 'white'}
                        style={{ border: `1px solid ${isDark ? theme.colors.dark[4] : theme.colors.gray[2]}` }}
                    >
                        <Stack gap="xs" align="center">
                            <IconBook size={20} color={theme.colors.blue[6]} />
                            <Text size="xl" fw={700} c="blue">
                                {fullProfile.statistics?.coursesCompleted || 0}
                            </Text>
                            <Text size="xs" c="dimmed" ta="center">Courses</Text>
                        </Stack>
                    </Card>

                    <Card
                        padding="md"
                        radius="md"
                        withBorder
                        bg={isDark ? theme.colors.dark[6] : 'white'}
                        style={{ border: `1px solid ${isDark ? theme.colors.dark[4] : theme.colors.gray[2]}` }}
                    >
                        <Stack gap="xs" align="center">
                            <IconTrophy size={20} color={theme.colors.green[6]} />
                            <Text size="xl" fw={700} c="green">
                                {fullProfile.statistics?.lessonsCompleted || 0}
                            </Text>
                            <Text size="xs" c="dimmed" ta="center">Lessons</Text>
                        </Stack>
                    </Card>

                    <Card
                        padding="md"
                        radius="md"
                        withBorder
                        bg={isDark ? theme.colors.dark[6] : 'white'}
                        style={{ border: `1px solid ${isDark ? theme.colors.dark[4] : theme.colors.gray[2]}` }}
                    >
                        <Stack gap="xs" align="center">
                            <IconClock size={20} color={theme.colors.orange[6]} />
                            <Text size="xl" fw={700} c="orange">
                                {fullProfile.statistics?.totalPoints || 0}
                            </Text>
                            <Text size="xs" c="dimmed" ta="center">Points</Text>
                        </Stack>
                    </Card>

                    <Card
                        padding="md"
                        radius="md"
                        withBorder
                        bg={isDark ? theme.colors.dark[6] : 'white'}
                        style={{ border: `1px solid ${isDark ? theme.colors.dark[4] : theme.colors.gray[2]}` }}
                    >
                        <Stack gap="xs" align="center">
                            <IconFlame size={20} color={theme.colors.red[6]} />
                            <Text size="xl" fw={700} c="red">
                                {fullProfile.statistics?.currentStreak || 0}
                            </Text>
                            <Text size="xs" c="dimmed" ta="center">Day Streak</Text>
                        </Stack>
                    </Card>
                </SimpleGrid>
            )}

            {/* Achievements Section */}
            {(isOwnProfile || (fullProfile?.settings?.showProfile !== false)) && fullProfile?.achievements && fullProfile.achievements.length > 0 && (
                <Box>
                    <Group justify="space-between" mb="sm">
                        <Text size="sm" fw={600} c="dimmed">RECENT ACHIEVEMENTS</Text>
                        <Badge variant="light" size="sm" color="yellow">
                            {fullProfile.achievements.length}
                        </Badge>
                    </Group>
                    <Group gap="xs">
                        {fullProfile.achievements.slice(0, 4).map((achievement) => (
                            <Tooltip key={achievement.id} label={achievement.description}>
                                <Badge
                                    variant="gradient"
                                    gradient={{ from: 'yellow', to: 'orange' }}
                                    size="sm"
                                    leftSection={<IconTrophy size={12} />}
                                >
                                    {achievement.name}
                                </Badge>
                            </Tooltip>
                        ))}
                        {fullProfile.achievements.length > 4 && (
                            <Badge variant="outline" size="sm">
                                +{fullProfile.achievements.length - 4} more
                            </Badge>
                        )}
                    </Group>
                </Box>
            )}
        </Card>
    )
}

export default UserProfileCard