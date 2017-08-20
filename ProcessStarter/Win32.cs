using System;
using System.Runtime.InteropServices;

namespace ProcessStarter
{
    internal static class Win32
    {
        #region "STRUCTS"

        [StructLayout(LayoutKind.Sequential)]
        public struct Startupinfo
        {
            public UInt32 cb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public String lpReserved;
            [MarshalAs(UnmanagedType.LPTStr)]
            public String lpDesktop;
            [MarshalAs(UnmanagedType.LPTStr)]
            public String lpTitle;
            public UInt32 dwX;
            public UInt32 dwY;
            public UInt32 dwXSize;
            public UInt32 dwYSize;
            public UInt32 dwXCountChars;
            public UInt32 dwYCountChars;
            public UInt32 dwFillAttribute;
            public UInt32 dwFlags;
            public UInt16 wShowWindow;
            public UInt16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ProcessInformation
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public UInt32 dwProcessId;
            public UInt32 dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SecurityAttributes
        {
            public UInt32 nLength;
            public IntPtr lpSecurityDescriptor;
            public Int32 bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ProfileInfo
        {
            public UInt32 dwSize;
            public UInt32 dwFlags;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpUserName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpProfilePath;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDefaultPath;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpServerName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpPolicyPath;
            public IntPtr hProfile;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct UserInfo
        {
            public string usri4_name;
            public string usri4_password;
            public UInt32 usri4_password_age;
            public UInt32 usri4_priv;
            public string usri4_home_dir;
            public string usri4_comment;
            public UInt32 usri4_flags;
            public string usri4_script_path;
            public UInt32 usri4_auth_flags;
            public string usri4_full_name;
            public string usri4_usr_comment;
            public string usri4_parms;
            public string usri4_workstations;
            public UInt32 usri4_last_logon;
            public UInt32 usri4_last_logoff;
            public UInt32 usri4_acct_expires;
            public UInt32 usri4_max_storage;
            public UInt32 usri4_units_per_week;
            public IntPtr usri4_logon_hours;
            public UInt32 usri4_bad_pw_count;
            public UInt32 usri4_num_logons;
            public string usri4_logon_server;
            public UInt32 usri4_country_code;
            public UInt32 usri4_code_page;
            public IntPtr usri4_user_sid;
            public UInt32 usri4_primary_group_id;
            public string usri4_profile;
            public string usri4_home_dir_drive;
            public UInt32 usri4_password_expired;
        }

        #endregion

        #region "ENUMS"

        public enum Logon32Type
        {
            Interactive = 2,
            Network = 3,
            Batch = 4,
            Service = 5,
            Unlock = 7,
            NetworkClearText = 8,
            NewCredentials = 9
        }

        public enum Logon32Provider
        {
            Default = 0,
            WinNT40 = 2,
            WinNT50 = 3
        }

        #endregion

        #region "FUNCTIONS (P/INVOKE)"

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public extern static int LogonUser(string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            UInt32 dwLogonType,
            UInt32 dwLogonProvider,
            out IntPtr phToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public extern static int CreateProcessAsUser(IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            ref SecurityAttributes lpProcessAttributes,
            ref SecurityAttributes lpThreadAttributes,
            Int32 bInheritHandles,
            UInt32 dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref Startupinfo lpStartupInfo,
            out ProcessInformation lpProcessInformation);

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
        public extern static uint NetUserGetInfo(string servername, string username, UInt32 level, out IntPtr bufptr);

        [DllImport("Userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public extern static int LoadUserProfile(IntPtr hToken, ref ProfileInfo lpProfileInfo);

        [DllImport("Userenv.dll", SetLastError = true)]
        public extern static int UnloadUserProfile(IntPtr hToken, IntPtr hProfile);

        [DllImport("Userenv.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public extern static int CreateEnvironmentBlock(out IntPtr lpEnvironment, IntPtr hToken, int bInherit);

        [DllImport("kernel32.dll", SetLastError = true)]
        public extern static int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public extern static int AllocConsole();

        #endregion
    }
}
