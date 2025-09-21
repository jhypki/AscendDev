import {
    Container,
    Title,
    Text,
    Grid,
    Card,
    Group,
    Stack,
    Badge,
    ActionIcon,
    Button,
    TextInput, Switch,
    Modal,
    MultiSelect,
    Center,
    Loader,
    Alert,
    SimpleGrid,
    ThemeIcon,
    Progress,
    Tabs,
    Paper
} from '@mantine/core'
import {
    IconUsers,
    IconBook,
    IconTrendingUp,
    IconServer,
    IconSearch,
    IconEdit,
    IconTrash,
    IconUserPlus,
    IconSettings,
    IconChartBar,
    IconAlertCircle,
    IconCheck,
    IconX
} from '@tabler/icons-react'
import { DataTable } from 'mantine-datatable'
import { useState } from 'react'
import { useDisclosure } from '@mantine/hooks'
import { notifications } from '@mantine/notifications'
import {
    useAdminStats,
    useUserManagement,
    useCourseAnalytics,
    useUpdateUserStatus,
    useUpdateUserRoles,
    useSystemAnalytics,
    type UserManagement
} from '../../hooks/api/useAdmin'

const AdminDashboard = () => {
    const [activeTab, setActiveTab] = useState<string | null>('overview')
    const [userPage, setUserPage] = useState(1)
    const [userSearch, setUserSearch] = useState('')
    const [selectedUser, setSelectedUser] = useState<UserManagement | null>(null)
    const [editModalOpened, { open: openEditModal, close: closeEditModal }] = useDisclosure(false)

    const { data: stats, isLoading: statsLoading, error: statsError } = useAdminStats()
    const { data: userData, isLoading: usersLoading } = useUserManagement(userPage, 10, userSearch)
    const { data: courseAnalytics, isLoading: courseLoading } = useCourseAnalytics()
    const { data: systemAnalytics, isLoading: systemLoading } = useSystemAnalytics()

    const updateUserStatus = useUpdateUserStatus()
    const updateUserRoles = useUpdateUserRoles()

    const handleEditUser = (user: UserManagement) => {
        setSelectedUser(user)
        openEditModal()
    }

    const handleUpdateUserStatus = async (userId: string, isActive: boolean) => {
        try {
            await updateUserStatus.mutateAsync({ userId, isActive })
            notifications.show({
                title: 'Success',
                message: `User ${isActive ? 'activated' : 'deactivated'} successfully`,
                color: 'green'
            })
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to update user status',
                color: 'red'
            })
        }
    }

    const handleUpdateUserRoles = async (roles: string[]) => {
        if (!selectedUser) return

        try {
            await updateUserRoles.mutateAsync({ userId: selectedUser.id, roles })
            notifications.show({
                title: 'Success',
                message: 'User roles updated successfully',
                color: 'green'
            })
            closeEditModal()
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to update user roles',
                color: 'red'
            })
        }
    }

    if (statsLoading || usersLoading || courseLoading || systemLoading) {
        return (
            <Container size="xl" py="xl">
                <Center>
                    <Loader size="lg" />
                </Center>
            </Container>
        )
    }

    if (statsError) {
        return (
            <Container size="xl" py="xl">
                <Alert
                    icon={<IconAlertCircle size={16} />}
                    title="Error loading admin dashboard"
                    color="red"
                >
                    Failed to load admin dashboard data. Please try again later.
                </Alert>
            </Container>
        )
    }

    return (
        <Container size="xl" py="xl">
            <Stack gap="xl">
                {/* Header */}
                <Group justify="space-between" align="center">
                    <div>
                        <Title order={1}>Admin Dashboard</Title>
                        <Text c="dimmed" size="sm">
                            Manage users, courses, and monitor system performance.
                        </Text>
                    </div>
                    <Button leftSection={<IconUserPlus size={16} />}>
                        Add User
                    </Button>
                </Group>

                {/* Stats Cards */}
                <SimpleGrid cols={{ base: 1, sm: 2, lg: 4 }} spacing="lg">
                    <Card shadow="sm" padding="lg" radius="md" withBorder>
                        <Group justify="space-between">
                            <div>
                                <Text c="dimmed" size="sm" tt="uppercase" fw={700}>
                                    Total Users
                                </Text>
                                <Text fw={700} size="xl">
                                    {stats?.totalUsers || 0}
                                </Text>
                                <Text size="xs" c="green">
                                    +{stats?.newRegistrations || 0} this week
                                </Text>
                            </div>
                            <ThemeIcon color="blue" size={40} radius="md">
                                <IconUsers size={24} />
                            </ThemeIcon>
                        </Group>
                    </Card>

                    <Card shadow="sm" padding="lg" radius="md" withBorder>
                        <Group justify="space-between">
                            <div>
                                <Text c="dimmed" size="sm" tt="uppercase" fw={700}>
                                    Active Users
                                </Text>
                                <Text fw={700} size="xl">
                                    {stats?.activeUsers || 0}
                                </Text>
                                <Text size="xs" c="blue">
                                    {stats ? Math.round((stats.activeUsers / stats.totalUsers) * 100) : 0}% of total
                                </Text>
                            </div>
                            <ThemeIcon color="green" size={40} radius="md">
                                <IconTrendingUp size={24} />
                            </ThemeIcon>
                        </Group>
                    </Card>

                    <Card shadow="sm" padding="lg" radius="md" withBorder>
                        <Group justify="space-between">
                            <div>
                                <Text c="dimmed" size="sm" tt="uppercase" fw={700}>
                                    Total Courses
                                </Text>
                                <Text fw={700} size="xl">
                                    {stats?.totalCourses || 0}
                                </Text>
                                <Text size="xs" c="blue">
                                    {stats?.publishedCourses || 0} published
                                </Text>
                            </div>
                            <ThemeIcon color="orange" size={40} radius="md">
                                <IconBook size={24} />
                            </ThemeIcon>
                        </Group>
                    </Card>

                    <Card shadow="sm" padding="lg" radius="md" withBorder>
                        <Group justify="space-between">
                            <div>
                                <Text c="dimmed" size="sm" tt="uppercase" fw={700}>
                                    System Health
                                </Text>
                                <Text fw={700} size="xl">
                                    {stats?.serverUptime || 0}%
                                </Text>
                                <Badge
                                    color={stats?.systemHealth === 'healthy' ? 'green' : 'red'}
                                    variant="light"
                                    size="sm"
                                >
                                    {stats?.systemHealth || 'unknown'}
                                </Badge>
                            </div>
                            <ThemeIcon color="red" size={40} radius="md">
                                <IconServer size={24} />
                            </ThemeIcon>
                        </Group>
                    </Card>
                </SimpleGrid>

                {/* Tabs */}
                <Tabs value={activeTab} onChange={setActiveTab}>
                    <Tabs.List>
                        <Tabs.Tab value="overview" leftSection={<IconChartBar size={16} />}>
                            Overview
                        </Tabs.Tab>
                        <Tabs.Tab value="users" leftSection={<IconUsers size={16} />}>
                            User Management
                        </Tabs.Tab>
                        <Tabs.Tab value="courses" leftSection={<IconBook size={16} />}>
                            Course Analytics
                        </Tabs.Tab>
                        <Tabs.Tab value="system" leftSection={<IconServer size={16} />}>
                            System
                        </Tabs.Tab>
                    </Tabs.List>

                    <Tabs.Panel value="overview" pt="xl">
                        <Grid>
                            <Grid.Col span={{ base: 12, md: 8 }}>
                                <Card shadow="sm" padding="lg" radius="md" withBorder h="100%">
                                    <Title order={3} mb="md">Top Courses by Enrollment</Title>
                                    <Stack gap="md">
                                        {systemAnalytics?.topCourses.map((course, index) => (
                                            <Paper key={course.name} p="md" withBorder>
                                                <Group justify="space-between" mb="xs">
                                                    <Text fw={500}>#{index + 1} {course.name}</Text>
                                                    <Text fw={700}>{course.enrollments} students</Text>
                                                </Group>
                                                <Progress
                                                    value={(course.enrollments / (systemAnalytics?.topCourses[0]?.enrollments || 1)) * 100}
                                                    size="sm"
                                                    radius="xl"
                                                />
                                            </Paper>
                                        ))}
                                    </Stack>
                                </Card>
                            </Grid.Col>

                            <Grid.Col span={{ base: 12, md: 4 }}>
                                <Card shadow="sm" padding="lg" radius="md" withBorder h="100%">
                                    <Title order={3} mb="md">Quick Actions</Title>
                                    <Stack gap="md">
                                        <Button variant="light" fullWidth leftSection={<IconUserPlus size={16} />}>
                                            Add New User
                                        </Button>
                                        <Button variant="light" fullWidth leftSection={<IconBook size={16} />}>
                                            Create Course
                                        </Button>
                                        <Button variant="light" fullWidth leftSection={<IconSettings size={16} />}>
                                            System Settings
                                        </Button>
                                        <Button variant="light" fullWidth leftSection={<IconChartBar size={16} />}>
                                            Generate Report
                                        </Button>
                                    </Stack>
                                </Card>
                            </Grid.Col>
                        </Grid>
                    </Tabs.Panel>

                    <Tabs.Panel value="users" pt="xl">
                        <Card shadow="sm" padding="lg" radius="md" withBorder>
                            <Group justify="space-between" mb="md">
                                <Title order={3}>User Management</Title>
                                <Group>
                                    <TextInput
                                        placeholder="Search users..."
                                        leftSection={<IconSearch size={16} />}
                                        value={userSearch}
                                        onChange={(event) => setUserSearch(event.currentTarget.value)}
                                    />
                                    <Button leftSection={<IconUserPlus size={16} />}>
                                        Add User
                                    </Button>
                                </Group>
                            </Group>

                            <DataTable
                                withTableBorder
                                borderRadius="sm"
                                withColumnBorders
                                striped
                                highlightOnHover
                                records={userData?.users || []}
                                columns={[
                                    {
                                        accessor: 'email',
                                        title: 'Email',
                                        render: (user) => (
                                            <div>
                                                <Text fw={500}>{user.email}</Text>
                                                <Text size="sm" c="dimmed">
                                                    {user.firstName} {user.lastName}
                                                </Text>
                                            </div>
                                        )
                                    },
                                    {
                                        accessor: 'roles',
                                        title: 'Roles',
                                        render: (user) => (
                                            <Group gap="xs">
                                                {user.roles.map((role) => (
                                                    <Badge key={role} size="sm" variant="light">
                                                        {role}
                                                    </Badge>
                                                ))}
                                            </Group>
                                        )
                                    },
                                    {
                                        accessor: 'isActive',
                                        title: 'Status',
                                        render: (user) => (
                                            <Switch
                                                checked={user.isActive}
                                                onChange={(event) =>
                                                    handleUpdateUserStatus(user.id, event.currentTarget.checked)
                                                }
                                                color="green"
                                                size="sm"
                                                thumbIcon={
                                                    user.isActive ? (
                                                        <IconCheck size={12} color="green" stroke={3} />
                                                    ) : (
                                                        <IconX size={12} color="red" stroke={3} />
                                                    )
                                                }
                                            />
                                        )
                                    },
                                    {
                                        accessor: 'coursesEnrolled',
                                        title: 'Courses',
                                        textAlign: 'center'
                                    },
                                    {
                                        accessor: 'lessonsCompleted',
                                        title: 'Lessons',
                                        textAlign: 'center'
                                    },
                                    {
                                        accessor: 'lastLogin',
                                        title: 'Last Login',
                                        render: (user) => new Date(user.lastLogin).toLocaleDateString()
                                    },
                                    {
                                        accessor: 'actions',
                                        title: 'Actions',
                                        textAlign: 'center',
                                        render: (user) => (
                                            <Group gap="xs" justify="center">
                                                <ActionIcon
                                                    variant="subtle"
                                                    color="blue"
                                                    onClick={() => handleEditUser(user)}
                                                >
                                                    <IconEdit size={16} />
                                                </ActionIcon>
                                                <ActionIcon variant="subtle" color="red">
                                                    <IconTrash size={16} />
                                                </ActionIcon>
                                            </Group>
                                        )
                                    }
                                ]}
                                totalRecords={userData?.totalCount || 0}
                                recordsPerPage={10}
                                page={userPage}
                                onPageChange={setUserPage}
                                paginationActiveBackgroundColor="blue"
                            />
                        </Card>
                    </Tabs.Panel>

                    <Tabs.Panel value="courses" pt="xl">
                        <Card shadow="sm" padding="lg" radius="md" withBorder>
                            <Title order={3} mb="md">Course Analytics</Title>

                            <DataTable
                                withTableBorder
                                borderRadius="sm"
                                withColumnBorders
                                striped
                                highlightOnHover
                                records={courseAnalytics || []}
                                columns={[
                                    { accessor: 'title', title: 'Course Title' },
                                    { accessor: 'language', title: 'Language' },
                                    { accessor: 'enrollments', title: 'Enrollments', textAlign: 'center' },
                                    { accessor: 'completions', title: 'Completions', textAlign: 'center' },
                                    {
                                        accessor: 'completionRate',
                                        title: 'Completion Rate',
                                        render: (course) => (
                                            <div>
                                                <Text size="sm">
                                                    {Math.round((course.completions / course.enrollments) * 100)}%
                                                </Text>
                                                <Progress
                                                    value={(course.completions / course.enrollments) * 100}
                                                    size="xs"
                                                    mt={4}
                                                />
                                            </div>
                                        )
                                    },
                                    {
                                        accessor: 'averageRating',
                                        title: 'Rating',
                                        render: (course) => (
                                            <Badge color="yellow" variant="light">
                                                ⭐ {course.averageRating}
                                            </Badge>
                                        )
                                    },
                                    {
                                        accessor: 'status',
                                        title: 'Status',
                                        render: (course) => (
                                            <Badge
                                                color={course.status === 'published' ? 'green' : 'gray'}
                                                variant="light"
                                            >
                                                {course.status}
                                            </Badge>
                                        )
                                    }
                                ]}
                            />
                        </Card>
                    </Tabs.Panel>

                    <Tabs.Panel value="system" pt="xl">
                        <Grid>
                            <Grid.Col span={12}>
                                <Card shadow="sm" padding="lg" radius="md" withBorder>
                                    <Title order={3} mb="md">System Status</Title>
                                    <SimpleGrid cols={{ base: 1, sm: 3 }} spacing="lg">
                                        <div>
                                            <Text size="sm" c="dimmed" mb="xs">Server Uptime</Text>
                                            <Text fw={700} size="lg">{stats?.serverUptime}%</Text>
                                            <Progress value={stats?.serverUptime || 0} size="sm" mt="xs" />
                                        </div>
                                        <div>
                                            <Text size="sm" c="dimmed" mb="xs">System Health</Text>
                                            <Badge
                                                color={stats?.systemHealth === 'healthy' ? 'green' : 'red'}
                                                variant="filled"
                                                size="lg"
                                            >
                                                {stats?.systemHealth}
                                            </Badge>
                                        </div>
                                        <div>
                                            <Text size="sm" c="dimmed" mb="xs">Total Lessons</Text>
                                            <Text fw={700} size="lg">{stats?.totalLessons}</Text>
                                        </div>
                                    </SimpleGrid>
                                </Card>
                            </Grid.Col>
                        </Grid>
                    </Tabs.Panel>
                </Tabs>

                {/* Edit User Modal */}
                <Modal
                    opened={editModalOpened}
                    onClose={closeEditModal}
                    title="Edit User Roles"
                    centered
                >
                    {selectedUser && (
                        <Stack gap="md">
                            <Text>
                                Editing roles for: <strong>{selectedUser.email}</strong>
                            </Text>
                            <MultiSelect
                                label="User Roles"
                                data={[
                                    { value: 'Student', label: 'Student' },
                                    { value: 'Instructor', label: 'Instructor' },
                                    { value: 'Admin', label: 'Admin' },
                                    { value: 'SuperAdmin', label: 'Super Admin' }
                                ]}
                                defaultValue={selectedUser.roles}
                                onChange={handleUpdateUserRoles}
                            />
                            <Group justify="flex-end">
                                <Button variant="default" onClick={closeEditModal}>
                                    Cancel
                                </Button>
                                <Button onClick={() => handleUpdateUserRoles(selectedUser.roles)}>
                                    Save Changes
                                </Button>
                            </Group>
                        </Stack>
                    )}
                </Modal>
            </Stack>
        </Container>
    )
}

export default AdminDashboard