export interface Course {
    id: string
    title: string
    slug: string
    description?: string
    language: string
    tags: string[]
    featuredImage?: string
    status: 'draft' | 'published' | 'archived'
    createdAt: string
    updatedAt?: string
    lessonSummaries: LessonSummary[]
    currentVersion: number
    createdBy?: string
    lastModifiedBy?: string
    hasDraftVersion: boolean
}

export interface LessonSummary {
    id: string
    title: string
    slug: string
    order: number
}

export interface Lesson {
    id: string
    courseId: string
    title: string
    slug: string
    content: string
    template: string
    createdAt: string
    updatedAt: string
    language: string
    order: number
    additionalResources: AdditionalResource[]
    tags: string[]
    mainFunction: string
    testCases: TestCase[]
}

export interface AdditionalResource {
    title: string
    url: string
    type: 'documentation' | 'video' | 'article' | 'example'
}

export interface TestCase {
    id: string
    name: string
    input: unknown
    expectedOutput: unknown
    description?: string
    testCode?: string // Actual unit test code
    isHidden?: boolean // Whether this test is hidden from students
}

export interface CreateCourseRequest {
    title: string
    slug: string
    description?: string
    language: string
    tags: string[]
    featuredImage?: string
    status: string
}

export interface UpdateCourseRequest {
    title?: string
    slug?: string
    description?: string
    language?: string
    tags?: string[]
    featuredImage?: string
    status?: string
}

export interface CourseFilters {
    search?: string
    language?: string
    status?: string
    tags?: string[]
}

export interface CoursesResponse {
    courses: Course[]
    totalCount: number
    page: number
    pageSize: number
    totalPages: number
    hasNextPage: boolean
    hasPreviousPage: boolean
}