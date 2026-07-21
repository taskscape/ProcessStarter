using System;
using System.ServiceProcess;
using System.Runtime.Versioning;
using Serilog;
using Serilog.Events;

namespace ProcessStarter
{
    static class Program
    {
        // Serilog is configured before the Windows Service is constructed so all lifecycle events use the replacement logger.
        public static ILogger Log { get; private set; } = Serilog.Log.Logger;

        // The service and its Event Log sink are Windows-only; this declaration records that deployment requirement for .NET analyzers.
        [SupportedOSPlatform("windows")]
        static void Main()
        {
            ILogger logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .WriteTo.File(
                    path: @"C:\Temp\ProcessStarterService.log",
                    rollingInterval: RollingInterval.Infinite,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 10,
                    fileSizeLimitBytes: 100 * 1024 * 1024,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{ThreadId}] {Level:u3} {SourceContext} {Message:lj}{NewLine}{Exception}")
                .WriteTo.EventLog(
                    source: "ProcessStarter",
                    manageEventSource: true,
                    restrictedToMinimumLevel: LogEventLevel.Information)
                .CreateLogger();

            // The global logger shares the sinks with integrations, while the scoped logger preserves the original Program source context.
            Serilog.Log.Logger = logger;
            Log = logger.ForContext(typeof(Program));

            try
            {
                Log.Debug("Serilog logging has been initialized");
                ProcessStarter service = new ProcessStarter { CanShutdown = true };
#if DEBUG
                Win32.AllocConsole();
                service.Initialise();
#else
                ServiceBase.Run(new ServiceBase[] { service });
#endif
            }
            catch (Exception ex)
            {
                // Fatal logging preserves unhandled startup failures now that log4net is no longer present.
                Log.Fatal(ex, "The service terminated unexpectedly");
                throw;
            }
            finally
            {
                // Flush buffered Serilog events before the process exits or the Service Control Manager releases it.
                Serilog.Log.CloseAndFlush();
            }
        }
    }
}
