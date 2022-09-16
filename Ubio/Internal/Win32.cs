using System.Runtime.InteropServices;

namespace Ubio.Internal;

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

    internal static int WriteFile(IntPtr hFile, byte[] lpBuffer, int nOffset, int nNumberOfBytesToRead)
    {
        var nativeOffset = new OrderedBytes(nOffset).AsNativeOverlapped;

        if (!WriteFile(hFile, lpBuffer, (uint)nNumberOfBytesToRead, out uint lpNumberOfBytesRead, ref nativeOffset))
        { ThrowExceptionForLastWin32Error(); throw new Exception(); }
        else return (int)lpNumberOfBytesRead;
    }

    [DllImport("kernel32.dll")]
    internal static extern bool WriteFile(
        IntPtr hFile,
        byte[] lpBuffer,
        uint nNumberOfBytesToWrite,
        out uint lpNumberOfBytesWritten,
        [In] ref NativeOverlapped lpOverlapped);

    internal static int ReadFile(IntPtr hFile, byte[] lpBuffer, int nOffset, int nNumberOfBytesToRead)
    {
        var nativeOffset = new OrderedBytes(nOffset).AsNativeOverlapped;

        if (!ReadFile(hFile, lpBuffer, (uint)nNumberOfBytesToRead, out uint lpNumberOfBytesRead, ref nativeOffset))
        { ThrowExceptionForLastWin32Error(ex => throw new ArgumentOutOfRangeException("count", nNumberOfBytesToRead, "")); throw new Exception(); }
        else return (int)lpNumberOfBytesRead;
    }

    [DllImport("kernel32.dll")]
    internal static extern bool ReadFile(
        IntPtr hFile,
        byte[] lpBuffer,
        uint nNumberOfBytesToRead,
        out uint lpNumberOfBytesRead,
        [In] ref NativeOverlapped lpOverlapped);


    internal static long GetFileSize(IntPtr hFile)
    {
        if (GetFileSizeEx(hFile, out var size)) return size;
        else ThrowExceptionForLastWin32Error(); throw new Exception();
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GetFileSizeEx(IntPtr hFile, out long lpFileSize);


    internal static long GetFilePointerPosition(IntPtr hFile)
    {
        if (SetFilePointerEx(hFile, 0, out var position, SeekOrigin.Current)) return position;
        else ThrowExceptionForLastWin32Error(); throw new Exception();
    }

    internal static long SetFilePointerPosition(IntPtr hFile, long liDistanceToMove, SeekOrigin dwMoveMethod)
    {
        if (SetFilePointerEx(hFile, liDistanceToMove, out var position, dwMoveMethod)) return position;
        else ThrowExceptionForLastWin32Error(); throw new Exception();
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetFilePointerEx(IntPtr hFile, long liDistanceToMove, out long lpNewFilePointer, SeekOrigin dwMoveMethod);


    internal static void SetFileEnd(IntPtr hFile)
    {
        if (!SetEndOfFile(hFile))
            ThrowExceptionForLastWin32Error();
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetEndOfFile(IntPtr hFile);

    internal static (int Logical, int Physical) GetSectorSizes(string? lpRootPathName)
    {
        if (GetDiskFreeSpace(lpRootPathName, out long sectorsPerCluster, out long logicalSectorSize, out _, out _))
            return ((int)logicalSectorSize, (int)(logicalSectorSize * sectorsPerCluster));
        else ThrowExceptionForLastWin32Error(); throw new Exception();
    }

    internal static void LockFile(IntPtr hFile, long position, long length)
    {
        var nativePosition = new OrderedBytes(position).AsNativeOverlapped;
        var nativeLength = new OrderedBytes(length).AsNativeOverlapped;

        if (!LockFile(
            hFile,
            (uint)nativePosition.OffsetLow, (uint)nativePosition.OffsetHigh,
            (uint)nativeLength.OffsetLow, (uint)nativeLength.OffsetHigh))
            ThrowExceptionForLastWin32Error();
    }

    [DllImport("kernel32.dll")]
    static extern bool LockFile(
        IntPtr hFile,
        uint dwFileOffsetLow,
        uint dwFileOffsetHigh,
        uint nNumberOfBytesToLockLow,
        uint nNumberOfBytesToLockHigh);

    internal static void UnlockFile(IntPtr hFile, long position, long length)
    {
        var nativePosition = new OrderedBytes(position).AsNativeOverlapped;
        var nativeLength = new OrderedBytes(length).AsNativeOverlapped;

        if (!UnlockFile(
            hFile,
            (uint)nativePosition.OffsetLow, (uint)nativePosition.OffsetHigh,
            (uint)nativeLength.OffsetLow, (uint)nativeLength.OffsetHigh))
            ThrowExceptionForLastWin32Error();

    }

    [DllImport("kernel32.dll")]
    static extern bool UnlockFile(
        IntPtr hFile,
        uint dwFileOffsetLow,
        uint dwFileOffsetHigh,
        uint nNumberOfBytesToLockLow,
        uint nNumberOfBytesToLockHigh);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern bool GetDiskFreeSpace(
        string? lpRootPathName,
        out long lpSectorsPerCluster,
        out long lpBytesPerSector,
        out long lpNumberOfFreeClusters,
        out long lpTotalNumberOfClusters);

    static void ThrowExceptionForLastWin32Error(Action<Exception>? errorWrapper = null)
    { Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error(), new IntPtr(-1)); throw new Exception(); }
}
