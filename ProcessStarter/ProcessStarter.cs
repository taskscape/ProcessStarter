using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace ProcessStarter
{
    public partial class ProcessStarter : ServiceBase
    {
        private const string UserName = "conv";
        private const string Password = "";

        public ProcessStarter()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Program.Log.Info("Service has started");
            Initialise();
        }

        protected override void OnStop()
        {
            Program.Log.Info("Service has stopped");
        }

        public bool Initialise()
        {

            string command = ConfigHelper.CommandName;

            IntPtr userToken = GetUserToken(UserName, Environment.MachineName, Password);
            if (userToken == IntPtr.Zero) return false;

            Process process = CreateProcessAsUser(userToken, null, command);
            if (process == null) return false;

            if (!ReleaseToken(userToken)) return false;

            process.WaitForExit();

            return true;
        }

        private static IntPtr GetUserToken(string username, string domain, string password)
        {
            IntPtr token = IntPtr.Zero;

            if (Win32.LogonUser(username, domain, password, (UInt32)Win32.Logon32Type.Interactive, (UInt32)Win32.Logon32Provider.Default, out token) == 0)
            {
                Program.Log.Error($"LogonUser failed with error code: {Marshal.GetLastWin32Error()}");
                return IntPtr.Zero;
            }

            return token;
        }

        private static bool ReleaseToken(IntPtr token)
        {
            if (Win32.CloseHandle(token) == 0)
            {
                Program.Log.Error($"CloseHandle failed with error code: {Marshal.GetLastWin32Error()}");
                return false;
            }
            return true;
        }

        private static Process CreateProcessAsUser(IntPtr token, string appName, string appArgs)
        {
            Win32.SecurityAttributes sa = new Win32.SecurityAttributes();
            sa.nLength = (UInt32)Marshal.SizeOf(sa);

            Win32.Startupinfo si = new Win32.Startupinfo();
            si.cb = (UInt32)Marshal.SizeOf(si);

            return CreateProcessAsUser(token, appName, appArgs, sa, sa, false, 0, IntPtr.Zero, null, si);
        }

        private static Process CreateProcessAsUser(IntPtr token, string appName, string appArgs,
                                                   Win32.SecurityAttributes processAttributes, Win32.SecurityAttributes threadAttributes,
                                                   bool inheritHandles, UInt32 creationFlags, IntPtr environment, string currentDir,
                                                   Win32.Startupinfo startupInfo)
        {
            Win32.ProcessInformation pi = new Win32.ProcessInformation();
            
            if (Win32.CreateProcessAsUser(token, appName, appArgs, ref processAttributes, ref threadAttributes,
               inheritHandles ? 1 : 0, creationFlags, environment, currentDir, ref startupInfo, out pi) == 0)
            {
                Program.Log.Error($"CreateProcessAsUser failed with error code: {Marshal.GetLastWin32Error()}");
                return null;
            }

            try
            {
                return Process.GetProcessById((Int32)pi.dwProcessId);
            }
            catch(ArgumentException ex)
            {
                Program.Log.Error($"Process with id {pi.dwProcessId} does not exist. {ex.Message}");
                return null;
            }
        }
    }
}