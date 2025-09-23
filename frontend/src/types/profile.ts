export interface UserProfile {
    id: string
    username: string
    email?: string
    firstName?: string
    lastName?: string
    fullName: string
    bio?: string
    profilePictureUrl?: string
    isEmailVerified: boolean
    provider?: string
    createdAt: string
    lastLogin?: string
    roles: string[]

    // Statistics (nested object from backend)
    statistics: {
        totalPoints: number
        lessonsCompleted: number
        coursesCompleted: number
        currentStreak: number
        longestStreak: number
        totalCodeReviews: number
        totalDiscussions: number
        lastActivityDate?: string
        joinedDate: string
    }

    // Settings (nested object from backend)
    settings?: {
        publicSubmissions: boolean
        showProfile: boolean
        emailOnCodeReview: boolean
        emailOnDiscussionReply: boolean
    }

    // Achievements
    achievements: UserAchievement[]

    // Recent Activity
    recentActivity: ActivityItem[]
}

export interface UserAchievement {
    id: string
    name: string
    description: string
    iconUrl?: string
    points: number
    category: string
    earnedAt: string
    progressData?: Record<string, unknown>
}

export interface ActivityItem {
    id: string
    type: string
    description: string
    metadata?: Record<string, unknown>
    createdAt: string
    relatedEntityId?: string
    relatedEntityType?: string
}

export interface UpdateUserProfileRequest {
    firstName?: string
    lastName?: string
    bio?: string
    profilePictureUrl?: string
}

export interface UserProfileSettings {
    isProfilePublic: boolean
    showEmail: boolean
    showProgress: boolean
    showAchievements: boolean
}

export interface ActivityFeedRequest {
    page?: number
    pageSize?: number
    activityTypes?: string[]
}

export interface ActivityFeedResponse {
    activities: ActivityItem[]
    totalCount: number
    page: number
    pageSize: number
    totalPages: number
    hasNextPage: boolean
    hasPreviousPage: boolean
}

export interface UserSearchRequest {
    query: string
    page?: number
    pageSize?: number
}

export interface UserSearchResponse {
    profiles: PublicUserProfile[]
    totalCount: number
    hasMore: boolean
    nextOffset?: number
}

export interface PublicUserProfile {
    id: string
    username: string
    firstName?: string
    lastName?: string
    fullName: string
    bio?: string
    profilePictureUrl?: string
    totalPoints: number
    lessonsCompleted: number
    coursesCompleted: number
    currentStreak: number
    joinedDate: string
    lastActivityDate?: string
}