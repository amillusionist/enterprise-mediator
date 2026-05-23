using System.Text.Json;
using Emp.ApiGateway.Web.Middleware;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Emp.ApiGateway.UnitTests.Middleware;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _sut;

    public GlobalExceptionHandlerTests()
    {
        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        _sut = new GlobalExceptionHandler(logger);
    }

    [Fact]
    public async Task TryHandleAsync_ArgumentException_Returns400()
    {
        var context = CreateHttpContext();
        var exception = new ArgumentException("Invalid argument");

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task TryHandleAsync_KeyNotFoundException_Returns404()
    {
        var context = CreateHttpContext();
        var exception = new KeyNotFoundException("Not found");

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task TryHandleAsync_UnauthorizedAccessException_Returns401()
    {
        var context = CreateHttpContext();
        var exception = new UnauthorizedAccessException();

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_Returns400WithErrors()
    {
        var context = CreateHttpContext();
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("ClientId", "ClientId is required")
        };
        var exception = new ValidationException(failures);

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var bodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        bodyText.Should().Contain("Validation Failed");
        bodyText.Should().Contain("Name");
        bodyText.Should().Contain("Name is required");
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_Returns500()
    {
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Something broke");

        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }
}
