
-- Fix Code Review Constraints Migration
-- This script adds constraints to prevent duplicate code reviews and self-reviews

-- Add unique constraint to prevent duplicate reviews for the same submission by the same reviewer
ALTER TABLE code_reviews 
ADD CONSTRAINT unique_submission_reviewer 
UNIQUE (submission_id, reviewer_id);

-- Add check constraint to prevent users from reviewing their own code
ALTER TABLE code_reviews 
ADD CONSTRAINT check_reviewer_not_reviewee 
CHECK (reviewer_id != reviewee_id);

