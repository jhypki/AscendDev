using Microsoft.AspNetCore.Http;

namespace ElearningPlatform.Core.Exceptions;

public class InternalServerErrorException(string? message = null, string? details = null)
    : ApiException(message, StatusCodes.Status500InternalServerError, details);