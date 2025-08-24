using AscendDev.Core.DTOs.Admin;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Interfaces.Utils;
using AscendDev.Core.Models.Admin;
using AscendDev.Core.Models.Auth;
using AscendDev.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IUserManagementRepository _userManagementRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        IUserManagementRepository userManagementRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<UserManagementService> logger)
    {
        _userManagementRepository = userManagementRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<PagedResult<UserManagementDto>> SearchUsersAsync(UserSearchRequest request)
    {
        try
        {
            return await _userManagementRepository.SearchUsersAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with request: {@Request}", request);
            throw new InternalServerErrorException("An error occurred while searching users");
        }
    }

    public async Task<UserManagementDto?> GetUserWithDetailsAsync(Guid userId)
    {
        try
        {
            var user = await _userManagementRepository.GetUserWithDetailsAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
            return user;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user details for user {UserId}", userId);
            throw new InternalServerErrorException("An error occurred while retrieving user details");
        }
    }

    public async Task<UserManagementDto> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            // Validate email uniqueness
            if (await _userRepository.EmailExistsAsync(request.Email))
            {
                throw new ConflictException("Email already exists");
            }

            // Validate username uniqueness
            if (await _userRepository.UsernameExistsAsync(request.Username))
            {
                throw new ConflictException("Username already exists");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Username = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = !string.IsNullOrEmpty(request.Password) ? _passwordHasher.Hash(request.Password) : null,
                IsEmailVerified = true, // Admin-created users are pre-verified
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                Language = "en"
            };

            var success = await _userManagementRepository.CreateUserAsync(user, request.Roles);
            if (!success)
            {
                throw new InternalServerErrorException("Failed to create user");
            }

            var createdUser = await _userManagementRepository.GetUserWithDetailsAsync(user.Id);
            return createdUser ?? throw new InternalServerErrorException("Failed to retrieve created user");
        }
        catch (ConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email {Email}", request.Email);
            throw new InternalServerErrorException("An error occurred while creating user");
        }
    }

    public async Task<UserManagementDto> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        try
        {
            // Check if user exists
            var existingUser = await _userRepository.GetByIdAsync(userId);
            if (existingUser == null)
            {
                throw new NotFoundException("User not found");
            }

            // Validate email uniqueness if email is being changed
            if (!string.IsNullOrEmpty(request.Email) && request.Email != existingUser.Email)
            {
                if (await _userRepository.EmailExistsAsync(request.Email))
                {
                    throw new ConflictException("Email already exists");
                }
            }

            // Validate username uniqueness if username is being changed
            if (!string.IsNullOrEmpty(request.Username) && request.Username != existingUser.Username)
            {
                if (await _userRepository.UsernameExistsAsync(request.Username))
                {
                    throw new ConflictException("Username already exists");
                }
            }

            var success = await _userManagementRepository.UpdateUserAsync(userId, request);
            if (!success)
            {
                throw new InternalServerErrorException("Failed to update user");
            }

            var updatedUser = await _userManagementRepository.GetUserWithDetailsAsync(userId);
            return updatedUser ?? throw new InternalServerErrorException("Failed to retrieve updated user");
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (ConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            throw new InternalServerErrorException("An error occurred while updating user");
        }
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            return await _userManagementRepository.DeleteUserAsync(userId);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            throw new InternalServerErrorException("An error occurred while deleting user");
        }
    }

    public async Task<bool> ActivateUserAsync(Guid userId)
    {
        var request = new UpdateUserRequest { IsActive = true };
        var result = await UpdateUserAsync(userId, request);
        return result != null;
    }

    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        var request = new UpdateUserRequest { IsActive = false };
        var result = await UpdateUserAsync(userId, request);
        return result != null;
    }

    public async Task<bool> LockUserAsync(Guid userId, DateTime? lockUntil = null)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            user.IsLocked = true;
            user.LockedUntil = lockUntil;
            user.UpdatedAt = DateTime.UtcNow;

            return await _userRepository.UpdateAsync(user);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking user {UserId}", userId);
            throw new InternalServerErrorException("An error occurred while locking user");
        }
    }

    public async Task<bool> UnlockUserAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            user.IsLocked = false;
            user.LockedUntil = null;
            user.UpdatedAt = DateTime.UtcNow;

            return await _userRepository.UpdateAsync(user);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking user {UserId}", userId);
            throw new InternalServerErrorException("An error occurred while unlocking user");
        }
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        try
        {
            return await _userManagementRepository.GetAllRolesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all roles");
            throw new InternalServerErrorException("An error occurred while retrieving roles");
        }
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            return await _userManagementRepository.GetUserRolesAsync(userId);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
            throw new InternalServerErrorException("An error occurred while retrieving user roles");
        }
    }

    public async Task<bool> AssignRoleToUserAsync(Guid userId, string roleName)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            return await _userManagementRepository.AssignRoleToUserAsync(userId, roleName);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", roleName, userId);
            throw new InternalServerErrorException("An error occurred while assigning role");
        }
    }

    public async Task<bool> RemoveRoleFromUserAsync(Guid userId, string roleName)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            return await _userManagementRepository.RemoveRoleFromUserAsync(userId, roleName);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {RoleName} from user {UserId}", roleName, userId);
            throw new InternalServerErrorException("An error occurred while removing role");
        }
    }

    public async Task<bool> UpdateUserRolesAsync(Guid userId, List<string> roles)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            return await _userManagementRepository.UpdateUserRolesAsync(userId, roles);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating roles for user {UserId}", userId);
            throw new InternalServerErrorException("An error occurred while updating user roles");
        }
    }

    public async Task<BulkOperationResult> PerformBulkOperationAsync(BulkUserOperationRequest request, Guid performedBy)
    {
        try
        {
            var operation = new BulkOperation
            {
                Id = Guid.NewGuid(),
                OperationType = request.Operation,
                PerformedBy = performedBy,
                TargetCount = request.UserIds.Count,
                OperationData = request.Parameters,
                Status = "in_progress",
                StartedAt = DateTime.UtcNow
            };

            var operationId = await _userManagementRepository.CreateBulkOperationAsync(operation);

            var successCount = 0;
            var errors = new List<string>();

            foreach (var userId in request.UserIds)
            {
                try
                {
                    var success = request.Operation switch
                    {
                        "activate" => await ActivateUserAsync(userId),
                        "deactivate" => await DeactivateUserAsync(userId),
                        "lock" => await LockUserAsync(userId),
                        "unlock" => await UnlockUserAsync(userId),
                        "assign_role" => request.Parameters?.ContainsKey("roleName") == true
                            ? await AssignRoleToUserAsync(userId, request.Parameters["roleName"].ToString()!)
                            : false,
                        "remove_role" => request.Parameters?.ContainsKey("roleName") == true
                            ? await RemoveRoleFromUserAsync(userId, request.Parameters["roleName"].ToString()!)
                            : false,
                        _ => false
                    };

                    if (success) successCount++;
                    else errors.Add($"Failed to {request.Operation} user {userId}");
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing user {userId}: {ex.Message}");
                }
            }

            var finalStatus = errors.Count == 0 ? "completed" : "completed";
            await _userManagementRepository.UpdateBulkOperationAsync(operationId, finalStatus);

            return new BulkOperationResult
            {
                OperationId = operationId,
                OperationType = request.Operation,
                TargetCount = request.UserIds.Count,
                SuccessCount = successCount,
                FailureCount = request.UserIds.Count - successCount,
                Status = finalStatus,
                Errors = errors,
                StartedAt = operation.StartedAt,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk operation {Operation}", request.Operation);
            throw new InternalServerErrorException("An error occurred while performing bulk operation");
        }
    }

    public async Task<BulkOperationResult?> GetBulkOperationResultAsync(Guid operationId)
    {
        try
        {
            return await _userManagementRepository.GetBulkOperationResultAsync(operationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bulk operation result {OperationId}", operationId);
            throw new InternalServerErrorException("An error occurred while retrieving bulk operation result");
        }
    }

    public async Task<List<BulkOperationResult>> GetBulkOperationsAsync(Guid? performedBy = null, int limit = 50)
    {
        try
        {
            return await _userManagementRepository.GetBulkOperationsAsync(performedBy, limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bulk operations");
            throw new InternalServerErrorException("An error occurred while retrieving bulk operations");
        }
    }

    public async Task<bool> LogUserActivityAsync(Guid userId, string activityType, string? description = null,
        Dictionary<string, object>? metadata = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var activity = new UserActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ActivityType = activityType,
                ActivityDescription = description,
                Metadata = metadata,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow
            };

            return await _userManagementRepository.LogUserActivityAsync(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging user activity for user {UserId}", userId);
            // Don't throw exception for logging failures to avoid breaking main functionality
            return false;
        }
    }

    public async Task<List<UserActivityLogDto>> GetUserActivityAsync(Guid userId, int limit = 100)
    {
        try
        {
            return await _userManagementRepository.GetUserActivityAsync(userId, limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity for user {UserId}", userId);
            throw new InternalServerErrorException("An error occurred while retrieving user activity");
        }
    }

    public async Task<List<UserActivityLogDto>> GetRecentActivityAsync(int limit = 100)
    {
        try
        {
            return await _userManagementRepository.GetRecentActivityAsync(limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent activity");
            throw new InternalServerErrorException("An error occurred while retrieving recent activity");
        }
    }

    public async Task<UserStatisticsDto?> GetUserStatisticsAsync(Guid userId)
    {
        try
        {
            return await _userManagementRepository.GetUserStatisticsAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user statistics for user {UserId}", userId);
            throw new InternalServerErrorException("An error occurred while retrieving user statistics");
        }
    }

    public async Task<List<UserStatisticsDto>> GetTopUsersByPointsAsync(int limit = 10)
    {
        try
        {
            return await _userManagementRepository.GetTopUsersByPointsAsync(limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top users by points");
            throw new InternalServerErrorException("An error occurred while retrieving top users by points");
        }
    }

    public async Task<List<UserStatisticsDto>> GetTopUsersByStreakAsync(int limit = 10)
    {
        try
        {
            return await _userManagementRepository.GetTopUsersByStreakAsync(limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top users by streak");
            throw new InternalServerErrorException("An error occurred while retrieving top users by streak");
        }
    }
}