using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Auth;
using Microsoft.Extensions.Logging;

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
}