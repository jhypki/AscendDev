const fs = require("fs");
const path = require("path");

const filePatterns = {
  config: /config\.json$/i,
  code_template: /code_template\.[a-z]+$/i,
  test_template: /test_template\.[a-z]+$/i,
  course_config: /^course_.*\.json$/i,
};

const courses = {}; // Stores courses and their associated lessons

function findFiles(directory) {
  const files = fs.readdirSync(directory);
  const matchedFiles = {
    config: null,
    code_template: null,
    test_template: null,
  };

  files.forEach((file) => {
    if (filePatterns.config.test(file)) matchedFiles.config = file;
    else if (filePatterns.code_template.test(file))
      matchedFiles.code_template = file;
    else if (filePatterns.test_template.test(file))
      matchedFiles.test_template = file;
  });

  return matchedFiles;
}

function processLesson(lessonDir, courseName) {
  console.log(`Processing lesson directory: ${lessonDir}`);
  const matchedFiles = findFiles(lessonDir);

  if (
    !matchedFiles.config ||
    !matchedFiles.code_template ||
    !matchedFiles.test_template
  ) {
    console.warn(`Skipping ${lessonDir}: Missing required files.`);
    return;
  }

  try {
    const configData = JSON.parse(
      fs.readFileSync(path.join(lessonDir, matchedFiles.config), "utf8")
    );
    const codeTemplate = fs.readFileSync(
      path.join(lessonDir, matchedFiles.code_template),
      "utf8"
    );
    const testTemplate = fs.readFileSync(
      path.join(lessonDir, matchedFiles.test_template),
      "utf8"
    );

    const lessonName = path.basename(lessonDir);
    const courseId = courseName;
    const lessonId = `${courseId}_${lessonName}`;
    const finalFilename = path.join(lessonDir, `lesson_${lessonId}.json`);

    let createdAt = new Date().toISOString();
    let existingConfig = null;

    if (fs.existsSync(finalFilename)) {
      existingConfig = JSON.parse(fs.readFileSync(finalFilename, "utf8"));
      createdAt = existingConfig.createdAt || createdAt;
    }

    const updatedAt = new Date().toISOString();

    const finalConfig = {
      _id: lessonId,
      courseId,
      title: configData.title || lessonName,
      slug: configData.slug || lessonName.toLowerCase(),
      content: configData.content || "",
      template: codeTemplate,
      createdAt,
      updatedAt,
      language: configData.language || "javascript",
      order: configData.order || 0,
      testConfig: {
        framework: configData.testConfig?.framework || "jest",
        timeoutMs: configData.testConfig?.timeoutMs || 3000,
        memoryLimitMb: configData.testConfig?.memoryLimitMb || 512,
        testTemplate: testTemplate.replace(
          "{testCases}",
          JSON.stringify(configData.testConfig?.testCases || [], null, 2)
        ),
        testCases: configData.testConfig?.testCases || [],
        mainFunction: configData.testConfig?.mainFunction || "",
        dependencies: configData.testConfig?.dependencies || [],
      },
      additionalResources: configData.additionalResources || [],
      tags: configData.tags || [],
    };

    fs.writeFileSync(finalFilename, JSON.stringify(finalConfig, null, 2));
    console.log(`Generated/Updated: ${finalFilename}`);

    // Store lesson data for course update
    if (!courses[courseId]) {
      courses[courseId] = {
        lessons: [],
        updatedAt: updatedAt,
      };
    }

    courses[courseId].lessons.push({
      lessonId,
      title: finalConfig.title,
      slug: finalConfig.slug,
      order: finalConfig.order,
    });
  } catch (err) {
    console.error(`Error processing ${lessonDir}:`, err);
  }
}

function processCourse(courseDir) {
  const courseName = path.basename(courseDir);
  console.log(`Processing course directory: ${courseName}`);

  // Process lesson directories within the course
  fs.readdirSync(courseDir, { withFileTypes: true }).forEach((item) => {
    if (item.isDirectory()) {
      const lessonDir = path.join(courseDir, item.name);
      processLesson(lessonDir, courseName);
    }
  });

  // Update course config file
  if (courses[courseName]) {
    // Find the existing course config file if it exists
    let courseConfigFile = null;
    const files = fs.readdirSync(courseDir);
    for (const file of files) {
      if (filePatterns.course_config.test(file)) {
        courseConfigFile = path.join(courseDir, file);
        break;
      }
    }

    try {
      let courseData = {};

      // If course config file exists, read it
      if (courseConfigFile && fs.existsSync(courseConfigFile)) {
        courseData = JSON.parse(fs.readFileSync(courseConfigFile, "utf8"));
      } else {
        // Create default course config
        courseConfigFile = path.join(courseDir, `course_${courseName}.json`);
        courseData = {
          _id: courseName,
          title: courseName.replace(/_/g, " "),
          slug: courseName.toLowerCase().replace(/_/g, "-"),
          description: `Learn ${courseName.replace(/_/g, " ")} from scratch.`,
          createdAt: new Date().toISOString(),
          featuredImage: "",
          tags: [],
          status: "published",
          createdBy: "",
        };
      }

      // Update course data with lessons
      courseData.lessons = courses[courseName].lessons.sort(
        (a, b) => a.order - b.order
      );
      courseData.updatedAt = new Date().toISOString();

      fs.writeFileSync(courseConfigFile, JSON.stringify(courseData, null, 2));
      console.log(`Updated/Created course config: ${courseConfigFile}`);
    } catch (err) {
      console.error(`Error updating course ${courseName}:`, err);
    }
  }
}

function traverseDirectories(rootDir) {
  // Find the courses directory
  const coursesDir = path.join(rootDir, "courses");

  if (!fs.existsSync(coursesDir)) {
    console.warn(`Courses directory not found: ${coursesDir}`);
    return;
  }

  // Process each course directory
  fs.readdirSync(coursesDir, { withFileTypes: true }).forEach((item) => {
    if (item.isDirectory()) {
      const courseDir = path.join(coursesDir, item.name);
      processCourse(courseDir);
    }
  });
}

const configurationDir = __dirname;
console.log(`Starting from ${configurationDir}...`);
traverseDirectories(configurationDir);
