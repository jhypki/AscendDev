import { useState } from 'react'
import {
    Container,
    Stack,
    Text,
    Paper,
    Group,
    Badge,
    Divider,
    Alert,
    List,
    ThemeIcon
} from '@mantine/core'
import {
    IconCode,
    IconInfoCircle,
    IconBulb,
    IconCheck
} from '@tabler/icons-react'
import { PlaygroundCodeEditor } from '../../components/playground/PlaygroundCodeEditor'

export const PlaygroundPage = () => {
    const [currentLanguage, setCurrentLanguage] = useState('javascript')

    const handleLanguageChange = (language: string) => {
        setCurrentLanguage(language)
    }

    const getLanguageInfo = (lang: string) => {
        switch (lang) {
            case 'javascript':
                return {
                    name: 'JavaScript',
                    description: 'Dynamic programming language for web development',
                    features: ['Dynamic typing', 'First-class functions', 'Prototype-based OOP', 'Event-driven programming']
                }
            case 'typescript':
                return {
                    name: 'TypeScript',
                    description: 'Typed superset of JavaScript that compiles to plain JavaScript',
                    features: ['Static typing', 'Type inference', 'Interfaces', 'Generics']
                }
            case 'python':
                return {
                    name: 'Python',
                    description: 'High-level programming language with emphasis on code readability',
                    features: ['Simple syntax', 'Dynamic typing', 'Extensive libraries', 'Object-oriented']
                }
            case 'csharp':
                return {
                    name: 'C#',
                    description: 'Modern, object-oriented programming language developed by Microsoft',
                    features: ['Strong typing', 'Memory management', 'LINQ', 'Async/await']
                }
            case 'go':
                return {
                    name: 'Go',
                    description: 'Statically typed, compiled programming language designed at Google',
                    features: ['Fast compilation', 'Garbage collection', 'Concurrency support', 'Simple syntax']
                }
            default:
                return {
                    name: 'JavaScript',
                    description: 'Dynamic programming language for web development',
                    features: ['Dynamic typing', 'First-class functions', 'Prototype-based OOP']
                }
        }
    }

    const languageInfo = getLanguageInfo(currentLanguage)

    return (
        <Container size="xl" py="md">
            <Stack gap="lg">
                {/* Header */}
                <Paper withBorder p="md">
                    <Group justify="space-between" align="flex-start">
                        <Stack gap="xs">
                            <Group gap="xs" align="center">
                                <IconCode size={24} />
                                <Text size="xl" fw={700}>Code Playground</Text>
                                <Badge size="sm" color="blue" variant="light">
                                    {languageInfo.name}
                                </Badge>
                            </Group>
                            <Text size="sm" c="dimmed">
                                Write, run, and experiment with code in multiple programming languages
                            </Text>
                        </Stack>
                    </Group>
                </Paper>

                {/* Main Content */}
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 300px', gap: 'var(--mantine-spacing-lg)', height: 'calc(100vh - 300px)', alignItems: 'start' }}>
                    {/* Code Editor */}
                    <div style={{ height: '100%', minHeight: 0 }}>
                        <PlaygroundCodeEditor onLanguageChange={handleLanguageChange} />
                    </div>

                    {/* Sidebar */}
                    <Stack gap="md" style={{ height: '100%', overflow: 'auto' }}>
                        {/* Language Info */}
                        <Paper withBorder p="md">
                            <Stack gap="sm">
                                <Group gap="xs" align="center">
                                    <IconInfoCircle size={16} />
                                    <Text size="sm" fw={600}>About {languageInfo.name}</Text>
                                </Group>
                                <Text size="xs" c="dimmed">
                                    {languageInfo.description}
                                </Text>
                                <Divider />
                                <div>
                                    <Text size="xs" fw={500} mb="xs">Key Features:</Text>
                                    <List size="xs" spacing="xs">
                                        {languageInfo.features.map((feature, index) => (
                                            <List.Item
                                                key={index}
                                                icon={
                                                    <ThemeIcon color="blue" size={16} radius="xl">
                                                        <IconCheck size={10} />
                                                    </ThemeIcon>
                                                }
                                            >
                                                {feature}
                                            </List.Item>
                                        ))}
                                    </List>
                                </div>
                            </Stack>
                        </Paper>

                        {/* Tips */}
                        <Paper withBorder p="md">
                            <Stack gap="sm">
                                <Group gap="xs" align="center">
                                    <IconBulb size={16} />
                                    <Text size="sm" fw={600}>Tips</Text>
                                </Group>
                                <List size="xs" spacing="xs">
                                    <List.Item>Use console.log() or print() to see output</List.Item>
                                    <List.Item>Click "Run Code" to execute your program</List.Item>
                                    <List.Item>Use "Reset" to restore the default template</List.Item>
                                    <List.Item>Switch languages using the dropdown</List.Item>
                                </List>
                            </Stack>
                        </Paper>

                        {/* Getting Started */}
                        <Alert icon={<IconInfoCircle size={16} />} title="Getting Started" color="blue">
                            <Text size="xs">
                                The playground comes with example code for each language.
                                Modify the code and click "Run Code" to see the results.
                                Perfect for learning, testing ideas, or experimenting with new concepts!
                            </Text>
                        </Alert>

                        {/* Language-specific tips */}
                        {currentLanguage === 'python' && (
                            <Alert icon={<IconBulb size={16} />} title="Python Tips" color="yellow">
                                <Text size="xs">
                                    Python uses indentation for code blocks. Use 4 spaces for each indentation level.
                                </Text>
                            </Alert>
                        )}

                        {currentLanguage === 'csharp' && (
                            <Alert icon={<IconBulb size={16} />} title="C# Tips" color="purple">
                                <Text size="xs">
                                    C# requires a Main method as the entry point. Don't forget semicolons at the end of statements!
                                </Text>
                            </Alert>
                        )}

                        {currentLanguage === 'go' && (
                            <Alert icon={<IconBulb size={16} />} title="Go Tips" color="cyan">
                                <Text size="xs">
                                    Go requires a main package and main function. The language is case-sensitive!
                                </Text>
                            </Alert>
                        )}

                        {(currentLanguage === 'javascript' || currentLanguage === 'typescript') && (
                            <Alert icon={<IconBulb size={16} />} title="JS/TS Tips" color="orange">
                                <Text size="xs">
                                    Use console.log() to output values. {currentLanguage === 'typescript' ? 'TypeScript adds static typing to JavaScript.' : 'JavaScript is dynamically typed.'}
                                </Text>
                            </Alert>
                        )}
                    </Stack>
                </div>
            </Stack>
        </Container>
    )
}

export default PlaygroundPage