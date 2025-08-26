using AscendDev.Core.DTOs.Social;
using AscendDev.Core.Models.Social;

namespace AscendDev.Core.Interfaces.Services;

public interface IDiscussionService
{
    Task<DiscussionResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<DiscussionResponse>> GetByLessonIdAsync(string lessonId, int page = 1, int pageSize = 20);
    Task<IEnumerable<DiscussionResponse>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<DiscussionResponse> CreateAsync(CreateDiscussionRequest request, Guid userId);
    Task<DiscussionResponse> UpdateAsync(Guid id, UpdateDiscussionRequest request, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task<bool> IncrementViewCountAsync(Guid id);
    Task<IEnumerable<DiscussionResponse>> GetPinnedByLessonIdAsync(string lessonId);
    Task<int> GetTotalCountByLessonIdAsync(string lessonId);
    Task<IEnumerable<DiscussionResponse>> SearchAsync(string searchTerm, string? lessonId = null, int page = 1, int pageSize = 20);
}