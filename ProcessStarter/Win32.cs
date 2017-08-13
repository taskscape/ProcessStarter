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
            [MarshalAs(UnmanagedType.LPStr)]
            public String lpReserved;
            [MarshalAs(UnmanagedType.LPStr)]
            public String lpDesktop;
            [MarshalAs(UnmanagedType.LPStr)]
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

        #endregion

        #region "ENUMS"

        public enum Logon32Type
        {
            Interactive         = 2,
            Network             = 3,
            Batch               = 4,
            Service             = 5,
            Unlock              = 7,
            NetworkClearText    = 8,
            NewCredentials      = 9
        }

        public enum Logon32Provider
        {
            Default = 0,
            WinNT40 = 2,
            WinNT50 = 3
        }

        #endregion

        #region "FUNCTIONS (P/INVOKE)"

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi, EntryPoint = "LogonUserA")]
        public extern static int LogonUser(string lpszUsername,
                                             string lpszDomain,
                                             string lpszPassword,
                                             UInt32 dwLogonType,
                                             UInt32 dwLogonProvider,
                                             out IntPtr phToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi, EntryPoint = "CreateProcessAsUserA")]
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

        [DllImport("kernel32.dll", SetLastError = true)]
        public extern static int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public extern static int AllocConsole();

        #endregion
    }
}
