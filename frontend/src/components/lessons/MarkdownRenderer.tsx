import { memo } from 'react'
import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'
import rehypeHighlight from 'rehype-highlight'
import { Box, Code, Text, Title, List, Divider, Paper, Blockquote } from '@mantine/core'
import { CodeHighlight } from '@mantine/code-highlight'
import 'highlight.js/styles/github.css'

interface MarkdownRendererProps {
    content: string
    className?: string
}

export const MarkdownRenderer = memo(({ content, className }: MarkdownRendererProps) => {
    return (
        <Box className={className}>
            <ReactMarkdown
                remarkPlugins={[remarkGfm]}
                rehypePlugins={[rehypeHighlight]}
                components={{
                    // Headers
                    h1: ({ children }) => (
                        <Title order={1} mb="md" mt="xl">
                            {children}
                        </Title>
                    ),
                    h2: ({ children }) => (
                        <Title order={2} mb="sm" mt="lg">
                            {children}
                        </Title>
                    ),
                    h3: ({ children }) => (
                        <Title order={3} mb="sm" mt="md">
                            {children}
                        </Title>
                    ),
                    h4: ({ children }) => (
                        <Title order={4} mb="xs" mt="md">
                            {children}
                        </Title>
                    ),
                    h5: ({ children }) => (
                        <Title order={5} mb="xs" mt="sm">
                            {children}
                        </Title>
                    ),
                    h6: ({ children }) => (
                        <Title order={6} mb="xs" mt="sm">
                            {children}
                        </Title>
                    ),

                    // Paragraphs
                    p: ({ children }) => (
                        <Text mb="sm" style={{ lineHeight: 1.6 }}>
                            {children}
                        </Text>
                    ),

                    // Code blocks and inline code
                    code: (props) => {
                        const { className, children } = props
                        const match = /language-(\w+)/.exec(className || '')
                        const language = match ? match[1] : ''

                        // Check if it's a code block (has language class) vs inline code
                        if (className && language) {
                            return (
                                <CodeHighlight
                                    code={String(children).replace(/\n$/, '')}
                                    language={language}
                                    mb="md"
                                />
                            )
                        }

                        return (
                            <Code>
                                {children}
                            </Code>
                        )
                    },

                    // Lists
                    ul: ({ children }) => (
                        <List mb="sm" spacing="xs">
                            {children}
                        </List>
                    ),
                    ol: ({ children }) => (
                        <List type="ordered" mb="sm" spacing="xs">
                            {children}
                        </List>
                    ),
                    li: ({ children }) => (
                        <List.Item>
                            {children}
                        </List.Item>
                    ),

                    // Blockquotes
                    blockquote: ({ children }) => (
                        <Blockquote mb="md">
                            {children}
                        </Blockquote>
                    ),

                    // Horizontal rules
                    hr: () => <Divider my="md" />,

                    // Tables
                    table: ({ children }) => (
                        <Paper withBorder mb="md" style={{ overflow: 'auto' }}>
                            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                                {children}
                            </table>
                        </Paper>
                    ),
                    thead: ({ children }) => (
                        <thead style={{ backgroundColor: 'var(--mantine-color-gray-0)' }}>
                            {children}
                        </thead>
                    ),
                    tbody: ({ children }) => <tbody>{children}</tbody>,
                    tr: ({ children }) => <tr>{children}</tr>,
                    th: ({ children }) => (
                        <th style={{
                            padding: '8px 12px',
                            textAlign: 'left',
                            borderBottom: '1px solid var(--mantine-color-gray-3)',
                            fontWeight: 600
                        }}>
                            {children}
                        </th>
                    ),
                    td: ({ children }) => (
                        <td style={{
                            padding: '8px 12px',
                            borderBottom: '1px solid var(--mantine-color-gray-2)'
                        }}>
                            {children}
                        </td>
                    ),

                    // Links
                    a: ({ href, children }) => (
                        <Text
                            component="a"
                            href={href}
                            target="_blank"
                            rel="noopener noreferrer"
                            style={{
                                color: 'var(--mantine-color-blue-6)',
                                textDecoration: 'none'
                            }}
                            onMouseEnter={(e) => {
                                e.currentTarget.style.textDecoration = 'underline'
                            }}
                            onMouseLeave={(e) => {
                                e.currentTarget.style.textDecoration = 'none'
                            }}
                        >
                            {children}
                        </Text>
                    ),

                    // Strong/Bold
                    strong: ({ children }) => (
                        <Text span fw={700}>
                            {children}
                        </Text>
                    ),

                    // Emphasis/Italic
                    em: ({ children }) => (
                        <Text span fs="italic">
                            {children}
                        </Text>
                    ),
                }}
            >
                {content}
            </ReactMarkdown>
        </Box>
    )
})

MarkdownRenderer.displayName = 'MarkdownRenderer'

export default MarkdownRenderer