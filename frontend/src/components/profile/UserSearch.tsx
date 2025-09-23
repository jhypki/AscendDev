import {
    Card,
    TextInput,
    Stack,
    Group,
    Avatar,
    Text,
    Badge,
    Button, Center,
    Loader,
    Alert,
    ActionIcon,
    Tooltip,
    SimpleGrid
} from '@mantine/core'
import {
    IconSearch,
    IconUser,
    IconTrophy,
    IconBook,
    IconClock,
    IconRefresh
} from '@tabler/icons-react'
import { useState } from 'react'
import { useDebouncedValue } from '@mantine/hooks'
import { useUserSearch } from '../../hooks/api/useUserProfile'
import type { UserSearchRequest, PublicUserProfile } from '../../types/profile'
import dayjs from 'dayjs'

interface UserSearchProps {
    onUserSelect?: (user: PublicUserProfile) => void
    showUserCards?: boolean
    pageSize?: number
}

const UserSearch = ({ onUserSelect, showUserCards = true, pageSize = 12 }: UserSearchProps) => {
    const [searchQuery, setSearchQuery] = useState('')
    const [currentPage, setCurrentPage] = useState(1)
    const [debouncedQuery] = useDebouncedValue(searchQuery, 300)

    const searchMutation = useUserSearch()

    const handleSearch = (query: string, page = 1) => {
        if (query.trim().length === 0) return

        const searchRequest: UserSearchRequest = {
            query: query.trim(),
            page,
            pageSize
        }

        searchMutation.mutate(searchRequest)
    }

    const handleQueryChange = (value: string) => {
        setSearchQuery(value)
        setCurrentPage(1)
        if (value.trim().length >= 2) {
            handleSearch(value, 1)
        }
    }


    const handleRefresh = () => {
        if (debouncedQuery.trim().length >= 2) {
            handleSearch(debouncedQuery, currentPage)
        }
    }

    const UserCard = ({ user }: { user: PublicUserProfile }) => (
        <Card shadow="sm" padding="md" radius="md" withBorder>
            <Stack gap="sm">
                <Group>
                    <Avatar
                        src={user.profilePictureUrl}
                        size="md"
                        radius="md"
                    >
                        {user.fullName?.charAt(0)?.toUpperCase() || user.username?.charAt(0)?.toUpperCase() || '?'}
                    </Avatar>
                    <Stack gap={0} style={{ flex: 1 }}>
                        <Text size="sm" fw={500}>{user.fullName || user.username || 'Unknown User'}</Text>
                        <Text size="xs" c="dimmed">@{user.username}</Text>
                    </Stack>
                </Group>

                {user.bio && (
                    <Text size="xs" c="dimmed" lineClamp={2}>
                        {user.bio}
                    </Text>
                )}

                {/* Statistics */}
                <Group gap="xs" justify="center">
                    {user.coursesCompleted > 0 && (
                        <Tooltip label="Courses Completed">
                            <Badge size="xs" variant="light" leftSection={<IconBook size={10} />}>
                                {user.coursesCompleted}
                            </Badge>
                        </Tooltip>
                    )}
                    {user.lessonsCompleted > 0 && (
                        <Tooltip label="Lessons Completed">
                            <Badge size="xs" variant="light" leftSection={<IconTrophy size={10} />}>
                                {user.lessonsCompleted}
                            </Badge>
                        </Tooltip>
                    )}
                    {user.totalPoints > 0 && (
                        <Tooltip label="Total Points">
                            <Badge size="xs" variant="light" leftSection={<IconTrophy size={10} />}>
                                {user.totalPoints}
                            </Badge>
                        </Tooltip>
                    )}
                    {user.currentStreak > 0 && (
                        <Tooltip label="Current Streak">
                            <Badge size="xs" variant="light" leftSection={<IconClock size={10} />}>
                                {user.currentStreak}d
                            </Badge>
                        </Tooltip>
                    )}
                </Group>

                <Group justify="space-between" align="center">
                    <Text size="xs" c="dimmed">
                        Joined {dayjs(user.joinedDate).format('MMM YYYY')}
                    </Text>
                    {onUserSelect && (
                        <Button
                            size="xs"
                            variant="light"
                            onClick={() => onUserSelect(user)}
                        >
                            View Profile
                        </Button>
                    )}
                </Group>
            </Stack>
        </Card>
    )

    const UserListItem = ({ user }: { user: PublicUserProfile }) => (
        <Group justify="space-between" p="sm">
            <Group>
                <Avatar
                    src={user.profilePictureUrl}
                    size="sm"
                    radius="md"
                >
                    {user.fullName?.charAt(0)?.toUpperCase() || user.username?.charAt(0)?.toUpperCase() || '?'}
                </Avatar>
                <Stack gap={0}>
                    <Text size="sm" fw={500}>{user.fullName || user.username || 'Unknown User'}</Text>
                    <Text size="xs" c="dimmed">@{user.username}</Text>
                </Stack>
            </Group>
            {onUserSelect && (
                <Button
                    size="xs"
                    variant="light"
                    onClick={() => onUserSelect(user)}
                >
                    View
                </Button>
            )}
        </Group>
    )

    return (
        <Card shadow="sm" padding="lg" radius="md" withBorder>
            <Stack gap="md">
                <Group>
                    <TextInput
                        placeholder="Search users by name or username..."
                        leftSection={<IconSearch size={16} />}
                        value={searchQuery}
                        onChange={(e) => handleQueryChange(e.target.value)}
                        style={{ flex: 1 }}
                    />
                    <Tooltip label="Refresh Results">
                        <ActionIcon
                            variant="light"
                            onClick={handleRefresh}
                            loading={searchMutation.isPending}
                        >
                            <IconRefresh size={16} />
                        </ActionIcon>
                    </Tooltip>
                </Group>

                {searchMutation.isPending && (
                    <Center py="xl">
                        <Loader size="md" />
                    </Center>
                )}

                {searchMutation.error && (
                    <Alert color="red" title="Search Failed">
                        Failed to search users. Please try again.
                    </Alert>
                )}

                {searchQuery.trim().length > 0 && searchQuery.trim().length < 2 && (
                    <Alert color="blue" title="Search Tip">
                        Enter at least 2 characters to search for users.
                    </Alert>
                )}

                {searchMutation.data && (!searchMutation.data.profiles || searchMutation.data.profiles.length === 0) && searchQuery.trim().length >= 2 && (
                    <Center py="xl">
                        <Stack align="center" gap="md">
                            <IconUser size={48} stroke={1} color="gray" />
                            <Text c="dimmed">No users found</Text>
                            <Text size="sm" c="dimmed" ta="center">
                                Try searching with different keywords.
                            </Text>
                        </Stack>
                    </Center>
                )}

                {searchMutation.data && searchMutation.data.profiles && searchMutation.data.profiles.length > 0 && (
                    <Stack gap="md">
                        {showUserCards ? (
                            <SimpleGrid cols={{ base: 1, sm: 2, md: 3 }} spacing="md">
                                {searchMutation.data?.profiles?.map((user) => (
                                    <UserCard key={user.id} user={user} />
                                ))}
                            </SimpleGrid>
                        ) : (
                            <Stack gap={0}>
                                {searchMutation.data?.profiles?.map((user) => (
                                    <UserListItem key={user.id} user={user} />
                                ))}
                            </Stack>
                        )}

                        <Group justify="space-between">
                            <Text size="xs" c="dimmed">
                                Showing {searchMutation.data?.profiles?.length || 0} of {searchMutation.data?.totalCount || 0} users
                            </Text>
                            {searchMutation.data?.hasMore && (
                                <Text size="xs" c="dimmed">
                                    More results available
                                </Text>
                            )}
                        </Group>
                    </Stack>
                )}
            </Stack>
        </Card>
    )
}

export default UserSearch