import { Container, Group, Text, Anchor, Divider } from '@mantine/core'

export function Footer() {
    const currentYear = new Date().getFullYear()

    return (
        <>
            <Divider my="xl" />
            <Container size="lg">
                <Group justify="space-between" py="md">
                    <Text size="sm" c="dimmed">
                        © {currentYear} AscendDev. All rights reserved.
                    </Text>

                    <Group gap="md">
                        <Anchor href="/privacy" size="sm" c="dimmed">
                            Privacy Policy
                        </Anchor>
                        <Anchor href="/terms" size="sm" c="dimmed">
                            Terms of Service
                        </Anchor>
                        <Anchor href="/support" size="sm" c="dimmed">
                            Support
                        </Anchor>
                    </Group>
                </Group>
            </Container>
        </>
    )
}