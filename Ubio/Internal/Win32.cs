using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Ubio.Internal;

[SupportedOSPlatform("windows")]
internal static class Win32
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr CreateFile(
    [MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
    [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
    [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
    IntPtr lpSecurityAttributes,
    [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
    [MarshalAs(UnmanagedType.U4)] uint dwFlagsAndAttributes,
    IntPtr hTemplateFile);

    internal static int WriteFile(IntPtr hFile, byte[] lpBuffer, long nOffset, int nNumberOfBytesToRead)
    {
        var overlappedWithOffset = new OrderedBytes(nOffset).AsNativeOverlapped.ForIO();

        if (WriteFile(hFile, lpBuffer, nNumberOfBytesToRead, out var lpNumberOfBytesRead, in overlappedWithOffset))
            return lpNumberOfBytesRead;
        else return GetOverlappedOrThrow(hFile, in overlappedWithOffset);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool WriteFile(
        IntPtr hFile,
        byte[] lpBuffer,
        int nNumberOfBytesToWrite,
        out int lpNumberOfBytesWritten,
        [In] in NativeOverlapped lpOverlapped);

    internal static int ReadFile(IntPtr hFile, byte[] lpBuffer, long nOffset, int nNumberOfBytesToRead)
    {
        var overlappedWithOffset = new OrderedBytes(nOffset).AsNativeOverlapped.ForIO();

        if (ReadFile(hFile, lpBuffer, nNumberOfBytesToRead, out var lpNumberOfBytesRead, in overlappedWithOffset))
            return lpNumberOfBytesRead;
        else return GetOverlappedOrThrow(hFile, in overlappedWithOffset,
            ex => throw new ArgumentOutOfRangeException("count", nNumberOfBytesToRead, ""));
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool ReadFile(
        IntPtr hFile,
        byte[] lpBuffer,
        int nNumberOfBytesToRead,
        out int lpNumberOfBytesRead,
        [In] in NativeOverlapped lpOverlapped);

    static NativeOverlapped ForIO(this NativeOverlapped nativeOverlapped)
    { nativeOverlapped.EventHandle = new ManualResetEventSlim().WaitHandle.GetSafeWaitHandle().DangerousGetHandle(); return nativeOverlapped; }

    static int GetOverlappedOrThrow(IntPtr hFile, in NativeOverlapped lpOverlapped, Action<Exception>? errorWrapper = null)
    {
        const uint ERROR_SUCCESS = 0X0;
        const uint ERROR_IO_PENDING = 0x3E5;

        var lastError = Marshal.GetLastWin32Error();
        if (lastError != ERROR_SUCCESS && lastError != ERROR_IO_PENDING)
        { ThrowExceptionForLastWin32Error(errorWrapper); }

        GetOverlappedResult(hFile, in lpOverlapped, out int lpNumberOfBytesTransferred, true); return lpNumberOfBytesTransferred;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GetOverlappedResult(
        IntPtr hFile,
        [In] in NativeOverlapped lpOverlapped,
        out int lpNumberOfBytesTransferred,
        bool bWait);

    internal static long GetFileSize(IntPtr hFile)
    {
        if (!GetFileSizeEx(hFile, out var size))
            ThrowExceptionForLastWin32Error();

        return size;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GetFileSizeEx(IntPtr hFile, out long lpFileSize);


    internal static long GetFilePointerPosition(IntPtr hFile)
    {
        if (!SetFilePointerEx(hFile, 0, out var position, SeekOrigin.Current))
            ThrowExceptionForLastWin32Error();
        
        return position;
    }

    internal static long SetFilePointerPosition(IntPtr hFile, long liDistanceToMove, SeekOrigin dwMoveMethod)
    {
        if (!SetFilePointerEx(hFile, liDistanceToMove, out var position, dwMoveMethod))
            ThrowExceptionForLastWin32Error();

        return position;
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
        if (!GetDiskFreeSpace(lpRootPathName, out long sectorsPerCluster, out long logicalSectorSize, out _, out _))
            ThrowExceptionForLastWin32Error();

        return ((int)logicalSectorSize, (int)(logicalSectorSize * sectorsPerCluster));
    }

    internal static void LockFile(IntPtr hFile, long position, long length)
    {
        var nativePosition = new OrderedBytes(position).AsNativeOverlapped;
        var nativeLength = new OrderedBytes(length).AsNativeOverlapped;

        if (!LockFile(
            hFile,
            (uint)nativePosition.OffsetLow, (uint)nativePosition.OffsetHigh,
            (uint)nativeLength.OffsetLow, (uint)nativeLength.OffsetHigh))
        { ThrowExceptionForLastWin32Error(); }
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
        { ThrowExceptionForLastWin32Error(); }

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
    {
        if (errorWrapper is null) ThrowExceptionForLastWin32ErrorCore();
        else
        {
            try { ThrowExceptionForLastWin32ErrorCore(); }
            catch (Exception ex) { errorWrapper(ex); }
        }
    }

    static void ThrowExceptionForLastWin32ErrorCore()
    { Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error(), new IntPtr(-1)); }
}
