using AscendDev.Core.DTOs.Social;
using AscendDev.Core.Models.Social;

namespace AscendDev.Core.Interfaces.Services;

public interface IDiscussionService
{
    Task<DiscussionResponse?> GetByIdAsync(Guid id, Guid? currentUserId = null);
    Task<IEnumerable<DiscussionResponse>> GetByLessonIdAsync(string lessonId, int page = 1, int pageSize = 20, Guid? currentUserId = null);
    Task<IEnumerable<DiscussionResponse>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<DiscussionResponse> CreateAsync(CreateDiscussionRequest request, Guid userId);
    Task<DiscussionResponse> UpdateAsync(Guid id, UpdateDiscussionRequest request, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task<bool> IncrementViewCountAsync(Guid id);
    Task<IEnumerable<DiscussionResponse>> GetPinnedByLessonIdAsync(string lessonId);
    Task<int> GetTotalCountByLessonIdAsync(string lessonId);
    Task<IEnumerable<DiscussionResponse>> SearchAsync(string searchTerm, string? lessonId = null, int page = 1, int pageSize = 20);

    // Course-level discussion methods
    Task<IEnumerable<DiscussionResponse>> GetByCourseIdAsync(string courseId, int page = 1, int pageSize = 20, Guid? currentUserId = null);
    Task<int> GetTotalCountByCourseIdAsync(string courseId);
    Task<IEnumerable<DiscussionResponse>> GetPinnedByCourseIdAsync(string courseId);

    // Like/Unlike methods
    Task<bool> LikeDiscussionAsync(Guid discussionId, Guid userId);
    Task<bool> UnlikeDiscussionAsync(Guid discussionId, Guid userId);
    Task<int> GetLikeCountAsync(Guid discussionId);
    Task<bool> IsLikedByUserAsync(Guid discussionId, Guid userId);
}