using EnterpriseMediator.Core.SharedKernel.Middleware;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace EnterpriseMediator.Core.SharedKernel.Extensions;

/// <summary>
/// Extension methods for WebApplication to wire up shared middleware during the pipeline configuration phase.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Adds the shared kernel middleware stack: global exception handler and Serilog request logging.
    /// Call early in the pipeline, before routing.
    /// </summary>
    public static WebApplication UseSharedKernelMiddleware(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionHandler>();
        app.UseSerilogRequestLogging();
        return app;
    }
}
