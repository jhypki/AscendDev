namespace AscendDev.Core.Caching;

public static class CacheKeys
{
    private const string CoursePrefix = "course";
    private const string LessonPrefix = "lesson";

    public static string CourseAll()
    {
        return $"{CoursePrefix}_all";
    }

    public static string CourseById(string id)
    {
        return $"{CoursePrefix}_{id}";
    }

    public static string CourseByLanguage(string language)
    {
        return $"{CoursePrefix}_language_{language}";
    }

    public static string CourseBySlug(string slug)
    {
        return $"{CoursePrefix}_slug_{slug}";
    }

    public static string CoursesByTag(string tag)
    {
        return $"{CoursePrefix}s_tag_{tag}";
    }

    public static string CoursePattern()
    {
        return $"{CoursePrefix}*";
    }

    public static string LessonsByCourseId(string courseId)
    {
        return $"{LessonPrefix}s_course_{courseId}";
    }

    public static string LessonById(string id)
    {
        return $"{LessonPrefix}_{id}";
    }

    public static string LessonBySlug(string slug)
    {
        return $"{LessonPrefix}_slug_{slug}";
    }

    public static string LessonPattern()
    {
        return $"{LessonPrefix}*";
    }

    public static string LessonsByCoursePattern(string courseId)
    {
        return $"{LessonPrefix}s_course_{courseId}*";
    }
}