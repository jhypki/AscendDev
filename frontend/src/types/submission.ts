export interface Submission {
    id: number
    userId: string
    lessonId: string
    code: string
    passed: boolean
    submittedAt: string
    executionTimeMs: number
    errorMessage?: string
    username: string
    firstName?: string
    lastName?: string
    profilePictureUrl?: string
    lessonTitle: string
    lessonSlug: string
}

export interface PublicSubmission {
    id: number
    userId: string
    lessonId: string
    code: string
    submittedAt: string
    executionTimeMs: number
    username: string
    firstName?: string
    profilePictureUrl?: string
    lessonTitle: string
    lessonSlug: string
}

export interface CodeReview {
    id: string
    lessonId: string
    reviewerId: string
    revieweeId: string
    submissionId: number
    submission?: Submission
    status: CodeReviewStatus
    createdAt: string
    updatedAt?: string
    completedAt?: string
    isCompleted: boolean
    commentCount: number
    reviewDuration?: string
    reviewer: {
        id: string
        username: string
        email: string
    }
    reviewee: {
        id: string
        username: string
        email: string
    }
    comments?: CodeReviewComment[]
}

export interface CodeReviewComment {
    id: string
    codeReviewId: string
    userId: string
    lineNumber?: number
    content: string
    createdAt: string
    updatedAt?: string
    isResolved: boolean
    user: {
        id: string
        username: string
        email: string
    }
    isEdited: boolean
    isLineComment: boolean
    isGeneralComment: boolean
    parentCommentId?: string
    isReply: boolean
    replyCount: number
    replies: CodeReviewComment[]
}

export const CodeReviewStatus = {
    Pending: 'pending',
    InReview: 'in_review',
    ChangesRequested: 'changes_requested',
    Approved: 'approved',
    Completed: 'completed'
} as const

export type CodeReviewStatus = typeof CodeReviewStatus[keyof typeof CodeReviewStatus]

export interface CreateCodeReviewRequest {
    lessonId: string
    revieweeId: string
    submissionId: number
}

export interface UpdateCodeReviewRequest {
    status?: CodeReviewStatus
    submissionId?: number
}

export interface CreateCodeReviewCommentRequest {
    lineNumber?: number
    content: string
    parentCommentId?: string
}

export interface UpdateCodeReviewCommentRequest {
    content: string
}