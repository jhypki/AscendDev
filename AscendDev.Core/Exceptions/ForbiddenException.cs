using Microsoft.AspNetCore.Http;

namespace AscendDev.Core.Exceptions;

public class ForbiddenException(string? message = "You do not have permission to access this resource.")
    : ApiException(message, StatusCodes.Status403Forbidden);