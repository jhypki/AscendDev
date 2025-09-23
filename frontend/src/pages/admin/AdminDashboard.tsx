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
    TextInput,
    Switch,
    Modal,
    MultiSelect,
    Center,
    Loader,
    Alert,
    SimpleGrid,
    ThemeIcon,
    Progress,
    Tabs,
    Paper,
    Select,
    Textarea,
    Checkbox,
    NumberInput,
    Divider
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
    IconX,
    IconDownload
} from '@tabler/icons-react'
import { DataTable } from 'mantine-datatable'
import { useState } from 'react'
import { useDisclosure } from '@mantine/hooks'
import { notifications } from '@mantine/notifications'
import { useForm } from '@mantine/form'
import { useNavigate } from 'react-router-dom'
import {
    useAdminStats,
    useUserManagement,
    useCourseAnalytics,
    useUpdateUserStatus,
    useUpdateUserRoles,
    useSystemAnalytics,
    useCreateUser,
    useGenerateReport,
    useGetReportTypes,
    type UserManagement
} from '../../hooks/api/useAdmin'

const AdminDashboard = () => {
    const navigate = useNavigate()
    const [activeTab, setActiveTab] = useState<string | null>('overview')
    const [userPage, setUserPage] = useState(1)
    const [userSearch, setUserSearch] = useState('')
    const [selectedUser, setSelectedUser] = useState<UserManagement | null>(null)
    const [editModalOpened, { open: openEditModal, close: closeEditModal }] = useDisclosure(false)
    const [addUserModalOpened, { open: openAddUserModal, close: closeAddUserModal }] = useDisclosure(false)
    const [reportModalOpened, { open: openReportModal, close: closeReportModal }] = useDisclosure(false)
    const [settingsModalOpened, { open: openSettingsModal, close: closeSettingsModal }] = useDisclosure(false)

    const { data: stats, isLoading: statsLoading, error: statsError } = useAdminStats()
    const { data: userData, isLoading: usersLoading } = useUserManagement(userPage, 10, userSearch)
    const { data: courseAnalytics, isLoading: courseLoading } = useCourseAnalytics()
    const { data: systemAnalytics, isLoading: systemLoading } = useSystemAnalytics()
    const { data: reportTypes } = useGetReportTypes()

    const updateUserStatus = useUpdateUserStatus()
    const updateUserRoles = useUpdateUserRoles()
    const createUser = useCreateUser()
    const generateReport = useGenerateReport()

    // Add User Form
    const addUserForm = useForm({
        initialValues: {
            email: '',
            username: '',
            firstName: '',
            lastName: '',
            password: '',
            roles: ['Student'],
            isActive: true,
            sendWelcomeEmail: true
        },
        validate: {
            email: (value) => (/^\S+@\S+$/.test(value) ? null : 'Invalid email'),
            username: (value) => (value.length < 3 ? 'Username must be at least 3 characters' : null),
            password: (value) => (value.length < 6 ? 'Password must be at least 6 characters' : null)
        }
    })

    // Report Generation Form
    const reportForm = useForm({
        initialValues: {
            reportType: '',
            startDate: '',
            endDate: '',
            format: 'json'
        }
    })

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

    const handleCreateUser = async (values: typeof addUserForm.values) => {
        try {
            await createUser.mutateAsync(values)
            notifications.show({
                title: 'Success',
                message: 'User created successfully',
                color: 'green'
            })
            addUserForm.reset()
            closeAddUserModal()
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to create user',
                color: 'red'
            })
        }
    }

    const handleGenerateReport = async (values: typeof reportForm.values) => {
        try {
            const report = await generateReport.mutateAsync({
                reportType: values.reportType,
                startDate: values.startDate ? new Date(values.startDate) : undefined,
                endDate: values.endDate ? new Date(values.endDate) : undefined,
                format: values.format
            })
            notifications.show({
                title: 'Success',
                message: 'Report generated successfully',
                color: 'green'
            })
            reportForm.reset()
            closeReportModal()

            // In a real implementation, you might download the report or show it in a new tab
            console.log('Generated report:', report)
        } catch {
            notifications.show({
                title: 'Error',
                message: 'Failed to generate report',
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
                    <Button leftSection={<IconUserPlus size={16} />} onClick={openAddUserModal}>
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
                                        <Button variant="light" fullWidth leftSection={<IconUserPlus size={16} />} onClick={openAddUserModal}>
                                            Add New User
                                        </Button>
                                        <Button variant="light" fullWidth leftSection={<IconBook size={16} />} onClick={() => navigate('/courses/create')}>
                                            Create Course
                                        </Button>
                                        <Button variant="light" fullWidth leftSection={<IconSettings size={16} />} onClick={openSettingsModal}>
                                            System Settings
                                        </Button>
                                        <Button variant="light" fullWidth leftSection={<IconChartBar size={16} />} onClick={openReportModal}>
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
                                    <Button leftSection={<IconUserPlus size={16} />} onClick={openAddUserModal}>
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

                {/* Add User Modal */}
                <Modal
                    opened={addUserModalOpened}
                    onClose={closeAddUserModal}
                    title="Add New User"
                    size="md"
                    centered
                >
                    <form onSubmit={addUserForm.onSubmit(handleCreateUser)}>
                        <Stack gap="md">
                            <Grid>
                                <Grid.Col span={6}>
                                    <TextInput
                                        label="First Name"
                                        placeholder="John"
                                        {...addUserForm.getInputProps('firstName')}
                                    />
                                </Grid.Col>
                                <Grid.Col span={6}>
                                    <TextInput
                                        label="Last Name"
                                        placeholder="Doe"
                                        {...addUserForm.getInputProps('lastName')}
                                    />
                                </Grid.Col>
                            </Grid>

                            <TextInput
                                label="Email"
                                placeholder="john.doe@example.com"
                                required
                                {...addUserForm.getInputProps('email')}
                            />

                            <TextInput
                                label="Username"
                                placeholder="johndoe"
                                required
                                {...addUserForm.getInputProps('username')}
                            />

                            <TextInput
                                label="Password"
                                type="password"
                                placeholder="Enter password"
                                required
                                {...addUserForm.getInputProps('password')}
                            />

                            <MultiSelect
                                label="Roles"
                                data={[
                                    { value: 'Student', label: 'Student' },
                                    { value: 'Instructor', label: 'Instructor' },
                                    { value: 'Admin', label: 'Admin' }
                                ]}
                                {...addUserForm.getInputProps('roles')}
                            />

                            <Group>
                                <Checkbox
                                    label="Active"
                                    {...addUserForm.getInputProps('isActive', { type: 'checkbox' })}
                                />
                                <Checkbox
                                    label="Send Welcome Email"
                                    {...addUserForm.getInputProps('sendWelcomeEmail', { type: 'checkbox' })}
                                />
                            </Group>

                            <Group justify="flex-end">
                                <Button variant="default" onClick={closeAddUserModal}>
                                    Cancel
                                </Button>
                                <Button type="submit" loading={createUser.isPending}>
                                    Create User
                                </Button>
                            </Group>
                        </Stack>
                    </form>
                </Modal>

                {/* Generate Report Modal */}
                <Modal
                    opened={reportModalOpened}
                    onClose={closeReportModal}
                    title="Generate Report"
                    size="md"
                    centered
                >
                    <form onSubmit={reportForm.onSubmit(handleGenerateReport)}>
                        <Stack gap="md">
                            <Select
                                label="Report Type"
                                placeholder="Select report type"
                                data={reportTypes?.map(type => ({
                                    value: type.id,
                                    label: type.name
                                })) || []}
                                required
                                {...reportForm.getInputProps('reportType')}
                            />

                            <Grid>
                                <Grid.Col span={6}>
                                    <TextInput
                                        label="Start Date"
                                        type="date"
                                        {...reportForm.getInputProps('startDate')}
                                    />
                                </Grid.Col>
                                <Grid.Col span={6}>
                                    <TextInput
                                        label="End Date"
                                        type="date"
                                        {...reportForm.getInputProps('endDate')}
                                    />
                                </Grid.Col>
                            </Grid>

                            <Select
                                label="Format"
                                data={[
                                    { value: 'json', label: 'JSON' },
                                    { value: 'csv', label: 'CSV' },
                                    { value: 'pdf', label: 'PDF' }
                                ]}
                                {...reportForm.getInputProps('format')}
                            />

                            <Group justify="flex-end">
                                <Button variant="default" onClick={closeReportModal}>
                                    Cancel
                                </Button>
                                <Button
                                    type="submit"
                                    loading={generateReport.isPending}
                                    leftSection={<IconDownload size={16} />}
                                >
                                    Generate Report
                                </Button>
                            </Group>
                        </Stack>
                    </form>
                </Modal>

                {/* System Settings Modal */}
                <Modal
                    opened={settingsModalOpened}
                    onClose={closeSettingsModal}
                    title="System Settings"
                    size="lg"
                    centered
                >
                    <Stack gap="md">
                        <Alert icon={<IconAlertCircle size={16} />} color="blue" variant="light">
                            <Text size="sm">
                                System settings functionality is currently in development.
                                This is a placeholder interface for future implementation.
                            </Text>
                        </Alert>

                        <Divider />

                        <Grid>
                            <Grid.Col span={6}>
                                <Checkbox label="Maintenance Mode" />
                            </Grid.Col>
                            <Grid.Col span={6}>
                                <Checkbox label="Registration Enabled" defaultChecked />
                            </Grid.Col>
                            <Grid.Col span={6}>
                                <Checkbox label="Email Notifications" defaultChecked />
                            </Grid.Col>
                            <Grid.Col span={6}>
                                <NumberInput label="Session Timeout (minutes)" defaultValue={60} />
                            </Grid.Col>
                        </Grid>

                        <Textarea
                            label="Maintenance Message"
                            placeholder="Enter maintenance message..."
                            rows={3}
                        />

                        <Group justify="flex-end">
                            <Button variant="default" onClick={closeSettingsModal}>
                                Cancel
                            </Button>
                            <Button disabled>
                                Save Settings (Coming Soon)
                            </Button>
                        </Group>
                    </Stack>
                </Modal>
            </Stack>
        </Container>
    )
}

export default AdminDashboard