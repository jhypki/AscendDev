using Microsoft.AspNetCore.Http;

namespace AscendDev.Core.Exceptions;

public class BadRequestException(string? message = "The request is invalid.")
    : ApiException(message, StatusCodes.Status400BadRequest);