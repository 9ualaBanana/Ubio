using System.Runtime.InteropServices;

namespace System.IO;

internal static class Win32
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    internal static extern IntPtr CreateFile(
    [MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
    [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
    [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
    IntPtr lpSecurityAttributes,
    [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
    [MarshalAs(UnmanagedType.U4)] uint dwFlagsAndAttributes,
    IntPtr hTemplateFile);

    [DllImport("kernel32.dll")]
    internal static extern bool WriteFile(
        IntPtr hFile,
        byte[] lpBuffer,
        uint nNumberOfBytesToWrite,
        out uint lpNumberOfBytesWritten,
        [In] ref NativeOverlapped lpOverlapped);

    [DllImport("kernel32.dll")]
    internal static extern bool ReadFile(
        IntPtr hFile,
        byte[] lpBuffer,
        uint nNumberOfBytesToRead,
        out uint lpNumberOfBytesRead,
        [In] ref NativeOverlapped lpOverlapped);
}
