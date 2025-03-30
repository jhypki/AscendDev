using Microsoft.AspNetCore.Http;

namespace AscendDev.Core.Exceptions;

public class InternalServerErrorException(string? message = null, string? details = null)
    : ApiException(message, StatusCodes.Status500InternalServerError, details);