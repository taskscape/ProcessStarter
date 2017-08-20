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

            IntPtr userToken = GetUserToken(UserName, Environment.MachineName, Password);
            if (userToken == IntPtr.Zero) return false;

            IntPtr userRegToken = LoadUserProfile(userToken, UserName);
            if (userRegToken == IntPtr.Zero) return false;

            if (Win32.CreateEnvironmentBlock(out IntPtr envBlock, userToken, 0) == 0) return false;

            Win32.SecurityAttributes sa = new Win32.SecurityAttributes();
            sa.nLength = (UInt32) Marshal.SizeOf(sa);

            Win32.Startupinfo si = new Win32.Startupinfo();
            si.cb = (UInt32) Marshal.SizeOf(si);

            Process process = CreateProcessAsUser(userToken, null, ConfigHelper.CommandName, sa, sa, false, 0, IntPtr.Zero, null, si);
            if (process == null) return false;

            if (Win32.UnloadUserProfile(userToken, userRegToken) == 0) return false;
            if (Win32.CloseHandle(envBlock) == 0) return false;
            if (!ReleaseToken(userToken)) return false;

            process.WaitForExit();

            return true;
        }

        /// <exception cref="Exception">NetUserGetInfo failed.</exception>
        private static Win32.UserInfo GetUserInfo(string username)
        {
            IntPtr bufptr = IntPtr.Zero;
            uint code = Win32.NetUserGetInfo(Environment.MachineName, username, 4, out bufptr);
            if (code != 0)
            {
                Program.Log.Error($"NetUserGetInfo failed with error code: {code}. See: NET_API_STATUS");
                throw new Exception($"NetUserGetInfo failed with error code: {code}. See: NET_API_STATUS");
            }

            try
            {
                Win32.UserInfo userInfo = (Win32.UserInfo) Marshal.PtrToStructure(bufptr, typeof(Win32.UserInfo));
                return userInfo;
            }
            catch (Exception ex)
            {
                Program.Log.Error($"GetUserInfo: Could not convert data to UserInfo. Exception: {ex.Message}");
                throw;
            }
        }

        private static IntPtr GetUserToken(string username, string domain, string password)
        {
            IntPtr token = IntPtr.Zero;

            if (Win32.LogonUser(username, domain, password, (UInt32) Win32.Logon32Type.Interactive,
                    (UInt32) Win32.Logon32Provider.Default, out token) == 0)
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

        private static IntPtr LoadUserProfile(IntPtr token, string username)
        {
            Win32.UserInfo uInfo;
            try
            {
                uInfo = GetUserInfo(username);
            }
            catch (Exception ex)
            {
                Program.Log.Error(
                    $"LoadUserProfile: Could not get user info for user {username}. Exception: {ex.Message}");
                return IntPtr.Zero;
            }

            Win32.ProfileInfo pInfo = new Win32.ProfileInfo();
            pInfo.dwSize = (UInt32) Marshal.SizeOf(pInfo);
            pInfo.lpUserName = uInfo.usri4_name;
            pInfo.lpProfilePath = uInfo.usri4_profile;

            if (Win32.LoadUserProfile(token, ref pInfo) == 0)
            {
                Program.Log.Error($"LoadUserProfile failed with error code: {Marshal.GetLastWin32Error()}");
                return IntPtr.Zero;
            }

            return pInfo.hProfile;
        }

        private static Process CreateProcessAsUser(IntPtr token, string appName, string appArgs)
        {
            Win32.SecurityAttributes sa = new Win32.SecurityAttributes();
            sa.nLength = (UInt32) Marshal.SizeOf(sa);

            Win32.Startupinfo si = new Win32.Startupinfo();
            si.cb = (UInt32) Marshal.SizeOf(si);

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
                return Process.GetProcessById((Int32) pi.dwProcessId);
            }
            catch (ArgumentException ex)
            {
                Program.Log.Error($"Process with id {pi.dwProcessId} does not exist. {ex.Message}");
                return null;
            }
        }
    }
}