using Microsoft.AspNetCore.Http;

namespace ElearningPlatform.Core.Exceptions;

public class BadRequestException(string? message = "The request is invalid.")
    : ApiException(message, StatusCodes.Status400BadRequest);