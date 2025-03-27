using ElearningPlatform.Core.Entities.Auth;
using ElearningPlatform.Core.Interfaces.Data;
using Microsoft.Extensions.Logging;

namespace ElearningPlatform.Data.Repositories;

public class UserRepository(ISqlExecutor sql, ILogger<UserRepository> logger)
    : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id)
    {
        const string query = """
                                         SELECT id, email, password_hash, username, first_name, last_name, is_email_verified,
                                                created_at, updated_at, last_login, profile_picture_url, bio
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
                                                created_at, updated_at, last_login, profile_picture_url, bio
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
                                                created_at, updated_at, last_login, profile_picture_url, bio
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

    public async Task<bool> CreateAsync(User user)
    {
        const string query = """
                                         INSERT INTO users (id, email, password_hash, username, first_name, last_name, is_email_verified,
                                                           created_at, updated_at, last_login, profile_picture_url, bio)
                                         VALUES (@Id, @Email, @PasswordHash, @Username, @FirstName, @LastName, @IsEmailVerified,
                                                 @CreatedAt, @UpdatedAt, @LastLogin, @ProfilePictureUrl, @Bio)
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
                                             profile_picture_url = @ProfilePictureUrl, bio = @Bio
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
}