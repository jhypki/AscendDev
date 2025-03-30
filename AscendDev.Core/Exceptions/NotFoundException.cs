using Microsoft.AspNetCore.Http;

namespace AscendDev.Core.Exceptions;

public class NotFoundException(string resource, string? id = null)
    : ApiException(
        id == null ? $"{resource} not found." : $"{resource} with ID '{id}' not found.",
        StatusCodes.Status404NotFound);