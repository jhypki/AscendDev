import {
    Group,
    TextInput,
    Select,
    MultiSelect,
    Button,
    Paper,
    Stack
} from '@mantine/core'
import { IconSearch, IconX } from '@tabler/icons-react'
import { useState, useEffect } from 'react'
import type { CourseFilters } from '../../types/course'

interface CourseFiltersProps {
    filters: CourseFilters
    onFiltersChange: (filters: CourseFilters) => void
    onClearFilters: () => void
}

const PROGRAMMING_LANGUAGES = [
    { value: 'javascript', label: 'JavaScript' },
    { value: 'typescript', label: 'TypeScript' },
    { value: 'python', label: 'Python' },
    { value: 'java', label: 'Java' },
    { value: 'csharp', label: 'C#' },
    { value: 'go', label: 'Go' },
    { value: 'rust', label: 'Rust' },
    { value: 'php', label: 'PHP' },
    { value: 'ruby', label: 'Ruby' },
    { value: 'swift', label: 'Swift' },
]

const STATUS_OPTIONS = [
    { value: 'published', label: 'Published' },
    { value: 'draft', label: 'Draft' },
    { value: 'archived', label: 'Archived' },
]

const COMMON_TAGS = [
    { value: 'beginner', label: 'Beginner' },
    { value: 'intermediate', label: 'Intermediate' },
    { value: 'advanced', label: 'Advanced' },
    { value: 'web-development', label: 'Web Development' },
    { value: 'backend', label: 'Backend' },
    { value: 'frontend', label: 'Frontend' },
    { value: 'algorithms', label: 'Algorithms' },
    { value: 'data-structures', label: 'Data Structures' },
    { value: 'api', label: 'API' },
    { value: 'database', label: 'Database' },
]

const CourseFiltersComponent = ({ filters, onFiltersChange, onClearFilters }: CourseFiltersProps) => {
    const [localFilters, setLocalFilters] = useState<CourseFilters>(filters)

    useEffect(() => {
        setLocalFilters(filters)
    }, [filters])

    const handleFilterChange = (key: keyof CourseFilters, value: string | string[] | null) => {
        const newFilters = { ...localFilters, [key]: value }
        setLocalFilters(newFilters)
        onFiltersChange(newFilters)
    }

    const hasActiveFilters = Object.values(filters).some(value =>
        value !== undefined && value !== '' && (!Array.isArray(value) || value.length > 0)
    )

    return (
        <Paper p="md" shadow="xs" radius="md">
            <Stack gap="md">
                <Group grow>
                    <TextInput
                        placeholder="Search courses..."
                        leftSection={<IconSearch size={16} />}
                        value={localFilters.search || ''}
                        onChange={(event) => handleFilterChange('search', event.currentTarget.value)}
                    />
                </Group>

                <Group grow>
                    <Select
                        placeholder="Select language"
                        data={PROGRAMMING_LANGUAGES}
                        value={localFilters.language || null}
                        onChange={(value) => handleFilterChange('language', value)}
                        clearable
                        searchable
                    />

                    <Select
                        placeholder="Select status"
                        data={STATUS_OPTIONS}
                        value={localFilters.status || null}
                        onChange={(value) => handleFilterChange('status', value)}
                        clearable
                    />
                </Group>

                <MultiSelect
                    placeholder="Select tags"
                    data={COMMON_TAGS}
                    value={localFilters.tags || []}
                    onChange={(value) => handleFilterChange('tags', value)}
                    searchable
                    clearable
                />

                {hasActiveFilters && (
                    <Group justify="flex-end">
                        <Button
                            variant="subtle"
                            color="gray"
                            leftSection={<IconX size={16} />}
                            onClick={onClearFilters}
                        >
                            Clear Filters
                        </Button>
                    </Group>
                )}
            </Stack>
        </Paper>
    )
}

export default CourseFiltersComponent