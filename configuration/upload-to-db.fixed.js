const fs = require("fs");
const path = require("path");
const { Pool } = require("pg");
const { v4: uuidv4 } = require("uuid");

// PostgreSQL connection configuration - using environment variables with fallbacks
// SECURITY NOTE: In production, never hardcode credentials in your code
const pool = new Pool({
  user: process.env.POSTGRES_USER || "postgres",
  host: process.env.POSTGRES_HOST || "localhost",
  database: process.env.POSTGRES_DB || "ascenddev",
  password: process.env.POSTGRES_PASSWORD || "postgres",
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

async function updateCourseConfig(client, courseConfigPath) {
  try {
    const configData = JSON.parse(fs.readFileSync(courseConfigPath, "utf8"));

    // Extract course name from the file path as a fallback
    const coursePathParts = courseConfigPath.split(path.sep);
    const courseFolder =
      coursePathParts[coursePathParts.length - 2] || "unknown_course";
    const courseSlug = configData.slug || courseFolder;

    // Use course folder as the course ID
    const courseId = courseFolder;

    // Check if course already exists
    const existingCourse = await client.query(
      "SELECT id FROM courses WHERE slug = $1",
      [courseSlug]
    );

    let postgresId;

    if (existingCourse.rows.length > 0) {
      // Use existing PostgreSQL ID
      postgresId = existingCourse.rows[0].id;

      // Update existing course
      await client.query(
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
      // Use course folder as PostgreSQL ID
      postgresId = courseId;

      // Insert new course
      await client.query(
        `INSERT INTO courses
         (id, title, slug, language, created_at, updated_at, tags, status, description)
         VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)`,
        [
          postgresId,
          configData.title,
          courseSlug,
          configData.language || "en",
          configData.createdAt ? new Date(configData.createdAt) : new Date(),
          configData.updatedAt ? new Date(configData.updatedAt) : new Date(),
          JSON.stringify(configData.tags || []),
          configData.status || "draft",
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
    throw err; // Re-throw error to be caught by transaction handler
  }
}

async function updateLessonConfig(client, lessonConfigPath) {
  try {
    const configData = JSON.parse(fs.readFileSync(lessonConfigPath, "utf8"));

    // Extract lesson directory name and course directory from the path
    const pathParts = lessonConfigPath.split(path.sep);
    const lessonFolder = pathParts[pathParts.length - 2] || "unknown_lesson";
    const courseFolder = pathParts[pathParts.length - 3] || "unknown_course";

    // Generate lesson slug if not provided
    // Make the slug unique by prefixing it with the course folder
    const lessonBaseSlug = configData.slug || lessonFolder;
    const lessonSlug = `${courseFolder}-${lessonBaseSlug}`;

    // Generate lesson ID as course_folder_lesson_folder
    const lessonId = `${courseFolder}_${lessonFolder}`;

    // Look up the PostgreSQL course ID from our mapping
    const mappedCourseId = idMappings.courses[courseFolder];

    if (!mappedCourseId) {
      console.error(
        `Error: Cannot find PostgreSQL ID for course folder ${courseFolder}. Skipping lesson ${configData.title}.`
      );
      return;
    }

    // Check if lesson already exists by ID
    const existingLesson = await client.query(
      "SELECT id FROM lessons WHERE id = $1",
      [lessonId]
    );

    let postgresId;

    if (existingLesson.rows.length > 0) {
      // Use existing PostgreSQL ID
      postgresId = existingLesson.rows[0].id;

      // Update existing lesson
      await client.query(
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
             status = $10,
             course_id = $11,
             slug = $12
         WHERE id = $13`,
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
          mappedCourseId,
          lessonSlug,
          postgresId,
        ]
      );
      console.log(`Updated lesson: ${configData.title} (${postgresId})`);
    } else {
      // Use generated lesson ID
      postgresId = lessonId;

      // Insert new lesson
      await client.query(
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

    // Store the mapping using a combined key of course and lesson folder
    idMappings.lessons[`${courseFolder}_${lessonFolder}`] = postgresId;

    return postgresId;
  } catch (err) {
    console.error(`Error updating lesson from ${lessonConfigPath}:`, err);
    throw err; // Re-throw error to be caught by transaction handler
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
        // Try to find any JSON file that might be a course config
        const jsonFiles = courseFiles.filter((file) => file.endsWith(".json"));
        let foundCourseConfig = false;

        for (const jsonFile of jsonFiles) {
          try {
            const filePath = path.join(courseDir, jsonFile);
            const content = JSON.parse(fs.readFileSync(filePath, "utf8"));

            // Check if this looks like a course config
            if (content.title && content.slug && content.language) {
              console.log(`Found potential course config: ${jsonFile}`);
              await updateCourseConfig(client, filePath);
              foundCourseConfig = true;
              break;
            }
          } catch (e) {
            console.warn(`Error parsing ${jsonFile}: ${e.message}`);
          }
        }

        // If no course config found, use directory name to create a minimal course
        if (!foundCourseConfig) {
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
          const tempConfigPath = path.join(
            courseDir,
            "temp_course_config.json"
          );
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
            console.warn(
              `Could not delete temp file ${tempConfigPath}: ${e.message}`
            );
          }
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
        } else {
          // Try to find any JSON file that might be a lesson config
          const jsonFiles = lessonFiles.filter((file) =>
            file.endsWith(".json")
          );

          for (const jsonFile of jsonFiles) {
            try {
              const filePath = path.join(lessonDir, jsonFile);
              const content = JSON.parse(fs.readFileSync(filePath, "utf8"));

              // Check if this looks like a lesson config
              if (
                content.title &&
                content.language &&
                (content.content || content.template)
              ) {
                console.log(`Found potential lesson config: ${jsonFile}`);
                await updateLessonConfig(client, filePath);
                break;
              }
            } catch (e) {
              console.warn(`Error parsing ${jsonFile}: ${e.message}`);
            }
          }
        }
      }
    }

    // Update lesson summaries in courses
    await updateLessonSummaries(client);

    // Commit the transaction
    await client.query("COMMIT");

    // Print summary of ID mappings
    console.log("\nID Mapping Summary:");
    console.log(`Processed ${Object.keys(idMappings.courses).length} courses`);
    console.log(`Processed ${Object.keys(idMappings.lessons).length} lessons`);

    // Save ID mappings to file for reference
    await saveIdMappings();
  } catch (err) {
    // Rollback in case of error
    await client.query("ROLLBACK");
    console.error("Error during PostgreSQL update:", err);
    throw err;
  } finally {
    client.release();
  }
}

// Update lesson summaries for each course
async function updateLessonSummaries(client) {
  console.log("Updating lesson summaries for courses...");

  try {
    // Get all courses
    const coursesResult = await client.query("SELECT id FROM courses");

    for (const course of coursesResult.rows) {
      const courseId = course.id;

      // Get all lessons for this course, ordered by their order field
      const lessonsResult = await client.query(
        `SELECT id, title, slug, "order", status
         FROM lessons
         WHERE course_id = $1
         ORDER BY "order" ASC`,
        [courseId]
      );

      // Create lesson summaries array
      const lessonSummaries = lessonsResult.rows.map((lesson) => ({
        id: lesson.id,
        title: lesson.title,
        slug: lesson.slug,
        order: lesson.order,
        status: lesson.status,
      }));

      // Update the course with lesson summaries
      await client.query(
        `UPDATE courses
         SET lesson_summaries = $1
         WHERE id = $2`,
        [JSON.stringify(lessonSummaries), courseId]
      );

      console.log(
        `Updated lesson summaries for course ID: ${courseId} (${lessonSummaries.length} lessons)`
      );
    }
  } catch (err) {
    console.error("Error updating lesson summaries:", err);
    throw err;
  }
}

// Save ID mappings to a JSON file for reference
async function saveIdMappings() {
  try {
    const mappingsFile = path.join(process.cwd(), "id_mappings.json");
    fs.writeFileSync(mappingsFile, JSON.stringify(idMappings, null, 2));
    console.log(`ID mappings saved to ${mappingsFile}`);
  } catch (err) {
    console.error("Error saving ID mappings:", err);
  }
}

// Main function to run the entire process
async function main() {
  const client = await pool.connect();
  try {
    console.log("Starting database migration...");

    // Check if database connection credentials are provided
    if (!process.env.POSTGRES_HOST || !process.env.POSTGRES_PASSWORD) {
      console.warn(
        "Database credentials not provided in environment variables."
      );
      console.warn(
        "Required variables: POSTGRES_USER, POSTGRES_HOST, POSTGRES_DB, POSTGRES_PASSWORD"
      );
      console.warn("Using fallback values is not recommended for production.");
    }

    // Get root directory from command line or use current directory
    const rootDir = process.argv[2] || process.cwd();
    console.log(`Using root directory: ${rootDir}`);

    // Process all course and lesson configs
    await traverseAndUpdateConfigs(rootDir);

    console.log("Database migration completed successfully!");
  } catch (err) {
    console.error("Migration failed:", err);
    process.exit(1);
  } finally {
    client.release();
    // Close the pool
    await pool.end();
  }
}

// Run the script
main().catch(console.error);
