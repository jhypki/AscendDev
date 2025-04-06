using AscendDev.Core.Caching;
using AscendDev.Core.Constants;
using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Courses;
using MongoDB.Driver;

namespace AscendDev.Data.Repositories.MongoDB;

public class CourseRepository : ICourseRepository
{
    private readonly ICachingService _cachingService;
    private readonly IMongoCollection<Course> _courses;

    public CourseRepository(
        IConnectionManager<IMongoDatabase> mongoDbConnection,
        ICachingService cachingService)
    {
        ArgumentNullException.ThrowIfNull(mongoDbConnection);
        _cachingService = cachingService ?? throw new ArgumentNullException(nameof(cachingService));

        _courses = mongoDbConnection.GetConnection()
            .GetCollection<Course>(MongoDBCollections.Courses);
    }

    public async Task<List<Course>> GetAll()
    {
        var cacheKey = CacheKeys.CourseAll();

        try
        {
            return await _cachingService.GetOrCreateAsync(cacheKey,
                async () => { return await _courses.Find(_ => true).ToListAsync(); });
        }
        catch (Exception ex)
        {
            throw new Exception("Error retrieving all courses", ex);
        }
    }

    public async Task<Course> GetById(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Course ID cannot be null or empty", nameof(id));

        var cacheKey = CacheKeys.CourseById(id);

        try
        {
            return await _cachingService.GetOrCreateAsync(cacheKey, async () =>
            {
                var course = await _courses.Find(Builders<Course>.Filter.Eq(c => c.Id, id))
                    .FirstOrDefaultAsync();

                if (course == null)
                    throw new NotFoundException("Course", id);

                return course;
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving course with ID: {id}", ex);
        }
    }

    public async Task<Course> GetByLanguage(string language)
    {
        if (string.IsNullOrEmpty(language))
            throw new ArgumentException("Language cannot be null or empty", nameof(language));

        var cacheKey = CacheKeys.CourseByLanguage(language);

        try
        {
            return await _cachingService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _courses.Find(Builders<Course>.Filter.Eq(c => c.Language, language))
                    .FirstOrDefaultAsync();
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving course with language: {language}", ex);
        }
    }

    public async Task<Course> GetBySlug(string slug)
    {
        if (string.IsNullOrEmpty(slug))
            throw new ArgumentException("Slug cannot be null or empty", nameof(slug));

        var cacheKey = CacheKeys.CourseBySlug(slug);

        try
        {
            return await _cachingService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _courses.Find(Builders<Course>.Filter.Eq(c => c.Slug, slug))
                    .FirstOrDefaultAsync();
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving course with slug: {slug}", ex);
        }
    }

    public async Task<List<Course>> GetByTag(string tag)
    {
        if (string.IsNullOrEmpty(tag))
            throw new ArgumentException("Tag cannot be null or empty", nameof(tag));

        var cacheKey = CacheKeys.CoursesByTag(tag);

        try
        {
            return await _cachingService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _courses.Find(Builders<Course>.Filter.ElemMatch(c => c.Tags, tag))
                    .ToListAsync();
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving courses with tag: {tag}", ex);
        }
    }
}