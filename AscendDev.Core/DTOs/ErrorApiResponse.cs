namespace AscendDev.Core.DTOs;

public class ErrorApiResponse(List<string>? errors, string message) : ApiResponse<object>(false, null, message, errors);