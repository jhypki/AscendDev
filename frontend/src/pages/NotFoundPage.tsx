import { Container, Title, Text, Button, Stack } from '@mantine/core'
import { useNavigate } from 'react-router-dom'

const NotFoundPage = () => {
    const navigate = useNavigate()

    return (
        <Container size="sm" py="xl">
            <Stack align="center" gap="md">
                <Title order={1} size="4rem" c="dimmed">404</Title>
                <Title order={2}>Page Not Found</Title>
                <Text c="dimmed" ta="center">
                    The page you are looking for does not exist.
                </Text>
                <Button onClick={() => navigate('/dashboard')}>
                    Go to Dashboard
                </Button>
            </Stack>
        </Container>
    )
}

export default NotFoundPage