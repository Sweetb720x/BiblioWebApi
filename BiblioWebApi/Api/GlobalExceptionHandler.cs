using BiblioWebApi.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BiblioWebApi.Api;

public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception while processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        httpContext.Response.StatusCode = exception switch
        {
            RequestValidationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException { Message: "EMAIL_EXISTS" or "BOOK_NOT_AVAILABLE" or "DUPLICATE_RESERVATION" or "LOAN_ALREADY_RETURNED" } => StatusCodes.Status409Conflict,
            InvalidOperationException { Message: "USER_NOT_ACTIVE" or "USER_BLOCKED" or "LOAN_LIMIT_REACHED" or "RESERVATION_LIMIT_REACHED" or "RENEWAL_NOT_ALLOWED" } => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };
        var details = new ProblemDetails
        {
            Status = httpContext.Response.StatusCode,
            Title = exception switch
            {
                RequestValidationException => "Validation error",
                UnauthorizedAccessException => "Unauthorized",
                KeyNotFoundException => "Resource not found",
                InvalidOperationException { Message: "EMAIL_EXISTS" or "BOOK_NOT_AVAILABLE" or "DUPLICATE_RESERVATION" or "LOAN_ALREADY_RETURNED" } => "Conflict",
                InvalidOperationException { Message: "USER_NOT_ACTIVE" or "USER_BLOCKED" or "LOAN_LIMIT_REACHED" or "RESERVATION_LIMIT_REACHED" or "RENEWAL_NOT_ALLOWED" } => "Business rule violation",
                _ => "Unexpected error"
            },
            Detail = exception switch
            {
                RequestValidationException validation => string.Join("; ", validation.Errors),
                InvalidOperationException business => business.Message,
                _ when httpContext.Response.StatusCode < 500 => exception.Message,
                _ => "An unexpected error occurred."
            }
        };
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = details });
    }
}

