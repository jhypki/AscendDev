using Microsoft.AspNetCore.Http;

namespace ElearningPlatform.Core.Exceptions;

public class UnauthorizedException(string? message = "You are not authorized to perform this action.")
    : ApiException(message, StatusCodes.Status401Unauthorized);