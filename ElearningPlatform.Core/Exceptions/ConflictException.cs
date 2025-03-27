using Microsoft.AspNetCore.Http;

namespace ElearningPlatform.Core.Exceptions;

public class ConflictException(string? message = "A conflict occurred with the current state of the resource.")
    : ApiException(message, StatusCodes.Status409Conflict);