using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Auth;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace AscendDev.Data.Repositories;

public class UserRepository(ISqlExecutor sql, ILogger<UserRepository> logger)
    : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id)
    {
        const string query = """
                                         SELECT id, email, password_hash, username, first_name, last_name, is_email_verified,
                                                created_at, updated_at, last_login, profile_picture_url, bio, external_id, provider,
                                                is_active, is_locked, locked_until, failed_login_attempts, last_failed_login,
                                                email_verification_token, email_verification_token_expires, password_reset_token,
                                                password_reset_token_expires, time_zone, language, email_notifications, push_notifications
                                         FROM users
                                         WHERE id = @Id
                             """;
        try
        {
            return await sql.QueryFirstOrDefaultAsync<User>(query, new { Id = id });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting user by id");
            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string query = """
                                         SELECT id, email, password_hash, username, first_name, last_name, is_email_verified,
                                                created_at, updated_at, last_login, profile_picture_url, bio, external_id, provider,
                                                is_active, is_locked, locked_until, failed_login_attempts, last_failed_login,
                                                email_verification_token, email_verification_token_expires, password_reset_token,
                                                password_reset_token_expires, time_zone, language, email_notifications, push_notifications
                                         FROM users
                                         WHERE email = @Email
                             """;
        try
        {
            return await sql.QueryFirstOrDefaultAsync<User>(query, new { Email = email });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting user by email");
            throw;
        }
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        const string query = """
                                         SELECT id, email, password_hash, username, first_name, last_name, is_email_verified,
                                                created_at, updated_at, last_login, profile_picture_url, bio, external_id, provider,
                                                is_active, is_locked, locked_until, failed_login_attempts, last_failed_login,
                                                email_verification_token, email_verification_token_expires, password_reset_token,
                                                password_reset_token_expires, time_zone, language, email_notifications, push_notifications
                                         FROM users
                                         WHERE username = @Username
                             """;
        try
        {
            return await sql.QueryFirstOrDefaultAsync<User>(query, new { Username = username });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting user by username");
            throw;
        }
    }

    public async Task<User?> GetByExternalIdAsync(string externalId, string provider)
    {
        const string query = """
                                         SELECT id, email, password_hash, username, first_name, last_name, is_email_verified,
                                                created_at, updated_at, last_login, profile_picture_url, bio, external_id, provider,
                                                is_active, is_locked, locked_until, failed_login_attempts, last_failed_login,
                                                email_verification_token, email_verification_token_expires, password_reset_token,
                                                password_reset_token_expires, time_zone, language, email_notifications, push_notifications
                                         FROM users
                                         WHERE external_id = @ExternalId AND provider = @Provider
                             """;
        try
        {
            return await sql.QueryFirstOrDefaultAsync<User>(query, new { ExternalId = externalId, Provider = provider });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting user by external ID and provider");
            throw;
        }
    }

    public async Task<bool> CreateAsync(User user)
    {
        const string query = """
                                         INSERT INTO users (id, email, password_hash, username, first_name, last_name, is_email_verified,
                                                           created_at, updated_at, last_login, profile_picture_url, bio, external_id, provider,
                                                           is_active, is_locked, locked_until, failed_login_attempts, last_failed_login,
                                                           email_verification_token, email_verification_token_expires, password_reset_token,
                                                           password_reset_token_expires, time_zone, language, email_notifications, push_notifications)
                                         VALUES (@Id, @Email, @PasswordHash, @Username, @FirstName, @LastName, @IsEmailVerified,
                                                 @CreatedAt, @UpdatedAt, @LastLogin, @ProfilePictureUrl, @Bio, @ExternalId, @Provider,
                                                 @IsActive, @IsLocked, @LockedUntil, @FailedLoginAttempts, @LastFailedLogin,
                                                 @EmailVerificationToken, @EmailVerificationTokenExpires, @PasswordResetToken,
                                                 @PasswordResetTokenExpires, @TimeZone, @Language, @EmailNotifications, @PushNotifications)
                             """;
        try
        {
            await sql.ExecuteAsync(query, user);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating user");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(User user)
    {
        const string query = """
                                         UPDATE users
                                         SET email = @Email, password_hash = @PasswordHash, username = @Username,
                                             first_name = @FirstName, last_name = @LastName, is_email_verified = @IsEmailVerified,
                                             created_at = @CreatedAt, updated_at = @UpdatedAt, last_login = @LastLogin,
                                             profile_picture_url = @ProfilePictureUrl, bio = @Bio, external_id = @ExternalId, provider = @Provider,
                                             is_active = @IsActive, is_locked = @IsLocked, locked_until = @LockedUntil,
                                             failed_login_attempts = @FailedLoginAttempts, last_failed_login = @LastFailedLogin,
                                             email_verification_token = @EmailVerificationToken, email_verification_token_expires = @EmailVerificationTokenExpires,
                                             password_reset_token = @PasswordResetToken, password_reset_token_expires = @PasswordResetTokenExpires,
                                             time_zone = @TimeZone, language = @Language, email_notifications = @EmailNotifications, push_notifications = @PushNotifications
                                         WHERE id = @Id
                             """;
        try
        {
            await sql.ExecuteAsync(query, user);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating user");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        const string query = """
                                         DELETE FROM users
                                         WHERE id = @Id
                             """;
        try
        {
            await sql.ExecuteAsync(query, new { Id = id });
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting user");
            return false;
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        const string query = "SELECT COUNT(1) FROM users WHERE id = @Id";
        try
        {
            var count = await sql.QueryFirstOrDefaultAsync<int>(query, new { Id = id });
            return count > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error checking if user exists");
            throw;
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        const string query = "SELECT COUNT(1) FROM users WHERE email = @Email";
        try
        {
            var count = await sql.QueryFirstOrDefaultAsync<int>(query, new { Email = email });
            return count > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error checking if email exists");
            throw;
        }
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        const string query = "SELECT COUNT(1) FROM users WHERE username = @Username";
        try
        {
            var count = await sql.QueryFirstOrDefaultAsync<int>(query, new { Username = username });
            return count > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error checking if username exists");
            throw;
        }
    }

    // Extended methods for analytics
    public async Task<int> CountAsync()
    {
        const string query = "SELECT COUNT(*) FROM users";
        try
        {
            return await sql.QueryFirstAsync<int>(query);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error counting users");
            throw;
        }
    }

    public async Task<int> CountAsync(Expression<Func<User, bool>> predicate)
    {
        // For now, implement basic cases that we need for admin stats
        // This is a simplified implementation - in a real scenario you'd use a proper ORM

        // Check if it's the IsActive predicate
        if (predicate.Body is MemberExpression memberExpr && memberExpr.Member.Name == "IsActive")
        {
            const string query = "SELECT COUNT(*) FROM users WHERE is_active = true";
            try
            {
                return await sql.QueryFirstAsync<int>(query);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error counting active users");
                throw;
            }
        }

        // Check if it's a date comparison for CreatedAt
        if (predicate.Body is BinaryExpression binaryExpr &&
            binaryExpr.Left is MemberExpression leftMember &&
            leftMember.Member.Name == "CreatedAt")
        {
            // This is a simplified approach - extract the date value
            var compiled = predicate.Compile();
            var testUser = new User { CreatedAt = DateTime.UtcNow };

            if (binaryExpr.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                // Get the date from the expression
                var constantExpr = binaryExpr.Right as ConstantExpression;
                if (constantExpr?.Value is DateTime date)
                {
                    const string query = "SELECT COUNT(*) FROM users WHERE created_at >= @Date";
                    try
                    {
                        return await sql.QueryFirstAsync<int>(query, new { Date = date });
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error counting users by date");
                        throw;
                    }
                }
            }
        }

        // Fallback - get all users and filter in memory (not efficient for large datasets)
        logger.LogWarning("Using inefficient in-memory filtering for user count");
        var allUsers = await GetAllAsync();
        return allUsers?.Where(predicate.Compile()).Count() ?? 0;
    }

    public async Task<List<User>> GetAllAsync()
    {
        const string query = """
                                     SELECT id, email, password_hash, username, first_name, last_name, is_email_verified,
                                            created_at, updated_at, last_login, profile_picture_url, bio, external_id, provider,
                                            is_active, is_locked, locked_until, failed_login_attempts, last_failed_login,
                                            email_verification_token, email_verification_token_expires, password_reset_token,
                                            password_reset_token_expires, time_zone, language, email_notifications, push_notifications
                                     FROM users
                             """;
        try
        {
            var result = await sql.QueryAsync<User>(query);
            return result.ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting all users");
            throw;
        }
    }

    public async Task<List<User>> GetAllAsync(Expression<Func<User, bool>> predicate)
    {
        var allUsers = await GetAllAsync();
        return allUsers.Where(predicate.Compile()).ToList();
    }

    public IQueryable<User> GetQueryable()
    {
        // This is a simplified implementation
        // In a real scenario with EF Core, you'd return the DbSet as IQueryable
        // For now, we'll return an empty queryable and handle queries in the service layer
        return new List<User>().AsQueryable();
    }
}