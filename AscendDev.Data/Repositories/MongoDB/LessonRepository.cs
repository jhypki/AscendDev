using AscendDev.Core.Caching;
using AscendDev.Core.Constants;
using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Courses;
using MongoDB.Driver;

namespace AscendDev.Data.Repositories.MongoDB;

public class LessonRepository : ILessonRepository
{
    private readonly ICachingService _cachingService;
    private readonly IMongoCollection<Lesson> _lessons;

    public LessonRepository(
        IConnectionManager<IMongoDatabase> mongoDbConnection,
        ICachingService cachingService)
    {
        ArgumentNullException.ThrowIfNull(mongoDbConnection);
        _cachingService = cachingService ?? throw new ArgumentNullException(nameof(cachingService));

        _lessons = mongoDbConnection.GetConnection()
            .GetCollection<Lesson>(MongoDBCollections.Lessons);
    }

    public async Task<List<Lesson>> GetByCourseId(string courseId)
    {
        var cacheKey = CacheKeys.LessonsByCourseId(courseId);

        try
        {
            return await _cachingService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _lessons.Find(Builders<Lesson>.Filter.Eq(l => l.CourseId, courseId))
                    .ToListAsync();
            });
        }
        catch (Exception ex)
        {
            throw new InternalServerErrorException($"Error retrieving lessons for course with ID: {courseId}",
                ex.Message);
        }
    }

    public async Task<Lesson> GetById(string lessonId)
    {
        var cacheKey = CacheKeys.LessonById(lessonId);

        try
        {
            return await _cachingService.GetOrCreateAsync(cacheKey, async () =>
            {
                var lesson = await _lessons.Find(Builders<Lesson>.Filter.Eq(l => l.Id, lessonId))
                    .FirstOrDefaultAsync();

                if (lesson == null)
                    throw new NotFoundException("Lesson", lessonId);

                return lesson;
            });
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving lesson with ID: {lessonId}", ex);
        }
    }

    public async Task<Lesson> GetBySlug(string slug)
    {
        var cacheKey = CacheKeys.LessonBySlug(slug);

        try
        {
            return await _cachingService.GetOrCreateAsync(cacheKey, async () =>
            {
                var lesson = await _lessons.Find(Builders<Lesson>.Filter.Eq(l => l.Slug, slug))
                    .FirstOrDefaultAsync();

                if (lesson == null)
                    throw new NotFoundException("Lesson", $"with slug '{slug}'");

                return lesson;
            });
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving lesson with slug: {slug}", ex);
        }
    }
}