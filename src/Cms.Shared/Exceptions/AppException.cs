namespace Cms.Shared.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404)
    {
    }
}

public class ValidationAppException : AppException
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationAppException(string message, IEnumerable<string>? errors = null)
        : base(message, 400)
    {
        Errors = errors?.ToList() ?? new List<string> { message };
    }
}

public class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message = "Unauthorized") : base(message, 401)
    {
    }
}

public class ForbiddenAppException : AppException
{
    public ForbiddenAppException(string message = "Forbidden") : base(message, 403)
    {
    }
}

public class TenantNotResolvedException : AppException
{
    public TenantNotResolvedException(string message = "Unable to resolve tenant for this request.")
        : base(message, 400)
    {
    }
}
