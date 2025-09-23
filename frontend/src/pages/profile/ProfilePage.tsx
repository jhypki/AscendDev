import {
    Container,
    Grid,
    Stack,
    Tabs,
    Alert,
    Center,
    Loader,
    Card,
    Text,
    Group,
    Badge,
    SimpleGrid
} from '@mantine/core'
import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { IconUser, IconActivity, IconSearch, IconX, IconTrophy, IconCalendar } from '@tabler/icons-react'
import dayjs from 'dayjs'
import { useMyProfile, useUserProfile } from '../../hooks/api/useUserProfile'
import {
    UserProfileCard,
    ActivityFeed,
    EditProfileModal,
    ProfileSettingsModal,
    UserSearch
} from '../../components/profile'

const ProfilePage = () => {
    const { userId } = useParams<{ userId?: string }>()
    const [editModalOpened, setEditModalOpened] = useState(false)
    const [settingsModalOpened, setSettingsModalOpened] = useState(false)

    // Determine if viewing own profile or another user's profile
    const isOwnProfile = !userId

    // Fetch appropriate profile data
    const {
        data: myProfile,
        isLoading: myProfileLoading,
        error: myProfileError
    } = useMyProfile()

    const {
        data: userProfile,
        isLoading: userProfileLoading,
        error: userProfileError
    } = useUserProfile(userId || '', !isOwnProfile)

    const profile = isOwnProfile ? myProfile : userProfile
    const isLoading = isOwnProfile ? myProfileLoading : userProfileLoading
    const error = isOwnProfile ? myProfileError : userProfileError

    // Helper functions for type-safe access
    const getStatistics = () => {
        if (!profile) return null
        if ('statistics' in profile) return profile.statistics
        return {
            coursesCompleted: (profile as any).coursesCompleted || 0,
            lessonsCompleted: (profile as any).lessonsCompleted || 0,
            totalPoints: (profile as any).totalPoints || 0,
            currentStreak: (profile as any).currentStreak || 0,
            longestStreak: 0
        }
    }

    const getAchievements = () => {
        if (!profile || !('achievements' in profile)) return []
        return profile.achievements || []
    }

    const getRoles = () => {
        if (!profile || !('roles' in profile)) return []
        return profile.roles || []
    }

    const stats = getStatistics()
    const achievements = getAchievements()
    const roles = getRoles()

    if (isLoading) {
        return (
            <Container size="xl" py="xl">
                <Center>
                    <Loader size="lg" />
                </Center>
            </Container>
        )
    }

    if (error) {
        return (
            <Container size="xl" py="xl">
                <Alert
                    color="red"
                    title="Error Loading Profile"
                    icon={<IconX size={16} />}
                >
                    {error instanceof Error ? error.message : 'Failed to load profile data'}
                </Alert>
            </Container>
        )
    }

    if (!profile) {
        return (
            <Container size="xl" py="xl">
                <Alert
                    color="yellow"
                    title="Profile Not Found"
                    icon={<IconUser size={16} />}
                >
                    The requested profile could not be found.
                </Alert>
            </Container>
        )
    }

    return (
        <Container size="xl" py="md">
            <Stack gap="lg">
                {/* Header Section with Profile Overview */}
                <UserProfileCard
                    profile={profile}
                    isOwnProfile={isOwnProfile}
                    onEdit={() => setEditModalOpened(true)}
                    onSettingsClick={() => setSettingsModalOpened(true)}
                />

                {/* Main Content Area */}
                <Grid gutter="lg">
                    <Grid.Col span={{ base: 12, lg: 8 }}>
                        <Tabs defaultValue="activity" variant="outline">
                            <Tabs.List grow>
                                <Tabs.Tab value="activity" leftSection={<IconActivity size={16} />}>
                                    Activity Feed
                                </Tabs.Tab>
                                {isOwnProfile && (
                                    <Tabs.Tab value="search" leftSection={<IconSearch size={16} />}>
                                        Discover Users
                                    </Tabs.Tab>
                                )}
                            </Tabs.List>

                            <Tabs.Panel value="activity" pt="lg">
                                <ActivityFeed
                                    userId={isOwnProfile ? undefined : userId}
                                    showFilters={isOwnProfile}
                                />
                            </Tabs.Panel>

                            {isOwnProfile && (
                                <Tabs.Panel value="search" pt="lg">
                                    <UserSearch
                                        onUserSelect={(user) => {
                                            // Navigate to user profile
                                            window.location.href = `/profile/${user.id}`
                                        }}
                                    />
                                </Tabs.Panel>
                            )}
                        </Tabs>
                    </Grid.Col>

                    <Grid.Col span={{ base: 12, lg: 4 }}>
                        <Stack gap="md">
                            {/* Quick Stats Card */}
                            {profile && (
                                <Card shadow="sm" padding="lg" radius="md" withBorder>
                                    <Stack gap="md">
                                        <Group justify="space-between">
                                            <Text size="sm" fw={600} c="dimmed">QUICK STATS</Text>
                                            <IconTrophy size={16} />
                                        </Group>
                                        <SimpleGrid cols={2} spacing="xs">
                                            <Stack gap="xs" align="center">
                                                <Text size="xl" fw={700} c="blue">
                                                    {stats?.coursesCompleted || 0}
                                                </Text>
                                                <Text size="xs" c="dimmed" ta="center">Courses</Text>
                                            </Stack>
                                            <Stack gap="xs" align="center">
                                                <Text size="xl" fw={700} c="green">
                                                    {stats?.lessonsCompleted || 0}
                                                </Text>
                                                <Text size="xs" c="dimmed" ta="center">Lessons</Text>
                                            </Stack>
                                            <Stack gap="xs" align="center">
                                                <Text size="xl" fw={700} c="orange">
                                                    {stats?.totalPoints || 0}
                                                </Text>
                                                <Text size="xs" c="dimmed" ta="center">Points</Text>
                                            </Stack>
                                            <Stack gap="xs" align="center">
                                                <Text size="xl" fw={700} c="red">
                                                    {stats?.currentStreak || 0}
                                                </Text>
                                                <Text size="xs" c="dimmed" ta="center">Day Streak</Text>
                                            </Stack>
                                        </SimpleGrid>
                                    </Stack>
                                </Card>
                            )}

                            {/* Recent Achievements */}
                            {achievements.length > 0 && (
                                <Card shadow="sm" padding="lg" radius="md" withBorder>
                                    <Stack gap="md">
                                        <Group justify="space-between">
                                            <Text size="sm" fw={600} c="dimmed">ACHIEVEMENTS</Text>
                                            <Badge variant="light" size="sm">
                                                {achievements.length}
                                            </Badge>
                                        </Group>
                                        <Stack gap="xs">
                                            {achievements.slice(0, 3).map((achievement) => (
                                                <Group key={achievement.id} gap="xs">
                                                    <IconTrophy size={14} color="gold" />
                                                    <Text size="sm" style={{ flex: 1 }}>
                                                        {achievement.name}
                                                    </Text>
                                                </Group>
                                            ))}
                                            {achievements.length > 3 && (
                                                <Text size="xs" c="dimmed" ta="center">
                                                    +{achievements.length - 3} more achievements
                                                </Text>
                                            )}
                                        </Stack>
                                    </Stack>
                                </Card>
                            )}

                            {/* Profile Info Card */}
                            <Card shadow="sm" padding="lg" radius="md" withBorder>
                                <Stack gap="md">
                                    <Group justify="space-between">
                                        <Text size="sm" fw={600} c="dimmed">PROFILE INFO</Text>
                                        <IconUser size={16} />
                                    </Group>
                                    <Stack gap="xs">
                                        <Group gap="xs">
                                            <IconCalendar size={14} />
                                            <Text size="sm">
                                                Joined {dayjs('createdAt' in profile ? profile.createdAt : profile.joinedDate).format('MMMM YYYY')}
                                            </Text>
                                        </Group>
                                        {roles.length > 0 && (
                                            <Group gap="xs">
                                                <Text size="sm" c="dimmed">Roles:</Text>
                                                <Group gap="xs">
                                                    {roles.map((role: string) => (
                                                        <Badge key={role} variant="light" size="xs">
                                                            {role}
                                                        </Badge>
                                                    ))}
                                                </Group>
                                            </Group>
                                        )}
                                    </Stack>
                                </Stack>
                            </Card>
                        </Stack>
                    </Grid.Col>
                </Grid>
            </Stack>

            {/* Modals */}
            {isOwnProfile && myProfile && (
                <>
                    <EditProfileModal
                        opened={editModalOpened}
                        onClose={() => setEditModalOpened(false)}
                        profile={myProfile}
                    />
                    <ProfileSettingsModal
                        opened={settingsModalOpened}
                        onClose={() => setSettingsModalOpened(false)}
                        profile={myProfile}
                    />
                </>
            )}
        </Container>
    )
}

export default ProfilePage