using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ElearningPlatform.Core.Exceptions;

public class ValidationException : ApiException
{
    public ValidationException(IDictionary<string, string[]> errors)
        : base("Validation failed.", StatusCodes.Status400BadRequest)
    {
        Errors = errors;
    }

    public ValidationException(ModelStateDictionary modelState)
        : base("Validation failed.", StatusCodes.Status400BadRequest)
    {
        Errors = modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );
    }

    public IDictionary<string, string[]> Errors { get; }
}