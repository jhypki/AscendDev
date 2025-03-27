namespace ElearningPlatform.Core.DTOs;

public class SuccessApiResponse<T>(T data, string message = "Operation completed successfully")
    : ApiResponse<T>(true, data, message, null);