namespace ElearningPlatform.Core.Exceptions;

public class ApiException(string? message, int statusCode, string? details = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string? Details { get; } = details;
}