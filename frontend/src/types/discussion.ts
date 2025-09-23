export interface User {
    id: string;
    username: string;
    email: string;
    profilePictureUrl?: string;
}

export interface Discussion {
    id: string;
    lessonId?: string;
    courseId?: string;
    userId: string;
    title: string;
    content: string;
    createdAt: string;
    updatedAt?: string;
    isPinned: boolean;
    isLocked: boolean;
    viewCount: number;
    replyCount: number;
    likeCount: number;
    isLikedByCurrentUser: boolean;
    lastActivity: string;
    user: User;
    replies?: DiscussionReply[];
}

export interface DiscussionReply {
    id: string;
    discussionId: string;
    userId: string;
    content: string;
    createdAt: string;
    updatedAt?: string;
    parentReplyId?: string;
    isSolution: boolean;
    isEdited: boolean;
    depth: number;
    user: User;
    childReplies?: DiscussionReply[];
}

export interface CreateDiscussionRequest {
    title: string;
    content: string;
    lessonId?: string;
    courseId?: string;
}

export interface UpdateDiscussionRequest {
    title?: string;
    content?: string;
    isPinned?: boolean;
    isLocked?: boolean;
}

export interface CreateDiscussionReplyRequest {
    content: string;
    parentReplyId?: string;
}

export interface UpdateDiscussionReplyRequest {
    content: string;
    isSolution?: boolean;
}