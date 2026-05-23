using System;
using EnterpriseMediator.Core.SharedKernel.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace EnterpriseMediator.Core.SharedKernel.Extensions
{
    /// <summary>
    /// Extension methods for configuring Serilog across the enterprise services.
    /// Ensures consistent logging patterns, enrichment, and sinks.
    /// </summary>
    public static class LoggerConfigurationExtensions
    {
        /// <summary>
        /// Applies shared enterprise logging configuration: enrichers, sinks, and minimum level.
        /// Called from <see cref="WebApplicationBuilderExtensions.ConfigureSharedKernelLogging"/>
        /// and can also be used standalone with a <see cref="LoggerConfiguration"/>.
        /// </summary>
        public static LoggerConfiguration ConfigureBaseLogging(
            this LoggerConfiguration loggerConfiguration,
            string applicationName,
            string environmentName,
            SerilogOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            // Parse minimum level
            if (Enum.TryParse<LogEventLevel>(options.MinimumLevel, ignoreCase: true, out var minLevel))
            {
                loggerConfiguration.MinimumLevel.Is(minLevel);
            }

            // Enrichers
            loggerConfiguration
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("ApplicationName", applicationName)
                .Enrich.WithProperty("Environment", environmentName);

            // Console sink
            if (options.UseConsole)
            {
                loggerConfiguration.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            }

            // Rolling file sink
            if (options.UseFile && !string.IsNullOrWhiteSpace(options.LogFilePath))
            {
                loggerConfiguration.WriteTo.File(
                    path: options.LogFilePath,
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Warning,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
            }

            // Seq sink
            if (options.UseSeq && !string.IsNullOrWhiteSpace(options.SeqUrl))
            {
                loggerConfiguration.WriteTo.Seq(options.SeqUrl);
            }

            return loggerConfiguration;
        }
    }
}