using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Interfaces.Data;

public interface IUserLessonCodeRepository
{
    Task<UserLessonCode?> GetUserCodeAsync(Guid userId, string lessonId);
    Task<UserLessonCode> SaveUserCodeAsync(Guid userId, string lessonId, string code);
    Task<bool> DeleteUserCodeAsync(Guid userId, string lessonId);
}