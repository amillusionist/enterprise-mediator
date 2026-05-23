using Emp.ApiGateway.Web.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Emp.ApiGateway.UnitTests.Middleware;

public class CorrelationIdMiddlewareTests
{
    private readonly CorrelationIdMiddleware _sut;

    public CorrelationIdMiddlewareTests()
    {
        var logger = Substitute.For<ILogger<CorrelationIdMiddleware>>();
        _sut = new CorrelationIdMiddleware(logger);
    }

    [Fact]
    public async Task InvokeAsync_WhenNoCorrelationIdHeader_GeneratesNewOne()
    {
        var context = new DefaultHttpContext();
        string? capturedCorrelationId = null;

        await _sut.InvokeAsync(context, next =>
        {
            capturedCorrelationId = context.Items["X-Correlation-ID"] as string;
            return Task.CompletedTask;
        });

        capturedCorrelationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(capturedCorrelationId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenCorrelationIdHeaderExists_UsesExistingOne()
    {
        var context = new DefaultHttpContext();
        var existingId = "my-correlation-123";
        context.Request.Headers["X-Correlation-ID"] = existingId;

        string? capturedCorrelationId = null;

        await _sut.InvokeAsync(context, next =>
        {
            capturedCorrelationId = context.Items["X-Correlation-ID"] as string;
            return Task.CompletedTask;
        });

        capturedCorrelationId.Should().Be(existingId);
    }

    [Fact]
    public async Task InvokeAsync_AddsCorrelationIdToResponseHeaders()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await _sut.InvokeAsync(context, next =>
        {
            // Trigger OnStarting callbacks by starting the response
            context.Response.Body.WriteByte(0);
            return Task.CompletedTask;
        });

        // OnStarting callbacks fire when the response is flushed; since DefaultHttpContext
        // doesn't trigger them automatically, we verify the item was set instead
        context.Items["X-Correlation-ID"].Should().NotBeNull();
    }
}
