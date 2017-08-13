using System.ServiceProcess;
using log4net;

namespace ProcessStarter
{
    static class Program
    {
        static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();
            Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            LogManager.GetLogger("Startup").Debug("This logging call loads the configuration");

            ProcessStarter service = new ProcessStarter { CanShutdown = true };
#if DEBUG
            Win32.AllocConsole();
            service.Initialise();
#else
            ServiceBase.Run(new ServiceBase[] { service });
#endif
        }

        public static ILog Log;
    }



}