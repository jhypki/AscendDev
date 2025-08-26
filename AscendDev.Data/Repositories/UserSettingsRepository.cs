using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Auth;

namespace AscendDev.Data.Repositories;

public class UserSettingsRepository : IUserSettingsRepository
{
    private readonly ISqlExecutor _sqlExecutor;

    public UserSettingsRepository(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public async Task<UserSettings?> GetByUserIdAsync(Guid userId)
    {
        const string sql = @"
            SELECT * FROM user_settings 
            WHERE user_id = @UserId";

        return await _sqlExecutor.QueryFirstOrDefaultAsync<UserSettings>(sql, new { UserId = userId });
    }

    public async Task<UserSettings?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT * FROM user_settings 
            WHERE id = @Id";

        return await _sqlExecutor.QueryFirstOrDefaultAsync<UserSettings>(sql, new { Id = id });
    }

    public async Task<Guid> CreateAsync(UserSettings userSettings)
    {
        const string sql = @"
            INSERT INTO user_settings (id, user_id, public_submissions, show_profile, email_on_code_review, email_on_discussion_reply, created_at)
            VALUES (@Id, @UserId, @PublicSubmissions, @ShowProfile, @EmailOnCodeReview, @EmailOnDiscussionReply, @CreatedAt)
            RETURNING id";

        if (userSettings.Id == Guid.Empty)
            userSettings.Id = Guid.NewGuid();

        if (userSettings.CreatedAt == default)
            userSettings.CreatedAt = DateTime.UtcNow;

        return await _sqlExecutor.QuerySingleAsync<Guid>(sql, userSettings);
    }

    public async Task UpdateAsync(UserSettings userSettings)
    {
        const string sql = @"
            UPDATE user_settings 
            SET public_submissions = @PublicSubmissions, 
                show_profile = @ShowProfile, 
                email_on_code_review = @EmailOnCodeReview, 
                email_on_discussion_reply = @EmailOnDiscussionReply,
                updated_at = @UpdatedAt
            WHERE id = @Id";

        userSettings.UpdatedAt = DateTime.UtcNow;
        await _sqlExecutor.ExecuteAsync(sql, userSettings);
    }

    public async Task DeleteAsync(Guid id)
    {
        const string sql = "DELETE FROM user_settings WHERE id = @Id";
        await _sqlExecutor.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<bool> ExistsAsync(Guid userId)
    {
        const string sql = "SELECT COUNT(1) FROM user_settings WHERE user_id = @UserId";
        var count = await _sqlExecutor.QuerySingleAsync<int>(sql, new { UserId = userId });
        return count > 0;
    }

    public async Task<UserSettings> GetOrCreateDefaultAsync(Guid userId)
    {
        var existing = await GetByUserIdAsync(userId);
        if (existing != null)
            return existing;

        var defaultSettings = new UserSettings
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PublicSubmissions = false,
            ShowProfile = true,
            EmailOnCodeReview = true,
            EmailOnDiscussionReply = true,
            CreatedAt = DateTime.UtcNow
        };

        await CreateAsync(defaultSettings);
        return defaultSettings;
    }
}