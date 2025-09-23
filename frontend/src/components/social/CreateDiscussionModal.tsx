import { Modal, TextInput, Textarea, Button, Stack, Group } from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import { IconCheck, IconX } from '@tabler/icons-react';
import { useCreateDiscussion } from '../../hooks/api/useDiscussions';
import type { CreateDiscussionRequest } from '../../types/discussion';

interface CreateDiscussionModalProps {
    opened: boolean;
    onClose: () => void;
    lessonId?: string;
    courseId?: string;
    title?: string;
}

export function CreateDiscussionModal({
    opened,
    onClose,
    lessonId,
    courseId,
    title = 'Create Discussion',
}: CreateDiscussionModalProps) {
    const createDiscussion = useCreateDiscussion();

    const form = useForm({
        initialValues: {
            title: '',
            content: '',
        },
        validate: {
            title: (value) => {
                if (!value.trim()) return 'Title is required';
                if (value.length < 5) return 'Title must be at least 5 characters';
                if (value.length > 200) return 'Title must be less than 200 characters';
                return null;
            },
            content: (value) => {
                if (!value.trim()) return 'Content is required';
                if (value.length < 10) return 'Content must be at least 10 characters';
                if (value.length > 5000) return 'Content must be less than 5000 characters';
                return null;
            },
        },
    });

    const handleSubmit = async (values: typeof form.values) => {
        try {
            const request: CreateDiscussionRequest = {
                title: values.title.trim(),
                content: values.content.trim(),
                lessonId,
                courseId,
            };

            await createDiscussion.mutateAsync(request);

            notifications.show({
                title: 'Success',
                message: 'Discussion created successfully!',
                color: 'green',
                icon: <IconCheck size={16} />,
            });

            form.reset();
            onClose();
        } catch (error) {
            console.error('Failed to create discussion:', error);
            notifications.show({
                title: 'Error',
                message: 'Failed to create discussion. Please try again.',
                color: 'red',
                icon: <IconX size={16} />,
            });
        }
    };

    const handleClose = () => {
        form.reset();
        onClose();
    };

    return (
        <Modal
            opened={opened}
            onClose={handleClose}
            title={title}
            size="lg"
            centered
        >
            <form onSubmit={form.onSubmit(handleSubmit)}>
                <Stack gap="md">
                    <TextInput
                        label="Title"
                        placeholder="Enter discussion title..."
                        required
                        {...form.getInputProps('title')}
                    />

                    <Textarea
                        label="Content"
                        placeholder="What would you like to discuss?"
                        required
                        minRows={6}
                        maxRows={12}
                        autosize
                        {...form.getInputProps('content')}
                    />

                    <Group justify="flex-end" gap="sm">
                        <Button
                            variant="subtle"
                            onClick={handleClose}
                            disabled={createDiscussion.isPending}
                        >
                            Cancel
                        </Button>
                        <Button
                            type="submit"
                            loading={createDiscussion.isPending}
                        >
                            Create Discussion
                        </Button>
                    </Group>
                </Stack>
            </form>
        </Modal>
    );
}