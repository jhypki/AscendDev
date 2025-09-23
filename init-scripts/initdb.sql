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

-- User Settings Table
CREATE TABLE user_settings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    public_submissions BOOLEAN NOT NULL DEFAULT FALSE,
    show_profile BOOLEAN NOT NULL DEFAULT TRUE,
    email_on_code_review BOOLEAN NOT NULL DEFAULT TRUE,
    email_on_discussion_reply BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    UNIQUE(user_id)
);

-- Submissions Table (replaces user_progress)
CREATE TABLE submissions (
    id SERIAL PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    lesson_id TEXT NOT NULL REFERENCES lessons(id) ON DELETE CASCADE,
    code TEXT NOT NULL,
    passed BOOLEAN NOT NULL DEFAULT FALSE,
    submitted_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    test_results TEXT,
    execution_time_ms INTEGER DEFAULT 0,
    error_message TEXT
);

-- User Progress Table for tracking lesson completion
CREATE TABLE user_progress (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    lesson_id TEXT NOT NULL REFERENCES lessons(id) ON DELETE CASCADE,
    completed_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    submission_id INTEGER NOT NULL REFERENCES submissions(id) ON DELETE CASCADE,
    UNIQUE(user_id, lesson_id)
);

-- User Lesson Code Progress Table for saving user's code edits
CREATE TABLE user_lesson_code (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    lesson_id TEXT NOT NULL REFERENCES lessons(id) ON DELETE CASCADE,
    code TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    UNIQUE(user_id, lesson_id)
);

-- Indexes for faster lookups
CREATE INDEX idx_submissions_user_id ON submissions(user_id);
CREATE INDEX idx_submissions_lesson_id ON submissions(lesson_id);
CREATE INDEX idx_submissions_user_lesson ON submissions(user_id, lesson_id);
CREATE INDEX idx_submissions_passed ON submissions(passed);
CREATE INDEX idx_submissions_submitted_at ON submissions(submitted_at DESC);

-- User Progress Indexes
CREATE INDEX idx_user_progress_user_id ON user_progress(user_id);
CREATE INDEX idx_user_progress_lesson_id ON user_progress(lesson_id);
CREATE INDEX idx_user_progress_user_lesson ON user_progress(user_id, lesson_id);
CREATE INDEX idx_user_progress_completed_at ON user_progress(completed_at DESC);

-- User Lesson Code Indexes
CREATE INDEX idx_user_lesson_code_user_id ON user_lesson_code(user_id);
CREATE INDEX idx_user_lesson_code_lesson_id ON user_lesson_code(lesson_id);
CREATE INDEX idx_user_lesson_code_user_lesson ON user_lesson_code(user_id, lesson_id);
CREATE INDEX idx_user_lesson_code_updated_at ON user_lesson_code(updated_at DESC);

CREATE INDEX idx_user_settings_user_id ON user_settings(user_id);
CREATE INDEX idx_user_settings_public_submissions ON user_settings(public_submissions);

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
    lesson_id TEXT REFERENCES lessons(id) ON DELETE CASCADE,
    course_id TEXT REFERENCES courses(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(500) NOT NULL,
    content TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    is_pinned BOOLEAN DEFAULT FALSE,
    is_locked BOOLEAN DEFAULT FALSE,
    view_count INTEGER DEFAULT 0,
    CONSTRAINT check_discussion_context CHECK (
        (lesson_id IS NOT NULL AND course_id IS NULL) OR
        (lesson_id IS NULL AND course_id IS NOT NULL)
    )
);

CREATE TABLE discussion_likes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    discussion_id UUID NOT NULL REFERENCES discussions(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    UNIQUE(discussion_id, user_id)
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
    submission_id INTEGER NOT NULL REFERENCES submissions(id) ON DELETE CASCADE,
    status VARCHAR(50) DEFAULT 'pending', -- pending, approved, changes_requested, completed
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    completed_at TIMESTAMP WITH TIME ZONE,
    -- Prevent duplicate reviews for the same submission by the same reviewer
    UNIQUE(submission_id, reviewer_id),
    -- Prevent users from reviewing their own code
    CONSTRAINT check_reviewer_not_reviewee CHECK (reviewer_id != reviewee_id)
);

CREATE TABLE code_review_comments (
    id UUID PRIMARY KEY,
    code_review_id UUID NOT NULL REFERENCES code_reviews(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    parent_comment_id UUID REFERENCES code_review_comments(id) ON DELETE CASCADE,
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

-- User Activity Tracking Tables for Admin Analytics
CREATE TABLE user_activity_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    activity_type VARCHAR(100) NOT NULL, -- login, logout, lesson_start, lesson_complete, course_start, course_complete, discussion_post, code_review, etc.
    activity_description TEXT,
    metadata JSONB, -- Additional activity-specific data
    ip_address VARCHAR(45),
    user_agent TEXT,
    session_id VARCHAR(255),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- System Performance Metrics for Admin Dashboard
CREATE TABLE system_metrics (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    metric_name VARCHAR(100) NOT NULL, -- api_response_time, active_users, concurrent_sessions, error_rate, etc.
    metric_value DECIMAL(10,4) NOT NULL,
    metric_unit VARCHAR(50), -- ms, count, percentage, etc.
    tags JSONB, -- Additional metric tags/dimensions
    recorded_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Admin Dashboard Statistics Cache
CREATE TABLE dashboard_statistics (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    statistic_key VARCHAR(100) NOT NULL UNIQUE, -- total_users, active_users_today, lessons_completed_today, etc.
    statistic_value JSONB NOT NULL, -- Can store numbers, objects, arrays
    last_updated TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMP WITH TIME ZONE
);

-- User Session Tracking for Admin Analytics
CREATE TABLE user_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    session_token VARCHAR(500) NOT NULL UNIQUE,
    ip_address VARCHAR(45),
    user_agent TEXT,
    started_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    last_activity TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    ended_at TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

-- Bulk Operations Log for Admin Tracking
CREATE TABLE bulk_operations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    operation_type VARCHAR(100) NOT NULL, -- bulk_user_update, bulk_user_delete, bulk_role_assign, etc.
    performed_by UUID NOT NULL REFERENCES users(id),
    target_count INTEGER NOT NULL, -- Number of records affected
    operation_data JSONB, -- Details of the operation
    status VARCHAR(50) NOT NULL DEFAULT 'pending', -- pending, in_progress, completed, failed
    started_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    completed_at TIMESTAMP WITH TIME ZONE,
    error_message TEXT
);

-- Additional Indexes for Performance
CREATE INDEX idx_discussions_lesson_id ON discussions(lesson_id);
CREATE INDEX idx_discussions_course_id ON discussions(course_id);
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
CREATE INDEX idx_code_review_comments_parent_id ON code_review_comments(parent_comment_id);

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

CREATE INDEX idx_discussion_likes_discussion_id ON discussion_likes(discussion_id);
CREATE INDEX idx_discussion_likes_user_id ON discussion_likes(user_id);

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

-- User Activity Logs Indexes
CREATE INDEX idx_user_activity_logs_user_id ON user_activity_logs(user_id);
CREATE INDEX idx_user_activity_logs_activity_type ON user_activity_logs(activity_type);
CREATE INDEX idx_user_activity_logs_created_at ON user_activity_logs(created_at DESC);
CREATE INDEX idx_user_activity_logs_session_id ON user_activity_logs(session_id);

-- System Metrics Indexes
CREATE INDEX idx_system_metrics_metric_name ON system_metrics(metric_name);
CREATE INDEX idx_system_metrics_recorded_at ON system_metrics(recorded_at DESC);
CREATE INDEX idx_system_metrics_metric_name_recorded_at ON system_metrics(metric_name, recorded_at DESC);

-- Dashboard Statistics Indexes
CREATE INDEX idx_dashboard_statistics_statistic_key ON dashboard_statistics(statistic_key);
CREATE INDEX idx_dashboard_statistics_expires_at ON dashboard_statistics(expires_at);

-- User Sessions Indexes
CREATE INDEX idx_user_sessions_user_id ON user_sessions(user_id);
CREATE INDEX idx_user_sessions_session_token ON user_sessions(session_token);
CREATE INDEX idx_user_sessions_is_active ON user_sessions(is_active);
CREATE INDEX idx_user_sessions_last_activity ON user_sessions(last_activity DESC);

-- Bulk Operations Indexes
CREATE INDEX idx_bulk_operations_performed_by ON bulk_operations(performed_by);
CREATE INDEX idx_bulk_operations_operation_type ON bulk_operations(operation_type);
CREATE INDEX idx_bulk_operations_status ON bulk_operations(status);
CREATE INDEX idx_bulk_operations_started_at ON bulk_operations(started_at DESC);

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

-- Sample Users Data (needed for foreign key constraints)
INSERT INTO users (id, email, password_hash, username, first_name, last_name, is_email_verified, created_at, is_active, language) VALUES
    ('11111111-1111-1111-1111-111111111111', 'admin@ascenddev.com', '$2a$11$example.hash.for.admin.user', 'admin', 'Admin', 'User', true, NOW(), true, 'en'),
    ('22222222-2222-2222-2222-222222222222', 'instructor@ascenddev.com', '$2a$11$example.hash.for.instructor.user', 'instructor', 'John', 'Instructor', true, NOW(), true, 'en'),
    ('33333333-3333-3333-3333-333333333333', 'student1@ascenddev.com', '$2a$11$example.hash.for.student1.user', 'student1', 'Alice', 'Student', true, NOW(), true, 'en'),
    ('44444444-4444-4444-4444-444444444444', 'student2@ascenddev.com', '$2a$11$example.hash.for.student2.user', 'student2', 'Bob', 'Learner', true, NOW(), true, 'en'),
    ('55555555-5555-5555-5555-555555555555', 'inactive@ascenddev.com', '$2a$11$example.hash.for.inactive.user', 'inactive', 'Inactive', 'User', false, NOW(), false, 'en')
ON CONFLICT (id) DO NOTHING;

-- Assign roles to users
DO $$
DECLARE
    admin_role_id UUID;
    instructor_role_id UUID;
    user_role_id UUID;
BEGIN
    -- Get role IDs
    SELECT id INTO admin_role_id FROM roles WHERE name = 'Admin';
    SELECT id INTO instructor_role_id FROM roles WHERE name = 'Instructor';
    SELECT id INTO user_role_id FROM roles WHERE name = 'User';
    
    -- Assign roles to users
    INSERT INTO user_roles (user_id, role_id, assigned_at) VALUES
        ('11111111-1111-1111-1111-111111111111', admin_role_id, NOW()),
        ('22222222-2222-2222-2222-222222222222', instructor_role_id, NOW()),
        ('33333333-3333-3333-3333-333333333333', user_role_id, NOW()),
        ('44444444-4444-4444-4444-444444444444', user_role_id, NOW()),
        ('55555555-5555-5555-5555-555555555555', user_role_id, NOW())
    ON CONFLICT (user_id, role_id) DO NOTHING;
END $$;

-- Sample Lessons Data for Testing
-- Insert sample courses for each language supported by docker runners
INSERT INTO courses (id, title, slug, description, created_at, language, status, created_by, last_modified_by, tags) VALUES
    ('python-fundamentals', 'Python Fundamentals', 'python-fundamentals', 'Learn Python programming fundamentals with hands-on exercises', NOW(), 'python', 'published', '22222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222222', '["python", "programming", "fundamentals"]'),
    ('javascript-essentials', 'JavaScript Essentials', 'javascript-essentials', 'Master JavaScript programming with practical examples', NOW(), 'javascript', 'published', '22222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222222', '["javascript", "programming", "web"]'),
    ('typescript-mastery', 'TypeScript Mastery', 'typescript-mastery', 'Advanced TypeScript programming concepts and practices', NOW(), 'typescript', 'published', '22222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222222', '["typescript", "programming", "web"]'),
    ('go-fundamentals', 'Go Fundamentals', 'go-fundamentals', 'Learn Go programming fundamentals with hands-on exercises', NOW(), 'go', 'published', '22222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222222', '["go", "programming", "fundamentals"]')
ON CONFLICT (id) DO UPDATE SET
    title = EXCLUDED.title,
    description = EXCLUDED.description,
    updated_at = NOW();

-- Insert Python lessons
INSERT INTO lessons (id, course_id, title, slug, content, language, template, created_at, updated_at, "order", test_config, additional_resources, tags, status) VALUES
    ('python-arrays', 'python-fundamentals', 'Working with Arrays', 'python-arrays',
     '# Working with Arrays in Python

Learn how to manipulate arrays (lists) in Python.

## What are Lists?

In Python, lists are ordered collections of items that can be of different types. Lists are mutable, which means you can change their content without changing their identity.

## Exercise

Implement functions to work with arrays:
- `reverse_array(arr)`: Return a new array with elements in reverse order
- `find_max(arr)`: Find the maximum element in the array
- `sum_array(arr)`: Calculate the sum of all elements',
     'python',
     'def reverse_array(arr):
    """
    Reverse the given array.
    Args:
        arr (list): The array to reverse
    Returns:
        list: A new array with elements in reverse order
    """
    # Write your code here
    pass

def find_max(arr):
    """
    Find the maximum element in the array.
    Args:
        arr (list): The array to search
    Returns:
        int/float: The maximum element, or None if array is empty
    """
    # Write your code here
    pass

def sum_array(arr):
    """
    Calculate the sum of all elements in the array.
    Args:
        arr (list): The array to sum
    Returns:
        int/float: The sum of all elements
    """
    # Write your code here
    pass',
     NOW(), NOW(), 1,
     '{
       "timeoutMs": 15000,
       "memoryLimitMb": 512,
       "testTemplate": "import pytest\nfrom solution import reverse_array, find_max, sum_array\n\ndef test_reverse_array():\n    assert reverse_array([1, 2, 3]) == [3, 2, 1]\n    assert reverse_array([]) == []\n    assert reverse_array([5]) == [5]\n\ndef test_find_max():\n    assert find_max([1, 5, 3]) == 5\n    assert find_max([10]) == 10\n    assert find_max([]) is None\n\ndef test_sum_array():\n    assert sum_array([1, 2, 3]) == 6\n    assert sum_array([]) == 0\n    assert sum_array([10]) == 10",
       "testCases": [
         {
           "name": "test_reverse_array_1",
           "input": [1, 2, 3],
           "expectedOutput": [3, 2, 1],
           "description": "Test reversing array [1, 2, 3]"
         },
         {
           "name": "test_find_max_1",
           "input": [1, 5, 3],
           "expectedOutput": 5,
           "description": "Test finding max in [1, 5, 3]"
         },
         {
           "name": "test_sum_array_1",
           "input": [1, 2, 3],
           "expectedOutput": 6,
           "description": "Test summing array [1, 2, 3]"
         }
       ],
       "keywordRequirements": [
         {
           "keyword": "def",
           "description": "Must use function definitions",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         }
       ]
     }',
     '[{"url": "https://docs.python.org/3/tutorial/datastructures.html", "title": "Python Lists Documentation"}]',
     '["python", "arrays", "lists"]',
     'published'),
     
    ('python-functions', 'python-fundamentals', 'Python Functions', 'python-functions',
     '# Python Functions

Learn to create and use functions in Python.

## Exercise

Implement mathematical functions:
- `add_numbers(a, b)`: Add two numbers
- `multiply_numbers(a, b)`: Multiply two numbers
- `calculate_factorial(n)`: Calculate factorial of n',
     'python',
     'def add_numbers(a, b):
    """
    Add two numbers.
    Args:
        a (int/float): First number
        b (int/float): Second number
    Returns:
        int/float: Sum of a and b
    """
    # Write your code here
    pass

def multiply_numbers(a, b):
    """
    Multiply two numbers.
    Args:
        a (int/float): First number
        b (int/float): Second number
    Returns:
        int/float: Product of a and b
    """
    # Write your code here
    pass

def calculate_factorial(n):
    """
    Calculate factorial of n.
    Args:
        n (int): Non-negative integer
    Returns:
        int: Factorial of n
    """
    # Write your code here
    pass',
     NOW(), NOW(), 2,
     '{
       "timeoutMs": 10000,
       "memoryLimitMb": 256,
       "testTemplate": "import pytest\nfrom solution import add_numbers, multiply_numbers, calculate_factorial\n\ndef test_add_numbers():\n    assert add_numbers(2, 3) == 5\n    assert add_numbers(0, 0) == 0\n    assert add_numbers(-1, 1) == 0\n\ndef test_multiply_numbers():\n    assert multiply_numbers(3, 4) == 12\n    assert multiply_numbers(0, 5) == 0\n    assert multiply_numbers(-2, 3) == -6\n\ndef test_calculate_factorial():\n    assert calculate_factorial(5) == 120\n    assert calculate_factorial(0) == 1\n    assert calculate_factorial(1) == 1",
       "testCases": [
         {
           "name": "test_add_numbers_1",
           "input": [2, 3],
           "expectedOutput": 5,
           "description": "Test adding 2 + 3"
         },
         {
           "name": "test_multiply_numbers_1",
           "input": [3, 4],
           "expectedOutput": 12,
           "description": "Test multiplying 3 * 4"
         },
         {
           "name": "test_calculate_factorial_1",
           "input": 5,
           "expectedOutput": 120,
           "description": "Test factorial of 5"
         }
       ],
       "keywordRequirements": [
         {
           "keyword": "def",
           "description": "Must use function definitions",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         }
       ]
     }',
     '[{"url": "https://docs.python.org/3/tutorial/controlflow.html#defining-functions", "title": "Python Functions Documentation"}]',
     '["python", "functions", "math"]',
     'published');

-- Insert JavaScript lessons
INSERT INTO lessons (id, course_id, title, slug, content, language, template, created_at, updated_at, "order", test_config, additional_resources, tags, status) VALUES
    ('javascript-arrays', 'javascript-essentials', 'JavaScript Arrays', 'javascript-arrays',
     '# JavaScript Arrays

Learn array manipulation in JavaScript.

## Exercise

Implement array functions:
- `reverseArray(arr)`: Return a new array with elements in reverse order
- `findMax(arr)`: Find the maximum element
- `sumArray(arr)`: Calculate sum of all elements',
     'javascript',
     'function reverseArray(arr) {
    // Reverse the given array
    // Return a new array with elements in reverse order
}

function findMax(arr) {
    // Find the maximum element in the array
    // Return the maximum element, or undefined if array is empty
}

function sumArray(arr) {
    // Calculate the sum of all elements in the array
    // Return the sum of all elements
}',
     NOW(), NOW(), 1,
     '{
       "timeoutMs": 10000,
       "memoryLimitMb": 256,
       "testTemplate": "__USER_CODE__\n\ndescribe(\"Array Functions\", () => {\n    test(\"reverseArray should reverse arrays\", () => {\n        expect(reverseArray([1, 2, 3])).toEqual([3, 2, 1]);\n        expect(reverseArray([])).toEqual([]);\n        expect(reverseArray([5])).toEqual([5]);\n    });\n    \n    test(\"findMax should find maximum element\", () => {\n        expect(findMax([1, 5, 3])).toBe(5);\n        expect(findMax([10])).toBe(10);\n        expect(findMax([])).toBeUndefined();\n    });\n    \n    test(\"sumArray should calculate sum\", () => {\n        expect(sumArray([1, 2, 3])).toBe(6);\n        expect(sumArray([])).toBe(0);\n        expect(sumArray([10])).toBe(10);\n    });\n});",
       "testCases": [
         {
           "name": "reverseArray should reverse arrays",
           "input": [1, 2, 3],
           "expectedOutput": [3, 2, 1],
           "description": "Test reversing array [1, 2, 3]"
         },
         {
           "name": "findMax should find maximum element",
           "input": [1, 5, 3],
           "expectedOutput": 5,
           "description": "Test finding max in [1, 5, 3]"
         },
         {
           "name": "sumArray should calculate sum",
           "input": [1, 2, 3],
           "expectedOutput": 6,
           "description": "Test summing array [1, 2, 3]"
         }
       ],
       "keywordRequirements": [
         {
           "keyword": "function",
           "description": "Must use function declarations",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         }
       ]
     }',
     '[{"url": "https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array", "title": "JavaScript Array Documentation"}]',
     '["javascript", "arrays", "functions"]',
     'published'),
     
    ('javascript-functions', 'javascript-essentials', 'JavaScript Functions', 'javascript-functions',
     '# JavaScript Functions

Master function creation and usage in JavaScript.

## Exercise

Implement mathematical functions:
- `addNumbers(a, b)`: Add two numbers
- `multiplyNumbers(a, b)`: Multiply two numbers
- `calculateFactorial(n)`: Calculate factorial of n',
     'javascript',
     'function addNumbers(a, b) {
    // Add two numbers and return the result
}

function multiplyNumbers(a, b) {
    // Multiply two numbers and return the result
}

function calculateFactorial(n) {
    // Calculate factorial of n
    // Return 1 for n = 0 or n = 1
}',
     NOW(), NOW(), 2,
     '{
       "timeoutMs": 8000,
       "memoryLimitMb": 256,
       "testTemplate": "__USER_CODE__\n\ndescribe(\"Math Functions\", () => {\n    test(\"addNumbers should add two numbers\", () => {\n        expect(addNumbers(2, 3)).toBe(5);\n        expect(addNumbers(0, 0)).toBe(0);\n        expect(addNumbers(-1, 1)).toBe(0);\n    });\n    \n    test(\"multiplyNumbers should multiply two numbers\", () => {\n        expect(multiplyNumbers(3, 4)).toBe(12);\n        expect(multiplyNumbers(0, 5)).toBe(0);\n        expect(multiplyNumbers(-2, 3)).toBe(-6);\n    });\n    \n    test(\"calculateFactorial should calculate factorial\", () => {\n        expect(calculateFactorial(5)).toBe(120);\n        expect(calculateFactorial(0)).toBe(1);\n        expect(calculateFactorial(1)).toBe(1);\n    });\n});",
       "testCases": [
         {
           "name": "addNumbers should add two numbers",
           "input": [2, 3],
           "expectedOutput": 5,
           "description": "Test adding 2 + 3"
         },
         {
           "name": "multiplyNumbers should multiply two numbers",
           "input": [3, 4],
           "expectedOutput": 12,
           "description": "Test multiplying 3 * 4"
         },
         {
           "name": "calculateFactorial should calculate factorial",
           "input": 5,
           "expectedOutput": 120,
           "description": "Test factorial of 5"
         }
       ],
       "keywordRequirements": [
         {
           "keyword": "function",
           "description": "Must use function declarations",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         }
       ]
     }',
     '[{"url": "https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Functions", "title": "JavaScript Functions Guide"}]',
     '["javascript", "functions", "math"]',
     'published');

-- Insert TypeScript lessons
INSERT INTO lessons (id, course_id, title, slug, content, language, template, created_at, updated_at, "order", test_config, additional_resources, tags, status) VALUES
    ('typescript-arrays', 'typescript-mastery', 'TypeScript Arrays', 'typescript-arrays',
     '# TypeScript Arrays

Learn typed array manipulation in TypeScript.

## Exercise

Implement strongly-typed array functions:
- `reverseArray<T>(arr: T[]): T[]`: Return a new array with elements in reverse order
- `findMax(arr: number[]): number | undefined`: Find the maximum element
- `sumArray(arr: number[]): number`: Calculate sum of all elements',
     'typescript',
     'function reverseArray<T>(arr: T[]): T[] {
    // Reverse the given array
    // Return a new array with elements in reverse order
}

function findMax(arr: number[]): number | undefined {
    // Find the maximum element in the array
    // Return the maximum element, or undefined if array is empty
}

function sumArray(arr: number[]): number {
    // Calculate the sum of all elements in the array
    // Return the sum of all elements
}',
     NOW(), NOW(), 1,
     '{
       "timeoutMs": 12000,
       "memoryLimitMb": 512,
       "testTemplate": "__USER_CODE__\n\ndescribe(\"TypeScript Array Functions\", () => {\n    test(\"reverseArray should reverse arrays\", () => {\n        expect(reverseArray([1, 2, 3])).toEqual([3, 2, 1]);\n        expect(reverseArray([])).toEqual([]);\n        expect(reverseArray([\"a\", \"b\", \"c\"])).toEqual([\"c\", \"b\", \"a\"]);\n    });\n    \n    test(\"findMax should find maximum element\", () => {\n        expect(findMax([1, 5, 3])).toBe(5);\n        expect(findMax([10])).toBe(10);\n        expect(findMax([])).toBeUndefined();\n    });\n    \n    test(\"sumArray should calculate sum\", () => {\n        expect(sumArray([1, 2, 3])).toBe(6);\n        expect(sumArray([])).toBe(0);\n        expect(sumArray([10])).toBe(10);\n    });\n});",
       "testCases": [
         {
           "name": "reverseArray should reverse arrays",
           "input": [1, 2, 3],
           "expectedOutput": [3, 2, 1],
           "description": "Test reversing number array [1, 2, 3]"
         },
         {
           "name": "findMax should find maximum element",
           "input": [1, 5, 3],
           "expectedOutput": 5,
           "description": "Test finding max in [1, 5, 3]"
         },
         {
           "name": "sumArray should calculate sum",
           "input": [1, 2, 3],
           "expectedOutput": 6,
           "description": "Test summing array [1, 2, 3]"
         }
       ],
       "keywordRequirements": [
         {
           "keyword": "function",
           "description": "Must use function declarations",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         },
         {
           "keyword": ":",
           "description": "Must use type annotations",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 5
         }
       ]
     }',
     '[{"url": "https://www.typescriptlang.org/docs/handbook/2/everyday-types.html#arrays", "title": "TypeScript Arrays Documentation"}]',
     '["typescript", "arrays", "generics", "types"]',
     'published'),
     
    ('typescript-basic-math', 'typescript-mastery', 'TypeScript Basic Math', 'typescript-basic-math',
     '# TypeScript Basic Math

Learn to create strongly-typed mathematical functions in TypeScript.

## Exercise

Implement typed mathematical functions:
- `square(number: number): number`: Calculate square of a number
- `addNumbers(a: number, b: number): number`: Add two numbers
- `calculateFactorial(n: number): number`: Calculate factorial of n',
     'typescript',
     'function square(number: number): number {
    // Calculate and return the square of the given number
}

function addNumbers(a: number, b: number): number {
    // Add two numbers and return the result
}

function calculateFactorial(n: number): number {
    // Calculate factorial of n
    // Return 1 for n = 0 or n = 1
}',
     NOW(), NOW(), 2,
     '{
       "timeoutMs": 8000,
       "memoryLimitMb": 256,
       "testTemplate": "__USER_CODE__\n\ndescribe(\"TypeScript Math Functions\", () => {\n    test(\"square should calculate square\", () => {\n        expect(square(2)).toBe(4);\n        expect(square(3)).toBe(9);\n        expect(square(4)).toBe(16);\n    });\n    \n    test(\"addNumbers should add two numbers\", () => {\n        expect(addNumbers(2, 3)).toBe(5);\n        expect(addNumbers(0, 0)).toBe(0);\n        expect(addNumbers(-1, 1)).toBe(0);\n    });\n    \n    test(\"calculateFactorial should calculate factorial\", () => {\n        expect(calculateFactorial(5)).toBe(120);\n        expect(calculateFactorial(0)).toBe(1);\n        expect(calculateFactorial(1)).toBe(1);\n    });\n});",
       "testCases": [
         {
           "name": "square should calculate square",
           "input": 2,
           "expectedOutput": 4,
           "description": "Test square of 2"
         },
         {
           "name": "addNumbers should add two numbers",
           "input": [2, 3],
           "expectedOutput": 5,
           "description": "Test adding 2 + 3"
         },
         {
           "name": "calculateFactorial should calculate factorial",
           "input": 5,
           "expectedOutput": 120,
           "description": "Test factorial of 5"
         }
       ],
       "keywordRequirements": [
         {
           "keyword": "function",
           "description": "Must use function declarations",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         },
         {
           "keyword": "number",
           "description": "Must use number type annotations",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 6
         }
       ]
     }',
     '[{"url": "https://www.typescriptlang.org/docs/handbook/2/functions.html", "title": "TypeScript Functions Documentation"}]',
     '["typescript", "functions", "math", "types"]',
     'published');

-- Insert Go lessons
INSERT INTO lessons (id, course_id, title, slug, content, language, template, created_at, updated_at, "order", test_config, additional_resources, tags, status) VALUES
    ('go-slices', 'go-fundamentals', 'Go Slices', 'go-slices',
     '# Working with Slices in Go

Learn how to manipulate slices in Go.

## What are Slices?

In Go, slices are a key data type that provides a more powerful interface to sequences of data than arrays. Slices are built on top of arrays and provide great flexibility and convenience.

## Exercise

Implement functions to work with slices:
- `ReverseSlice(slice []int) []int`: Return a new slice with elements in reverse order
- `FindMax(slice []int) int`: Find the maximum element in the slice
- `SumSlice(slice []int) int`: Calculate the sum of all elements',
     'go',
     'package main

// ReverseSlice returns a new slice with elements in reverse order
func ReverseSlice(slice []int) []int {
    // Write your code here
    return nil
}

// FindMax finds the maximum element in the slice
// Returns 0 if slice is empty
func FindMax(slice []int) int {
    // Write your code here
    return 0
}

// SumSlice calculates the sum of all elements in the slice
func SumSlice(slice []int) int {
    // Write your code here
    return 0
}',
     NOW(), NOW(), 1,
     '{
       "timeoutMs": 15000,
       "memoryLimitMb": 512,
       "testTemplate": "package main\n\nimport (\n\t\"reflect\"\n\t\"testing\"\n)\n\nfunc TestReverseSlice(t *testing.T) {\n\tresult := ReverseSlice([]int{1, 2, 3})\n\texpected := []int{3, 2, 1}\n\tif !reflect.DeepEqual(result, expected) {\n\t\tt.Errorf(\"ReverseSlice([1, 2, 3]) = %v; want %v\", result, expected)\n\t}\n\n\tresult = ReverseSlice([]int{})\n\texpected = []int{}\n\tif !reflect.DeepEqual(result, expected) {\n\t\tt.Errorf(\"ReverseSlice([]) = %v; want %v\", result, expected)\n\t}\n}\n\nfunc TestFindMax(t *testing.T) {\n\tresult := FindMax([]int{1, 5, 3})\n\tif result != 5 {\n\t\tt.Errorf(\"FindMax([1, 5, 3]) = %d; want 5\", result)\n\t}\n\n\tresult = FindMax([]int{10})\n\tif result != 10 {\n\t\tt.Errorf(\"FindMax([10]) = %d; want 10\", result)\n\t}\n}\n\nfunc TestSumSlice(t *testing.T) {\n\tresult := SumSlice([]int{1, 2, 3})\n\tif result != 6 {\n\t\tt.Errorf(\"SumSlice([1, 2, 3]) = %d; want 6\", result)\n\t}\n\n\tresult = SumSlice([]int{})\n\tif result != 0 {\n\t\tt.Errorf(\"SumSlice([]) = %d; want 0\", result)\n\t}\n}",
       "testCases": [
         {
           "name": "TestReverseSlice",
           "input": [1, 2, 3],
           "expectedOutput": [3, 2, 1],
           "description": "Test reversing slice [1, 2, 3]"
         },
         {
           "name": "TestFindMax",
           "input": [1, 5, 3],
           "expectedOutput": 5,
           "description": "Test finding max in [1, 5, 3]"
         },
         {
           "name": "TestSumSlice",
           "input": [1, 2, 3],
           "expectedOutput": 6,
           "description": "Test summing slice [1, 2, 3]"
         }
       ],
       "keywordRequirements": [
         {
           "keyword": "func",
           "description": "Must use function declarations",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         },
         {
           "keyword": "[]int",
           "description": "Must work with integer slices",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         }
       ]
     }',
     '[{"url": "https://go.dev/tour/moretypes/7", "title": "Go Slices Documentation"}]',
     '["go", "slices", "arrays"]',
     'published'),
     
    ('go-functions', 'go-fundamentals', 'Go Functions', 'go-functions',
     '# Go Functions

Learn to create and use functions in Go.

## Exercise

Implement mathematical functions:
- `AddNumbers(a, b int) int`: Add two numbers
- `MultiplyNumbers(a, b int) int`: Multiply two numbers
- `CalculateFactorial(n int) int`: Calculate factorial of n',
     'go',
     'package main

// AddNumbers adds two integers and returns the result
func AddNumbers(a, b int) int {
    // Write your code here
    return 0
}

// MultiplyNumbers multiplies two integers and returns the result
func MultiplyNumbers(a, b int) int {
    // Write your code here
    return 0
}

// CalculateFactorial calculates the factorial of n
// Returns 1 for n = 0 or n = 1
func CalculateFactorial(n int) int {
    // Write your code here
    return 0
}',
     NOW(), NOW(), 2,
     '{
       "timeoutMs": 10000,
       "memoryLimitMb": 256,
       "testTemplate": "package main\n\nimport \"testing\"\n\nfunc TestAddNumbers(t *testing.T) {\n\tresult := AddNumbers(2, 3)\n\tif result != 5 {\n\t\tt.Errorf(\"AddNumbers(2, 3) = %d; want 5\", result)\n\t}\n\n\tresult = AddNumbers(0, 0)\n\tif result != 0 {\n\t\tt.Errorf(\"AddNumbers(0, 0) = %d; want 0\", result)\n\t}\n\n\tresult = AddNumbers(-1, 1)\n\tif result != 0 {\n\t\tt.Errorf(\"AddNumbers(-1, 1) = %d; want 0\", result)\n\t}\n}\n\nfunc TestMultiplyNumbers(t *testing.T) {\n\tresult := MultiplyNumbers(3, 4)\n\tif result != 12 {\n\t\tt.Errorf(\"MultiplyNumbers(3, 4) = %d; want 12\", result)\n\t}\n\n\tresult = MultiplyNumbers(0, 5)\n\tif result != 0 {\n\t\tt.Errorf(\"MultiplyNumbers(0, 5) = %d; want 0\", result)\n\t}\n}\n\nfunc TestCalculateFactorial(t *testing.T) {\n\tresult := CalculateFactorial(5)\n\tif result != 120 {\n\t\tt.Errorf(\"CalculateFactorial(5) = %d; want 120\", result)\n\t}\n\n\tresult = CalculateFactorial(0)\n\tif result != 1 {\n\t\tt.Errorf(\"CalculateFactorial(0) = %d; want 1\", result)\n\t}\n\n\tresult = CalculateFactorial(1)\n\tif result != 1 {\n\t\tt.Errorf(\"CalculateFactorial(1) = %d; want 1\", result)\n\t}\n}",
       "testCases": [
         {
           "name": "TestAddNumbers",
           "input": [2, 3],
           "expectedOutput": 5,
           "description": "Test adding 2 + 3"
         },
         {
           "name": "TestMultiplyNumbers",
           "input": [3, 4],
           "expectedOutput": 12,
           "description": "Test multiplying 3 * 4"
         },
         {
           "name": "TestCalculateFactorial",
           "input": 5,
           "expectedOutput": 120,
           "description": "Test factorial of 5"
         }
       ],
       "keywordRequirements": [
         {
           "keyword": "func",
           "description": "Must use function declarations",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         },
         {
           "keyword": "int",
           "description": "Must use integer types",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 6
         }
       ]
     }',
     '[{"url": "https://go.dev/tour/basics/4", "title": "Go Functions Documentation"}]',
     '["go", "functions", "math"]',
     'published');
