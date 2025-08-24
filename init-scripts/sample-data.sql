-- Sample Data Insertion Script for AscendDev Platform
-- This script inserts sample data for testing admin functionality

-- Insert sample users with different roles
INSERT INTO users (id, email, password_hash, username, first_name, last_name, is_email_verified, created_at, is_active, language) VALUES
    ('11111111-1111-1111-1111-111111111111', 'admin@ascenddev.com', '$2a$11$example.hash.for.admin.user', 'admin', 'Admin', 'User', true, NOW(), true, 'en'),
    ('22222222-2222-2222-2222-222222222222', 'instructor@ascenddev.com', '$2a$11$example.hash.for.instructor.user', 'instructor', 'John', 'Instructor', true, NOW(), true, 'en'),
    ('33333333-3333-3333-3333-333333333333', 'student1@ascenddev.com', '$2a$11$example.hash.for.student1.user', 'student1', 'Alice', 'Student', true, NOW(), true, 'en'),
    ('44444444-4444-4444-4444-444444444444', 'student2@ascenddev.com', '$2a$11$example.hash.for.student2.user', 'student2', 'Bob', 'Learner', true, NOW(), true, 'en'),
    ('55555555-5555-5555-5555-555555555555', 'inactive@ascenddev.com', '$2a$11$example.hash.for.inactive.user', 'inactive', 'Inactive', 'User', false, NOW(), false, 'en');

-- Get role IDs for assignment
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
        ('55555555-5555-5555-5555-555555555555', user_role_id, NOW());
END $$;

-- Insert sample courses
INSERT INTO courses (id, title, slug, description, created_at, language, status, created_by, last_modified_by) VALUES
    ('python-basics', 'Python Basics', 'python-basics', 'Learn the fundamentals of Python programming', NOW(), 'python', 'published', '22222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222222'),
    ('javascript-intro', 'JavaScript Introduction', 'javascript-intro', 'Introduction to JavaScript programming', NOW(), 'javascript', 'published', '22222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222222'),
    ('csharp-fundamentals', 'C# Fundamentals', 'csharp-fundamentals', 'Learn C# programming fundamentals', NOW(), 'csharp', 'draft', '22222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222222');

-- Insert sample lessons
INSERT INTO lessons (id, course_id, title, slug, content, language, template, created_at, "order", status) VALUES
    ('python-basics-variables', 'python-basics', 'Variables and Data Types', 'variables-data-types', 'Learn about Python variables and basic data types', 'python', 'print("Hello, World!")', NOW(), 1, 'published'),
    ('python-basics-loops', 'python-basics', 'Loops in Python', 'loops-python', 'Understanding for and while loops', 'python', 'for i in range(10):\n    print(i)', NOW(), 2, 'published'),
    ('javascript-intro-basics', 'javascript-intro', 'JavaScript Basics', 'javascript-basics', 'Introduction to JavaScript syntax', 'javascript', 'console.log("Hello, World!");', NOW(), 1, 'published'),
    ('csharp-fundamentals-hello', 'csharp-fundamentals', 'Hello World in C#', 'hello-world-csharp', 'Your first C# program', 'csharp', 'Console.WriteLine("Hello, World!");', NOW(), 1, 'draft');

-- Insert sample user progress
INSERT INTO user_progress (user_id, lesson_id, completed_at, code_solution) VALUES
    ('33333333-3333-3333-3333-333333333333', 'python-basics-variables', NOW() - INTERVAL '2 days', 'name = "Alice"\nage = 25\nprint(f"Hello, {name}! You are {age} years old.")'),
    ('33333333-3333-3333-3333-333333333333', 'python-basics-loops', NOW() - INTERVAL '1 day', 'for i in range(5):\n    print(f"Number: {i}")'),
    ('44444444-4444-4444-4444-444444444444', 'python-basics-variables', NOW() - INTERVAL '3 days', 'name = "Bob"\nprint(f"Hello, {name}!")'),
    ('44444444-4444-4444-4444-444444444444', 'javascript-intro-basics', NOW() - INTERVAL '1 day', 'console.log("Hello from Bob!");');

-- Insert sample user statistics
INSERT INTO user_statistics (id, user_id, total_points, lessons_completed, courses_completed, current_streak, longest_streak, last_activity_date) VALUES
    (gen_random_uuid(), '33333333-3333-3333-3333-333333333333', 150, 2, 0, 2, 3, CURRENT_DATE),
    (gen_random_uuid(), '44444444-4444-4444-4444-444444444444', 75, 2, 0, 1, 2, CURRENT_DATE - INTERVAL '1 day'),
    (gen_random_uuid(), '22222222-2222-2222-2222-222222222222', 500, 0, 0, 0, 0, CURRENT_DATE);

-- Insert sample user activity logs
INSERT INTO user_activity_logs (user_id, activity_type, activity_description, metadata, ip_address, created_at) VALUES
    ('33333333-3333-3333-3333-333333333333', 'login', 'User logged in', '{"method": "email"}', '192.168.1.100', NOW() - INTERVAL '1 hour'),
    ('33333333-3333-3333-3333-333333333333', 'lesson_complete', 'Completed lesson: Variables and Data Types', '{"lesson_id": "python-basics-variables", "course_id": "python-basics"}', '192.168.1.100', NOW() - INTERVAL '2 days'),
    ('44444444-4444-4444-4444-444444444444', 'login', 'User logged in', '{"method": "oauth", "provider": "github"}', '192.168.1.101', NOW() - INTERVAL '2 hours'),
    ('44444444-4444-4444-4444-444444444444', 'lesson_start', 'Started lesson: JavaScript Basics', '{"lesson_id": "javascript-intro-basics", "course_id": "javascript-intro"}', '192.168.1.101', NOW() - INTERVAL '1 day'),
    ('11111111-1111-1111-1111-111111111111', 'login', 'Admin logged in', '{"method": "email"}', '192.168.1.1', NOW() - INTERVAL '30 minutes');

-- Insert sample system metrics
INSERT INTO system_metrics (metric_name, metric_value, metric_unit, tags, recorded_at) VALUES
    ('api_response_time', 125.50, 'ms', '{"endpoint": "/api/courses", "method": "GET"}', NOW() - INTERVAL '1 hour'),
    ('active_users', 15, 'count', '{"period": "current_hour"}', NOW() - INTERVAL '1 hour'),
    ('concurrent_sessions', 8, 'count', '{"timestamp": "current"}', NOW() - INTERVAL '1 hour'),
    ('error_rate', 2.5, 'percentage', '{"period": "last_hour"}', NOW() - INTERVAL '1 hour'),
    ('lessons_completed', 25, 'count', '{"period": "today"}', NOW() - INTERVAL '1 hour'),
    ('api_response_time', 98.75, 'ms', '{"endpoint": "/api/lessons", "method": "GET"}', NOW() - INTERVAL '2 hours'),
    ('active_users', 12, 'count', '{"period": "current_hour"}', NOW() - INTERVAL '2 hours');

-- Insert sample dashboard statistics
INSERT INTO dashboard_statistics (statistic_key, statistic_value, last_updated, expires_at) VALUES
    ('total_users', '5', NOW(), NOW() + INTERVAL '1 hour'),
    ('active_users_today', '3', NOW(), NOW() + INTERVAL '1 hour'),
    ('lessons_completed_today', '2', NOW(), NOW() + INTERVAL '1 hour'),
    ('courses_published', '2', NOW(), NOW() + INTERVAL '1 hour'),
    ('total_lessons', '4', NOW(), NOW() + INTERVAL '1 hour'),
    ('avg_session_duration', '{"value": 45.5, "unit": "minutes"}', NOW(), NOW() + INTERVAL '1 hour'),
    ('popular_courses', '[{"id": "python-basics", "title": "Python Basics", "completions": 2}, {"id": "javascript-intro", "title": "JavaScript Introduction", "completions": 1}]', NOW(), NOW() + INTERVAL '1 hour');

-- Insert sample user sessions
INSERT INTO user_sessions (user_id, session_token, ip_address, user_agent, started_at, last_activity) VALUES
    ('33333333-3333-3333-3333-333333333333', 'session_token_alice_123', '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36', NOW() - INTERVAL '1 hour', NOW() - INTERVAL '10 minutes'),
    ('44444444-4444-4444-4444-444444444444', 'session_token_bob_456', '192.168.1.101', 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36', NOW() - INTERVAL '2 hours', NOW() - INTERVAL '30 minutes'),
    ('11111111-1111-1111-1111-111111111111', 'session_token_admin_789', '192.168.1.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36', NOW() - INTERVAL '30 minutes', NOW() - INTERVAL '5 minutes');

-- Insert sample bulk operations
INSERT INTO bulk_operations (operation_type, performed_by, target_count, operation_data, status, started_at, completed_at) VALUES
    ('bulk_user_update', '11111111-1111-1111-1111-111111111111', 3, '{"field": "email_notifications", "value": true, "user_ids": ["33333333-3333-3333-3333-333333333333", "44444444-4444-4444-4444-444444444444", "55555555-5555-5555-5555-555555555555"]}', 'completed', NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day' + INTERVAL '5 minutes'),
    ('bulk_role_assign', '11111111-1111-1111-1111-111111111111', 2, '{"role": "User", "user_ids": ["33333333-3333-3333-3333-333333333333", "44444444-4444-4444-4444-444444444444"]}', 'completed', NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days' + INTERVAL '2 minutes');

-- Insert sample notifications
INSERT INTO notifications (user_id, type, title, message, is_read, created_at, metadata) VALUES
    ('33333333-3333-3333-3333-333333333333', 'achievement', 'Achievement Unlocked!', 'You have earned the "First Steps" achievement for completing your first lesson.', false, NOW() - INTERVAL '2 days', '{"achievement_id": "first-steps", "points": 10}'),
    ('44444444-4444-4444-4444-444444444444', 'progress', 'Lesson Completed', 'Congratulations on completing "Variables and Data Types"!', true, NOW() - INTERVAL '3 days', '{"lesson_id": "python-basics-variables", "course_id": "python-basics"}'),
    ('33333333-3333-3333-3333-333333333333', 'system', 'Welcome to AscendDev!', 'Welcome to our learning platform. Start your coding journey today!', true, NOW() - INTERVAL '5 days', '{"welcome_message": true}');

-- Insert sample discussions
INSERT INTO discussions (id, lesson_id, user_id, title, content, created_at, view_count) VALUES
    (gen_random_uuid(), 'python-basics-variables', '33333333-3333-3333-3333-333333333333', 'Question about variable naming', 'What are the best practices for naming variables in Python?', NOW() - INTERVAL '1 day', 5),
    (gen_random_uuid(), 'javascript-intro-basics', '44444444-4444-4444-4444-444444444444', 'Difference between var and let', 'Can someone explain the difference between var and let in JavaScript?', NOW() - INTERVAL '2 days', 8);

COMMIT;