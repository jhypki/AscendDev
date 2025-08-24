-- Create database users and set permissions
-- Note: The postgres superuser already exists, but we'll ensure the password is set correctly
ALTER USER postgres PASSWORD 'postgres';

-- Create application user for better security (optional)
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'elearning_user') THEN
        CREATE USER elearning_user WITH PASSWORD 'elearning_password';
    END IF;
END
$$;

-- Grant necessary permissions to the application user
GRANT CONNECT ON DATABASE ascenddev TO elearning_user;
GRANT USAGE ON SCHEMA public TO elearning_user;
GRANT CREATE ON SCHEMA public TO elearning_user;

-- Create tables and grant permissions
CREATE TABLE users (
    id UUID PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255),
    username VARCHAR(255) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    is_email_verified BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE,
    last_login TIMESTAMP WITH TIME ZONE,
    profile_picture_url TEXT,
    bio TEXT,
    external_id VARCHAR(255),
    provider VARCHAR(50),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    is_locked BOOLEAN NOT NULL DEFAULT FALSE,
    locked_until TIMESTAMP WITH TIME ZONE,
    failed_login_attempts INTEGER NOT NULL DEFAULT 0,
    last_failed_login TIMESTAMP WITH TIME ZONE,
    email_verification_token VARCHAR(500),
    email_verification_token_expires TIMESTAMP WITH TIME ZONE,
    password_reset_token VARCHAR(500),
    password_reset_token_expires TIMESTAMP WITH TIME ZONE,
    time_zone VARCHAR(50),
    language VARCHAR(10) DEFAULT 'en',
    email_notifications BOOLEAN NOT NULL DEFAULT TRUE,
    push_notifications BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE courses (
    id TEXT PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL UNIQUE,
    description TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE,
    language VARCHAR(255) NOT NULL,
    featured_image TEXT,
    tags JSONB NOT NULL DEFAULT '[]'::jsonb,
    lesson_summaries JSONB NOT NULL DEFAULT '[]'::jsonb,
    status VARCHAR(50) NOT NULL DEFAULT 'draft',
    current_version INTEGER NOT NULL DEFAULT 1,
    created_by UUID REFERENCES users(id),
    last_modified_by UUID REFERENCES users(id),
    has_draft_version BOOLEAN NOT NULL DEFAULT FALSE
);

-- Course Versions Table for versioning system
CREATE TABLE course_versions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    course_id TEXT NOT NULL REFERENCES courses(id) ON DELETE CASCADE,
    version_number INTEGER NOT NULL,
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL,
    description TEXT,
    language VARCHAR(255) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    featured_image TEXT,
    tags JSONB NOT NULL DEFAULT '[]'::jsonb,
    lesson_summaries JSONB NOT NULL DEFAULT '[]'::jsonb,
    status VARCHAR(50) NOT NULL DEFAULT 'draft',
    created_by UUID NOT NULL REFERENCES users(id),
    change_log TEXT,
    is_active BOOLEAN NOT NULL DEFAULT FALSE,
    UNIQUE(course_id, version_number)
);

CREATE TABLE lessons (
    id TEXT PRIMARY KEY,
    course_id TEXT NOT NULL REFERENCES courses(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL UNIQUE,
    content TEXT,
    language VARCHAR(255) NOT NULL,
    template TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE,
    "order" INT NOT NULL,
    test_config JSONB NOT NULL DEFAULT '{}'::jsonb,
    additional_resources JSONB NOT NULL DEFAULT '[]'::jsonb,
    tags JSONB NOT NULL DEFAULT '[]'::jsonb,
    status VARCHAR(50) NOT NULL DEFAULT 'draft'
);

CREATE TABLE user_progress (
    id SERIAL PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    lesson_id TEXT NOT NULL REFERENCES lessons(id) ON DELETE CASCADE,
    completed_at TIMESTAMP WITH TIME ZONE NOT NULL,
    code_solution TEXT,
    UNIQUE(user_id, lesson_id)
);

-- Indexes for faster lookups
CREATE INDEX idx_user_progress_user_id ON user_progress(user_id);
CREATE INDEX idx_user_progress_lesson_id ON user_progress(lesson_id);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_username ON users(username);

-- Roles and User Roles Tables
CREATE TABLE roles (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE TABLE user_roles (
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_id UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    assigned_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    assigned_by UUID REFERENCES users(id),
    PRIMARY KEY (user_id, role_id)
);

-- Refresh Tokens Table
CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token VARCHAR(500) NOT NULL UNIQUE,
    expires TIMESTAMP WITH TIME ZONE NOT NULL,
    created TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_by_ip VARCHAR(45) NOT NULL,
    revoked TIMESTAMP WITH TIME ZONE,
    revoked_by_ip VARCHAR(45)
);

-- Social Features Tables
CREATE TABLE discussions (
    id UUID PRIMARY KEY,
    lesson_id TEXT NOT NULL REFERENCES lessons(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(500) NOT NULL,
    content TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    is_pinned BOOLEAN DEFAULT FALSE,
    is_locked BOOLEAN DEFAULT FALSE,
    view_count INTEGER DEFAULT 0
);

CREATE TABLE discussion_replies (
    id UUID PRIMARY KEY,
    discussion_id UUID NOT NULL REFERENCES discussions(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    content TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    parent_reply_id UUID REFERENCES discussion_replies(id) ON DELETE CASCADE,
    is_solution BOOLEAN DEFAULT FALSE
);

CREATE TABLE code_reviews (
    id UUID PRIMARY KEY,
    lesson_id TEXT NOT NULL REFERENCES lessons(id) ON DELETE CASCADE,
    reviewer_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    reviewee_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    code_solution TEXT NOT NULL,
    status VARCHAR(50) DEFAULT 'pending', -- pending, approved, changes_requested, completed
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    completed_at TIMESTAMP WITH TIME ZONE
);

CREATE TABLE code_review_comments (
    id UUID PRIMARY KEY,
    code_review_id UUID NOT NULL REFERENCES code_reviews(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    line_number INTEGER,
    content TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    is_resolved BOOLEAN DEFAULT FALSE
);

-- Achievement System Tables
CREATE TABLE achievements (
    id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE,
    description TEXT NOT NULL,
    icon_url VARCHAR(500),
    criteria JSONB NOT NULL, -- Conditions for earning the achievement
    points INTEGER DEFAULT 0,
    category VARCHAR(100) DEFAULT 'general', -- general, progress, social, skill
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE TABLE user_achievements (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    achievement_id UUID NOT NULL REFERENCES achievements(id) ON DELETE CASCADE,
    earned_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    progress_data JSONB, -- Additional data about how achievement was earned
    UNIQUE(user_id, achievement_id)
);

-- Notifications Table
CREATE TABLE notifications (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    type VARCHAR(100) NOT NULL, -- achievement, progress, discussion, review, system
    title VARCHAR(500) NOT NULL,
    message TEXT NOT NULL,
    is_read BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    read_at TIMESTAMP WITH TIME ZONE,
    metadata JSONB, -- Additional data specific to notification type
    action_url VARCHAR(500) -- URL for notification action
);

-- Admin Content Management Tables
CREATE TABLE course_content (
    id UUID PRIMARY KEY,
    course_id TEXT NOT NULL REFERENCES courses(id) ON DELETE CASCADE,
    title VARCHAR(500) NOT NULL,
    description TEXT,
    language VARCHAR(50) NOT NULL,
    difficulty_level VARCHAR(50) DEFAULT 'beginner', -- beginner, intermediate, advanced
    estimated_duration_minutes INTEGER,
    prerequisites TEXT[], -- Array of course IDs
    learning_objectives TEXT[],
    created_by UUID NOT NULL REFERENCES users(id),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    is_published BOOLEAN DEFAULT FALSE,
    version INTEGER DEFAULT 1
);

CREATE TABLE lesson_content (
    id UUID PRIMARY KEY,
    course_id TEXT NOT NULL REFERENCES courses(id) ON DELETE CASCADE,
    lesson_id TEXT REFERENCES lessons(id) ON DELETE CASCADE,
    title VARCHAR(500) NOT NULL,
    content TEXT NOT NULL,
    code_template TEXT,
    solution_code TEXT,
    hints TEXT[],
    difficulty_level VARCHAR(50) DEFAULT 'beginner',
    estimated_duration_minutes INTEGER,
    order_index INTEGER NOT NULL,
    created_by UUID NOT NULL REFERENCES users(id),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    is_published BOOLEAN DEFAULT FALSE,
    version INTEGER DEFAULT 1
);

-- User Statistics and Gamification
CREATE TABLE user_statistics (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    total_points INTEGER DEFAULT 0,
    lessons_completed INTEGER DEFAULT 0,
    courses_completed INTEGER DEFAULT 0,
    current_streak INTEGER DEFAULT 0,
    longest_streak INTEGER DEFAULT 0,
    last_activity_date DATE,
    total_code_reviews INTEGER DEFAULT 0,
    total_discussions INTEGER DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    UNIQUE(user_id)
);

-- Study Groups (for future Phase 3)
CREATE TABLE study_groups (
    id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    created_by UUID NOT NULL REFERENCES users(id),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN DEFAULT TRUE,
    max_members INTEGER DEFAULT 20,
    is_public BOOLEAN DEFAULT TRUE
);

CREATE TABLE study_group_members (
    id UUID PRIMARY KEY,
    group_id UUID NOT NULL REFERENCES study_groups(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role VARCHAR(50) DEFAULT 'member', -- owner, moderator, member
    joined_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    UNIQUE(group_id, user_id)
);

-- Additional Indexes for Performance
CREATE INDEX idx_discussions_lesson_id ON discussions(lesson_id);
CREATE INDEX idx_discussions_user_id ON discussions(user_id);
CREATE INDEX idx_discussions_created_at ON discussions(created_at DESC);

CREATE INDEX idx_discussion_replies_discussion_id ON discussion_replies(discussion_id);
CREATE INDEX idx_discussion_replies_user_id ON discussion_replies(user_id);
CREATE INDEX idx_discussion_replies_parent_id ON discussion_replies(parent_reply_id);

CREATE INDEX idx_code_reviews_lesson_id ON code_reviews(lesson_id);
CREATE INDEX idx_code_reviews_reviewer_id ON code_reviews(reviewer_id);
CREATE INDEX idx_code_reviews_reviewee_id ON code_reviews(reviewee_id);
CREATE INDEX idx_code_reviews_status ON code_reviews(status);

CREATE INDEX idx_code_review_comments_review_id ON code_review_comments(code_review_id);
CREATE INDEX idx_code_review_comments_user_id ON code_review_comments(user_id);

CREATE INDEX idx_user_achievements_user_id ON user_achievements(user_id);
CREATE INDEX idx_user_achievements_achievement_id ON user_achievements(achievement_id);
CREATE INDEX idx_user_achievements_earned_at ON user_achievements(earned_at DESC);

CREATE INDEX idx_notifications_user_id ON notifications(user_id);
CREATE INDEX idx_notifications_is_read ON notifications(is_read);
CREATE INDEX idx_notifications_created_at ON notifications(created_at DESC);
CREATE INDEX idx_notifications_type ON notifications(type);

CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_token ON refresh_tokens(token);
CREATE INDEX idx_refresh_tokens_expires ON refresh_tokens(expires);

CREATE INDEX idx_user_roles_user_id ON user_roles(user_id);
CREATE INDEX idx_user_roles_role_id ON user_roles(role_id);

CREATE INDEX idx_course_content_course_id ON course_content(course_id);
CREATE INDEX idx_course_content_created_by ON course_content(created_by);
CREATE INDEX idx_course_content_is_published ON course_content(is_published);

CREATE INDEX idx_lesson_content_course_id ON lesson_content(course_id);
CREATE INDEX idx_lesson_content_lesson_id ON lesson_content(lesson_id);
CREATE INDEX idx_lesson_content_created_by ON lesson_content(created_by);
CREATE INDEX idx_lesson_content_order_index ON lesson_content(order_index);

CREATE INDEX idx_user_statistics_user_id ON user_statistics(user_id);
CREATE INDEX idx_user_statistics_total_points ON user_statistics(total_points DESC);
CREATE INDEX idx_user_statistics_current_streak ON user_statistics(current_streak DESC);

-- Course Versioning Indexes
CREATE INDEX idx_course_versions_course_id ON course_versions(course_id);
CREATE INDEX idx_course_versions_version_number ON course_versions(version_number);
CREATE INDEX idx_course_versions_is_active ON course_versions(is_active);
CREATE INDEX idx_course_versions_created_at ON course_versions(created_at DESC);
CREATE INDEX idx_course_versions_created_by ON course_versions(created_by);

-- Enhanced Course Indexes
CREATE INDEX idx_courses_status ON courses(status);
CREATE INDEX idx_courses_language ON courses(language);
CREATE INDEX idx_courses_created_by ON courses(created_by);
CREATE INDEX idx_courses_last_modified_by ON courses(last_modified_by);
CREATE INDEX idx_courses_current_version ON courses(current_version);
CREATE INDEX idx_courses_has_draft_version ON courses(has_draft_version);

-- Enhanced Lesson Indexes
CREATE INDEX idx_lessons_status ON lessons(status);
CREATE INDEX idx_lessons_course_id_order ON lessons(course_id, "order");

-- Insert Default Roles
INSERT INTO roles (id, name, description) VALUES
    (gen_random_uuid(), 'SuperAdmin', 'Full system access with all permissions'),
    (gen_random_uuid(), 'Admin', 'Administrative access to manage users and content'),
    (gen_random_uuid(), 'Instructor', 'Can create and manage courses and lessons'),
    (gen_random_uuid(), 'User', 'Standard user with learning access');

-- Insert Default Achievements
INSERT INTO achievements (id, name, description, criteria, points, category) VALUES
    (gen_random_uuid(), 'First Steps', 'Complete your first lesson', '{"type": "lesson_completed", "count": 1}', 10, 'progress'),
    (gen_random_uuid(), 'Getting Started', 'Complete 5 lessons', '{"type": "lesson_completed", "count": 5}', 50, 'progress'),
    (gen_random_uuid(), 'Dedicated Learner', 'Complete 25 lessons', '{"type": "lesson_completed", "count": 25}', 250, 'progress'),
    (gen_random_uuid(), 'Course Master', 'Complete your first course', '{"type": "course_completed", "count": 1}', 100, 'progress'),
    (gen_random_uuid(), 'Streak Starter', 'Maintain a 3-day learning streak', '{"type": "streak", "count": 3}', 30, 'progress'),
    (gen_random_uuid(), 'Consistent Learner', 'Maintain a 7-day learning streak', '{"type": "streak", "count": 7}', 70, 'progress'),
    (gen_random_uuid(), 'Community Helper', 'Help 5 other students in discussions', '{"type": "discussion_replies", "count": 5}', 50, 'social'),
    (gen_random_uuid(), 'Code Reviewer', 'Complete 10 code reviews', '{"type": "code_reviews", "count": 10}', 100, 'social');

-- Grant permissions to elearning_user on all tables
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO elearning_user;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO elearning_user;

-- Grant permissions on future tables (for any additional tables created later)
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO elearning_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT USAGE, SELECT ON SEQUENCES TO elearning_user;
