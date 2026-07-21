# ProcessStarter

The solution targets .NET 10 for Windows. `ProcessStarter` remains a Windows Service using `ServiceBase`; the legacy `ProjectInstaller` classes were removed because `System.Configuration.Install` is not available on modern .NET.

Install the published service with `sc.exe create ProcessStarter binPath= "<published ProcessStarter.exe path>" start= delayed-auto`, then configure its display name and description as required by your environment.

The service now uses Serilog instead of log4net. It writes debug-and-higher events to `C:\Temp\ProcessStarterService.log`, rolls at 100 MB, keeps ten rollover files, and writes information-and-higher events to the Windows Event Log with source `ProcessStarter`. The service account must be allowed to create that Event Log source on its first run, or it should be pre-created during deployment.

`ProcessStarter/App.config` still provides the `CommandName` setting read by `ConfigurationManager`; its default now points to the .NET 10 Debug output, so change it to the deployed ConsoleApp executable path before installing the service.
