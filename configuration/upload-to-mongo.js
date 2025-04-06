const fs = require("fs");
const path = require("path");
const { MongoClient } = require("mongodb");

const uri =
  process.env.MONGODB_URI ||
  "mongodb://elearning_user:elearning_pass@localhost:27017/elearning_db?authSource=admin";
const dbName = process.env.DB_NAME || "elearning_db";
const coursesCollection = "courses";
const lessonsCollection = "lessons";

// File pattern for finding config files
const filePatterns = {
  course_config: /^course_.*\.json$/i,
  lesson_config: /^lesson_.*\.json$/i,
};

async function connectToMongoDB() {
  try {
    const client = new MongoClient(uri, { useUnifiedTopology: true });
    await client.connect();
    console.log("Connected to MongoDB");
    return client;
  } catch (err) {
    console.error("Error connecting to MongoDB:", err);
    throw err;
  }
}

async function updateCourseConfig(db, courseConfigPath) {
  try {
    const configData = JSON.parse(fs.readFileSync(courseConfigPath, "utf8"));
    const courseId = configData._id;

    // Check if course already exists
    const existingCourse = await db
      .collection(coursesCollection)
      .findOne({ _id: courseId });

    if (existingCourse) {
      // Update existing course
      const result = await db
        .collection(coursesCollection)
        .updateOne({ _id: courseId }, { $set: configData });
      console.log(
        `Updated course: ${courseId} - Modified: ${result.modifiedCount}`
      );
    } else {
      // Insert new course
      const result = await db
        .collection(coursesCollection)
        .insertOne(configData);
      console.log(`Inserted course: ${courseId}`);
    }
  } catch (err) {
    console.error(`Error updating course from ${courseConfigPath}:`, err);
  }
}

async function updateLessonConfig(db, lessonConfigPath) {
  try {
    const configData = JSON.parse(fs.readFileSync(lessonConfigPath, "utf8"));
    const lessonId = configData._id;

    // Check if lesson already exists
    const existingLesson = await db
      .collection(lessonsCollection)
      .findOne({ _id: lessonId });

    if (existingLesson) {
      // Update existing lesson
      const result = await db
        .collection(lessonsCollection)
        .updateOne({ _id: lessonId }, { $set: configData });
      console.log(
        `Updated lesson: ${lessonId} - Modified: ${result.modifiedCount}`
      );
    } else {
      // Insert new lesson
      const result = await db
        .collection(lessonsCollection)
        .insertOne(configData);
      console.log(`Inserted lesson: ${lessonId}`);
    }
  } catch (err) {
    console.error(`Error updating lesson from ${lessonConfigPath}:`, err);
  }
}

async function traverseAndUpdateConfigs(rootDir, db) {
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

  for (const courseDir of courseDirs) {
    // Find and update course config file
    const courseFiles = fs.readdirSync(courseDir);
    const courseConfigFile = courseFiles.find((file) =>
      filePatterns.course_config.test(file)
    );

    if (courseConfigFile) {
      await updateCourseConfig(db, path.join(courseDir, courseConfigFile));
    }

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
        await updateLessonConfig(db, path.join(lessonDir, lessonConfigFile));
      }
    }
  }
}

async function main() {
  let client;

  try {
    client = await connectToMongoDB();
    const db = client.db(dbName);

    console.log(`Starting MongoDB update for database: ${dbName}`);
    console.log(`Collections: ${coursesCollection}, ${lessonsCollection}`);

    // Create indexes for better performance
    await db
      .collection(coursesCollection)
      .createIndex({ slug: 1 }, { unique: true });
    await db
      .collection(lessonsCollection)
      .createIndex({ courseId: 1, slug: 1 }, { unique: true });

    // Process directories and update configs
    await traverseAndUpdateConfigs(__dirname, db);

    console.log("MongoDB update completed");
  } catch (err) {
    console.error("Error during MongoDB update:", err);
  } finally {
    if (client) {
      await client.close();
      console.log("MongoDB connection closed");
    }
  }
}

// Run the script
main().catch(console.error);
