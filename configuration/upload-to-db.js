const fs = require("fs");
const path = require("path");
const { Pool } = require("pg");
const { v4: uuidv4 } = require("uuid");
const crypto = require("crypto");

// PostgreSQL connection configuration
const pool = new Pool({
  user: process.env.POSTGRES_USER || "elearning_user",
  host: process.env.POSTGRES_HOST || "localhost",
  database: process.env.POSTGRES_DB || "elearning_db",
  password: process.env.POSTGRES_PASSWORD || "elearning_pass",
  port: process.env.POSTGRES_PORT || 5432,
});

// Map to track original IDs to new PostgreSQL IDs
const idMappings = {
  courses: {},
  lessons: {},
};

// File pattern for finding config files
const filePatterns = {
  course_config: /^course_.*\.json$/i,
  lesson_config: /^lesson_.*\.json$/i,
};

// Connect to PostgreSQL and setup tables
async function setupDatabase() {
  const client = await pool.connect();
  try {
    console.log("Connected to PostgreSQL");

    // Create courses table if it doesn't exist
    await client.query(`
      CREATE TABLE IF NOT EXISTS courses (
        id VARCHAR(50) PRIMARY KEY,
        title VARCHAR(255) NOT NULL,
        slug VARCHAR(255) NOT NULL UNIQUE,
        description TEXT DEFAULT '',
        created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
        language VARCHAR(50) NOT NULL,
        updated_at TIMESTAMP WITH TIME ZONE,
        featured_image VARCHAR(255),
        tags JSONB NOT NULL DEFAULT '[]'::jsonb,
        lesson_summaries JSONB NOT NULL DEFAULT '[]'::jsonb,
        status VARCHAR(50) NOT NULL DEFAULT 'draft',
        created_by VARCHAR(50) NOT NULL DEFAULT 'system'
      )
    `);

    // Create lessons table if it doesn't exist
    await client.query(`
      CREATE TABLE IF NOT EXISTS lessons (
        id VARCHAR(50) PRIMARY KEY,
        course_id VARCHAR(50) NOT NULL REFERENCES courses(id),
        title VARCHAR(255) NOT NULL,
        slug VARCHAR(255) NOT NULL,
        content TEXT,
        template TEXT,
        created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
        updated_at TIMESTAMP WITH TIME ZONE,
        language VARCHAR(50) NOT NULL,
        "order" INTEGER NOT NULL,
        test_config JSONB,
        additional_resources JSONB DEFAULT '[]'::jsonb,
        tags JSONB DEFAULT '[]'::jsonb,
        status VARCHAR(50) NOT NULL DEFAULT 'draft',
        UNIQUE(course_id, slug)
      )
    `);

    // Create indexes for better performance
    await client.query(
      `CREATE INDEX IF NOT EXISTS lessons_course_id_idx ON lessons(course_id)`
    );
    await client.query(
      `CREATE INDEX IF NOT EXISTS courses_status_idx ON courses(status)`
    );
    await client.query(
      `CREATE INDEX IF NOT EXISTS lessons_status_idx ON lessons(status)`
    );
    await client.query(
      `CREATE INDEX IF NOT EXISTS courses_tags_gin_idx ON courses USING GIN (tags jsonb_path_ops)`
    );
    await client.query(
      `CREATE INDEX IF NOT EXISTS lessons_tags_gin_idx ON lessons USING GIN (tags jsonb_path_ops)`
    );

    console.log("Database setup completed");
  } catch (err) {
    console.error("Error setting up database:", err);
    throw err;
  } finally {
    client.release();
  }
}

async function updateCourseConfig(client, courseConfigPath) {
  try {
    const configData = JSON.parse(fs.readFileSync(courseConfigPath, "utf8"));

    // Generate a course ID based on the slug if no original ID exists
    // Extract course name from the file path as a fallback
    const coursePathParts = courseConfigPath.split(path.sep);
    const courseFolder =
      coursePathParts[coursePathParts.length - 2] || "unknown_course";
    const courseSlug = configData.slug || courseFolder;

    // Generate a stable ID based on the slug
    const stableId = generateStableId(courseSlug);

    // Check if course already exists
    const existingCourse = await client.query(
      "SELECT id FROM courses WHERE slug = $1",
      [configData.slug]
    );

    let postgresId;

    if (existingCourse.rows.length > 0) {
      // Use existing PostgreSQL ID
      postgresId = existingCourse.rows[0].id;

      // Update existing course
      const result = await client.query(
        `UPDATE courses 
         SET title = $1, 
             language = $2, 
             updated_at = $3, 
             tags = $4, 
             status = $5,
             description = $6
         WHERE id = $7`,
        [
          configData.title,
          configData.language || "en",
          new Date(),
          JSON.stringify(configData.tags || []),
          configData.status || "draft",
          configData.description || "",
          postgresId,
        ]
      );
      console.log(`Updated course: ${configData.title} (${postgresId})`);
    } else {
      // Create new ID for PostgreSQL
      postgresId = stableId;

      // Insert new course
      const result = await client.query(
        `INSERT INTO courses 
         (id, title, slug, language, created_at, updated_at, tags, status, created_by, description)
         VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10)`,
        [
          postgresId,
          configData.title,
          configData.slug,
          configData.language || "en",
          configData.createdAt ? new Date(configData.createdAt) : new Date(),
          configData.updatedAt ? new Date(configData.updatedAt) : new Date(),
          JSON.stringify(configData.tags || []),
          configData.status || "draft",
          "system",
          configData.description || "",
        ]
      );
      console.log(`Inserted course: ${configData.title} (${postgresId})`);
    }

    // Store the mapping for directory name to PostgreSQL ID for lessons
    idMappings.courses[courseFolder] = postgresId;

    return postgresId;
  } catch (err) {
    console.error(`Error updating course from ${courseConfigPath}:`, err);
    return null;
  }
}

// Generate a stable ID based on a string (for consistency across runs)
function generateStableId(text) {
  const hash = crypto.createHash("md5").update(text).digest("hex");
  return (
    hash.substring(0, 8) +
    "-" +
    hash.substring(8, 12) +
    "-" +
    hash.substring(12, 16) +
    "-" +
    hash.substring(16, 20) +
    "-" +
    hash.substring(20, 32)
  );
}

async function updateLessonConfig(client, lessonConfigPath) {
  try {
    const configData = JSON.parse(fs.readFileSync(lessonConfigPath, "utf8"));

    // Extract lesson directory name and course directory from the path
    const pathParts = lessonConfigPath.split(path.sep);
    const lessonFolder = pathParts[pathParts.length - 2] || "unknown_lesson";
    const courseFolder = pathParts[pathParts.length - 3] || "unknown_course";

    // Generate lesson slug if not provided
    const lessonSlug = configData.slug || lessonFolder;

    // Generate a stable ID based on course and lesson slugs
    const stableId = generateStableId(`${courseFolder}_${lessonSlug}`);

    // Look up the PostgreSQL course ID from our mapping
    const mappedCourseId = idMappings.courses[courseFolder];

    if (!mappedCourseId) {
      console.error(
        `Error: Cannot find PostgreSQL ID for course folder ${courseFolder}. Skipping lesson ${configData.title}.`
      );
      return;
    }

    // Check if lesson already exists by course_id and slug
    const existingLesson = await client.query(
      "SELECT id FROM lessons WHERE course_id = $1 AND slug = $2",
      [mappedCourseId, lessonSlug]
    );

    let postgresId;

    if (existingLesson.rows.length > 0) {
      // Use existing PostgreSQL ID
      postgresId = existingLesson.rows[0].id;

      // Update existing lesson
      const result = await client.query(
        `UPDATE lessons 
         SET title = $1, 
             content = $2,
             template = $3,
             language = $4, 
             "order" = $5,
             updated_at = $6, 
             test_config = $7,
             additional_resources = $8,
             tags = $9, 
             status = $10
         WHERE id = $11`,
        [
          configData.title,
          configData.content || "",
          configData.template || "",
          configData.language || "en",
          configData.order || 1,
          new Date(),
          JSON.stringify(configData.testConfig || {}),
          JSON.stringify(configData.additionalResources || []),
          JSON.stringify(configData.tags || []),
          configData.status || "draft",
          postgresId,
        ]
      );
      console.log(`Updated lesson: ${configData.title} (${postgresId})`);
    } else {
      // Create new ID for PostgreSQL
      postgresId = stableId;

      // Insert new lesson
      const result = await client.query(
        `INSERT INTO lessons 
         (id, title, slug, course_id, content, template, created_at, updated_at, 
          language, "order", test_config, additional_resources, tags, status)
         VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14)`,
        [
          postgresId,
          configData.title,
          lessonSlug,
          mappedCourseId,
          configData.content || "",
          configData.template || "",
          configData.createdAt ? new Date(configData.createdAt) : new Date(),
          configData.updatedAt ? new Date(configData.updatedAt) : new Date(),
          configData.language || "en",
          configData.order || 1,
          JSON.stringify(configData.testConfig || {}),
          JSON.stringify(configData.additionalResources || []),
          JSON.stringify(configData.tags || []),
          configData.status || "draft",
        ]
      );
      console.log(`Inserted lesson: ${configData.title} (${postgresId})`);
    }

    // Store the mapping for reference
    idMappings.lessons[lessonFolder] = postgresId;
  } catch (err) {
    console.error(`Error updating lesson from ${lessonConfigPath}:`, err);
  }
}

async function traverseAndUpdateConfigs(rootDir) {
  const client = await pool.connect();

  try {
    // Find the courses directory
    const coursesDir = path.join(rootDir, "courses");

    if (!fs.existsSync(coursesDir)) {
      console.warn(`Courses directory not found: ${coursesDir}`);
      return;
    }

    // Process each course directory
    const courseDirs = fs
      .readdirSync(coursesDir, { withFileTypes: true })
      .filter((item) => item.isDirectory())
      .map((item) => path.join(coursesDir, item.name));

    // Start a transaction
    await client.query("BEGIN");

    // First pass: process all courses to build ID mappings
    console.log("First pass: Processing courses...");
    for (const courseDir of courseDirs) {
      // Find and update course config file
      const courseFiles = fs.readdirSync(courseDir);
      const courseConfigFile = courseFiles.find((file) =>
        filePatterns.course_config.test(file)
      );

      if (courseConfigFile) {
        await updateCourseConfig(
          client,
          path.join(courseDir, courseConfigFile)
        );
      } else {
        // If no course config found, use directory name to create a minimal course
        const courseDirName = path.basename(courseDir);
        const courseSlug = courseDirName
          .toLowerCase()
          .replace(/[^a-z0-9]+/g, "-");

        // Generate a minimal course config
        const minimalCourseConfig = {
          title: courseDirName
            .replace(/-/g, " ")
            .replace(/\b\w/g, (c) => c.toUpperCase()), // Convert to title case
          slug: courseSlug,
          language: "en",
          status: "draft",
          tags: [],
        };

        // Write temporary file
        const tempConfigPath = path.join(courseDir, "temp_course_config.json");
        fs.writeFileSync(
          tempConfigPath,
          JSON.stringify(minimalCourseConfig, null, 2)
        );

        // Process the temporary file
        await updateCourseConfig(client, tempConfigPath);

        // Clean up
        try {
          fs.unlinkSync(tempConfigPath);
        } catch (e) {
          /* ignore */
        }
      }
    }

    // Second pass: process all lessons using the course ID mappings
    console.log("Second pass: Processing lessons...");
    for (const courseDir of courseDirs) {
      // Find and process lesson directories
      const lessonDirs = fs
        .readdirSync(courseDir, { withFileTypes: true })
        .filter((item) => item.isDirectory())
        .map((item) => path.join(courseDir, item.name));

      for (const lessonDir of lessonDirs) {
        // Find lesson config files
        const lessonFiles = fs.readdirSync(lessonDir);
        const lessonConfigFile = lessonFiles.find((file) =>
          filePatterns.lesson_config.test(file)
        );

        if (lessonConfigFile) {
          await updateLessonConfig(
            client,
            path.join(lessonDir, lessonConfigFile)
          );
        }
      }
    }

    // Commit the transaction
    await client.query("COMMIT");

    // Print summary of ID mappings
    console.log("\nID Mapping Summary:");
    console.log(`Processed ${Object.keys(idMappings.courses).length} courses`);
    console.log(`Processed ${Object.keys(idMappings.lessons).length} lessons`);
  } catch (err) {
    // Rollback in case of error
    await client.query("ROLLBACK");
    console.error("Error during PostgreSQL update:", err);
    throw err;
  } finally {
    client.release();
  }
}

async function updateLessonSummaries() {
  const client = await pool.connect();

  try {
    // For each course, update the lesson summaries based on its lessons
    const coursesResult = await client.query("SELECT id, title FROM courses");

    for (const course of coursesResult.rows) {
      const courseId = course.id;

      // Get all lessons for this course
      const lessonsResult = await client.query(
        'SELECT id, title, slug, "order" FROM lessons WHERE course_id = $1 ORDER BY "order"',
        [courseId]
      );

      // Create lesson summaries
      const lessonSummaries = lessonsResult.rows.map((lesson) => ({
        id: lesson.id,
        title: lesson.title,
        slug: lesson.slug,
        order: lesson.order,
      }));

      // Update the course with lesson summaries
      await client.query(
        "UPDATE courses SET lesson_summaries = $1 WHERE id = $2",
        [JSON.stringify(lessonSummaries), courseId]
      );

      console.log(
        `Updated lesson summaries for course: ${course.title} (${courseId}) - ${lessonSummaries.length} lessons`
      );
    }
  } catch (err) {
    console.error("Error updating lesson summaries:", err);
  } finally {
    client.release();
  }
}

// Helper function to save ID mappings to a file for reference
async function saveIdMappings() {
  try {
    fs.writeFileSync(
      path.join(__dirname, "id_mappings.json"),
      JSON.stringify(idMappings, null, 2),
      "utf8"
    );
    console.log("ID mappings saved to id_mappings.json");
  } catch (err) {
    console.error("Error saving ID mappings:", err);
  }
}

async function main() {
  try {
    // Setup database tables and indexes
    await setupDatabase();

    console.log("Starting PostgreSQL update");

    // Process directories and update configs
    await traverseAndUpdateConfigs(__dirname);

    // Update lesson summaries for each course
    await updateLessonSummaries();

    // Save ID mappings to a file for reference
    await saveIdMappings();

    console.log("PostgreSQL update completed");
  } catch (err) {
    console.error("Error during PostgreSQL update:", err);
  } finally {
    // Close pool
    await pool.end();
    console.log("PostgreSQL connection closed");
  }
}

// Run the script
main().catch(console.error);
