using Newtonsoft.Json;

namespace AscendDev.Core.DTOs;

public class ApiResponse<T>(bool success, T? data, string message, List<string>? errors)
{
    public bool Success { get; set; } = success;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public T? Data { get; set; } = data;

    public string Message { get; set; } = message;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? Errors { get; set; } = errors;

    public static ApiResponse<object?> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ErrorApiResponse(errors, message);
    }
}