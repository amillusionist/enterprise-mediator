using System.Net;
using System.Text.Json;
using EnterpriseMediator.Core.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.Core.SharedKernel.Middleware;

/// <summary>
/// Catches unhandled exceptions and returns RFC 7807 ProblemDetails responses.
/// Maps <see cref="CustomException"/> subtypes to appropriate HTTP status codes.
/// </summary>
public sealed class GlobalExceptionHandler : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ProblemDetails
                {
                    Title = "Validation Failed",
                    Detail = validationEx.Message,
                    Status = (int)HttpStatusCode.BadRequest,
                    Extensions = { ["errors"] = validationEx.Errors }
                }),

            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Title = "Resource Not Found",
                    Detail = notFoundEx.Message,
                    Status = (int)HttpStatusCode.NotFound
                }),

            BusinessRuleException businessEx => (
                HttpStatusCode.UnprocessableEntity,
                new ProblemDetails
                {
                    Title = "Business Rule Violation",
                    Detail = businessEx.Message,
                    Status = (int)HttpStatusCode.UnprocessableEntity,
                    Extensions = { ["ruleName"] = businessEx.RuleName }
                }),

            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                new ProblemDetails
                {
                    Title = "Conflict",
                    Detail = conflictEx.Message,
                    Status = (int)HttpStatusCode.Conflict
                }),

            ForbiddenAccessException forbiddenEx => (
                HttpStatusCode.Forbidden,
                new ProblemDetails
                {
                    Title = "Forbidden",
                    Detail = forbiddenEx.Message,
                    Status = (int)HttpStatusCode.Forbidden
                }),

            CustomException customEx => (
                customEx.StatusCode,
                new ProblemDetails
                {
                    Title = "Application Error",
                    Detail = customEx.Message,
                    Status = (int)customEx.StatusCode
                }),

            _ => (
                HttpStatusCode.InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred.",
                    Status = (int)HttpStatusCode.InternalServerError
                })
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception ({StatusCode}) for {Method} {Path}",
                (int)statusCode, context.Request.Method, context.Request.Path);
        }

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problemDetails, JsonOptions));
    }
}
