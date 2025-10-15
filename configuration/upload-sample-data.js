const fs = require("fs");
const path = require("path");
const { Pool } = require("pg");

// PostgreSQL connection configuration - using environment variables with fallbacks
const pool = new Pool({
    user: process.env.POSTGRES_USER || "postgres",
    host: process.env.POSTGRES_HOST || "localhost",
    database: process.env.POSTGRES_DB || "ascenddev",
    password: process.env.POSTGRES_PASSWORD || "postgres",
    port: process.env.POSTGRES_PORT || 5432,
});

async function uploadSampleData() {
    const client = await pool.connect();

    try {
        console.log("Starting sample data upload...");

        // Read the sample data JSON file
        const sampleDataPath = path.join(__dirname, "sample-data.json");
        const sampleData = JSON.parse(fs.readFileSync(sampleDataPath, "utf8"));

        // Start a transaction
        await client.query("BEGIN");

        console.log("Uploading courses...");

        // Upload courses first (since lessons depend on courses)
        for (const course of sampleData.courses) {
            // Check if course already exists
            const existingCourse = await client.query(
                "SELECT id FROM courses WHERE id = $1",
                [course.id]
            );

            if (existingCourse.rows.length > 0) {
                // Update existing course
                await client.query(
                    `UPDATE courses
           SET title = $2, slug = $3, description = $4, language = $5,
               status = $6, created_by = $7, last_modified_by = $8,
               tags = $9, current_version = $10, has_draft_version = $11,
               featured_image = $12, updated_at = NOW()
           WHERE id = $1`,
                    [
                        course.id,
                        course.title,
                        course.slug,
                        course.description,
                        course.language,
                        course.status,
                        course.created_by,
                        course.last_modified_by,
                        JSON.stringify(course.tags),
                        course.current_version,
                        course.has_draft_version,
                        course.featured_image
                    ]
                );
                console.log(`Updated course: ${course.title} (${course.id})`);
            } else {
                // Insert new course
                await client.query(
                    `INSERT INTO courses
           (id, title, slug, description, language, status, created_by,
            last_modified_by, tags, current_version, has_draft_version,
            featured_image, created_at, updated_at)
           VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, NOW(), NOW())`,
                    [
                        course.id,
                        course.title,
                        course.slug,
                        course.description,
                        course.language,
                        course.status,
                        course.created_by,
                        course.last_modified_by,
                        JSON.stringify(course.tags),
                        course.current_version,
                        course.has_draft_version,
                        course.featured_image
                    ]
                );
                console.log(`Inserted course: ${course.title} (${course.id})`);
            }
        }

        console.log("Uploading lessons...");

        // Upload lessons
        for (const lesson of sampleData.lessons) {
            // Check if lesson already exists
            const existingLesson = await client.query(
                "SELECT id FROM lessons WHERE id = $1",
                [lesson.id]
            );

            if (existingLesson.rows.length > 0) {
                // Update existing lesson
                await client.query(
                    `UPDATE lessons 
           SET course_id = $2, title = $3, slug = $4, content = $5, 
               language = $6, template = $7, "order" = $8, test_config = $9, 
               additional_resources = $10, tags = $11, status = $12, 
               updated_at = NOW()
           WHERE id = $1`,
                    [
                        lesson.id,
                        lesson.course_id,
                        lesson.title,
                        lesson.slug,
                        lesson.content,
                        lesson.language,
                        lesson.template,
                        lesson.order,
                        JSON.stringify(lesson.test_config),
                        JSON.stringify(lesson.additional_resources),
                        JSON.stringify(lesson.tags),
                        lesson.status
                    ]
                );
                console.log(`Updated lesson: ${lesson.title} (${lesson.id})`);
            } else {
                // Insert new lesson
                await client.query(
                    `INSERT INTO lessons 
           (id, course_id, title, slug, content, language, template, 
            "order", test_config, additional_resources, tags, status, 
            created_at, updated_at)
           VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, NOW(), NOW())`,
                    [
                        lesson.id,
                        lesson.course_id,
                        lesson.title,
                        lesson.slug,
                        lesson.content,
                        lesson.language,
                        lesson.template,
                        lesson.order,
                        JSON.stringify(lesson.test_config),
                        JSON.stringify(lesson.additional_resources),
                        JSON.stringify(lesson.tags),
                        lesson.status
                    ]
                );
                console.log(`Inserted lesson: ${lesson.title} (${lesson.id})`);
            }
        }

        // Commit the transaction
        await client.query("COMMIT");

        console.log("\nSample data upload completed successfully!");
        console.log(`Processed ${sampleData.courses.length} courses`);
        console.log(`Processed ${sampleData.lessons.length} lessons`);

    } catch (err) {
        // Rollback in case of error
        await client.query("ROLLBACK");
        console.error("Error during sample data upload:", err);
        throw err;
    } finally {
        client.release();
    }
}

// Validation function to check if required users exist
async function validateRequiredData() {
    const client = await pool.connect();

    try {
        // Check if the instructor user exists (used as created_by in sample data)
        const instructorUser = await client.query(
            "SELECT id FROM users WHERE id = $1",
            ["22222222-2222-2222-2222-222222222222"]
        );

        if (instructorUser.rows.length === 0) {
            console.warn("Warning: Instructor user (22222222-2222-2222-2222-222222222222) not found.");
            console.warn("You may need to run the main initdb.sql script first to create sample users.");
            return false;
        }

        return true;
    } catch (err) {
        console.error("Error validating required data:", err);
        return false;
    } finally {
        client.release();
    }
}

// Main function to run the entire process
async function main() {
    try {
        console.log("Starting sample data upload process...");

        // Check database connection
        const client = await pool.connect();
        console.log("Database connection successful");
        client.release();

        // Validate required data exists
        const isValid = await validateRequiredData();
        if (!isValid) {
            console.log("Continuing with upload despite warnings...");
        }

        // Upload sample data
        await uploadSampleData();

        console.log("Sample data upload process completed successfully!");

    } catch (err) {
        console.error("Sample data upload failed:", err);
        process.exit(1);
    } finally {
        // Close the pool
        await pool.end();
    }
}

// Handle command line arguments
if (require.main === module) {
    // Check if help is requested
    if (process.argv.includes("--help") || process.argv.includes("-h")) {
        console.log(`
Usage: node upload-sample-data.js [options]

Options:
  --help, -h     Show this help message

Environment Variables:
  POSTGRES_USER     PostgreSQL username (default: postgres)
  POSTGRES_HOST     PostgreSQL host (default: localhost)
  POSTGRES_DB       PostgreSQL database name (default: ascenddev)
  POSTGRES_PASSWORD PostgreSQL password (default: postgres)
  POSTGRES_PORT     PostgreSQL port (default: 5432)

Description:
  This script uploads sample courses and lessons data from sample-data.json
  to the PostgreSQL database. It will create new records or update existing
  ones based on the ID fields.

Examples:
  node upload-sample-data.js
  POSTGRES_HOST=localhost POSTGRES_PASSWORD=mypass node upload-sample-data.js
    `);
        process.exit(0);
    }

    // Run the main function
    main().catch(console.error);
}

module.exports = {
    uploadSampleData,
    validateRequiredData
};