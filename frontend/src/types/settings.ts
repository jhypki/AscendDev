export interface UserSettings {
    id: string
    userId: string
    publicSubmissions: boolean
    showProfile: boolean
    emailOnCodeReview: boolean
    emailOnDiscussionReply: boolean
    createdAt: string
    updatedAt?: string
}

export interface UpdateUserSettingsRequest {
    publicSubmissions: boolean
    showProfile: boolean
    emailOnCodeReview: boolean
    emailOnDiscussionReply: boolean
}

export interface UpdatePrivacySettingsRequest {
    isProfilePublic: boolean
    showEmail: boolean
    showProgress: boolean
    showAchievements: boolean
}