using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Emp.ApiGateway.Web.Middleware
{
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
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            if (exception is ValidationException validationEx)
            {
                var validationProblem = new ValidationProblemDetails(
                    validationEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()))
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Failed",
                    Instance = httpContext.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                };

                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(validationProblem, cancellationToken);
                return true;
            }

            var problemDetails = CreateProblemDetails(httpContext, exception, traceId);
            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception, string traceId)
        {
            return exception switch
            {
                ArgumentException argEx => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad Request",
                    Detail = argEx.Message,
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                },
                KeyNotFoundException notFoundEx => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource Not Found",
                    Detail = notFoundEx.Message,
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                },
                UnauthorizedAccessException => new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = "Authentication is required to access this resource.",
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                },
                _ => new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An error occurred while processing your request",
                    Detail = "Internal Server Error",
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                }
            };
        }
    }
}
