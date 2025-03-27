using Microsoft.AspNetCore.Http;

namespace ElearningPlatform.Core.Exceptions;

public class ForbiddenException(string? message = "You do not have permission to access this resource.")
    : ApiException(message, StatusCodes.Status403Forbidden);