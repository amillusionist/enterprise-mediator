using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMediator.Financial.Web.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            ValidationException validationException => (
                (int)HttpStatusCode.BadRequest,
                "Validation Error",
                string.Join("; ", validationException.Errors.Select(e => e.ErrorMessage))),

            ArgumentException argException => (
                (int)HttpStatusCode.BadRequest,
                "Invalid Argument",
                argException.Message),

            InvalidOperationException opException => (
                (int)HttpStatusCode.Conflict,
                "Business Rule Violation",
                opException.Message),

            UnauthorizedAccessException => (
                (int)HttpStatusCode.Forbidden,
                "Forbidden",
                "You do not have permission to perform this action."),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred.")
        };

        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {Title}", title);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.com/{statusCode}",
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
