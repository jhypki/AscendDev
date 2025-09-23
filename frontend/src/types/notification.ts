export interface Notification {
    id: string;
    userId: string;
    type: NotificationType;
    title: string;
    message: string;
    isRead: boolean;
    createdAt: string;
    readAt?: string;
    metadata?: Record<string, unknown>;
    actionUrl?: string;
    isRecent: boolean;
    age: string;
}

export type NotificationType =
    | 'Achievement'
    | 'Progress'
    | 'Discussion'
    | 'CodeReview'
    | 'System'
    | 'Welcome'
    | 'Reminder'
    | 'Social';

export interface NotificationResponse {
    notifications: Notification[];
    totalCount: number;
    unreadCount: number;
}