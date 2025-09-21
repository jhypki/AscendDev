import { Container, Title, Text, Grid, Card } from '@mantine/core'

const DashboardPage = () => {
    return (
        <Container size="xl" py="xl">
            <Title order={1} mb="xl">Dashboard</Title>
            <Grid>
                <Grid.Col span={12}>
                    <Card shadow="sm" p="lg" radius="md" withBorder>
                        <Text>Dashboard page placeholder - will be implemented in Phase 5</Text>
                    </Card>
                </Grid.Col>
            </Grid>
        </Container>
    )
}

export default DashboardPage