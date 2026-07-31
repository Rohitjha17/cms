namespace Cms.Shared.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    public int StatusCode { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message ?? "Success",
        StatusCode = 200
    };

    public static ApiResponse<T> Created(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message ?? "Created",
        StatusCode = 201
    };

    public static ApiResponse<T> Fail(string message, int statusCode = 400, IEnumerable<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        StatusCode = statusCode,
        Errors = errors
    };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string? message = null) => new()
    {
        Success = true,
        Message = message ?? "Success",
        StatusCode = 200
    };

    public new static ApiResponse Fail(string message, int statusCode = 400, IEnumerable<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        StatusCode = statusCode,
        Errors = errors
    };
}
