using Microsoft.Win32.SafeHandles;
using System.Runtime.Versioning;

namespace System.IO;

[SupportedOSPlatform("windows")]
public static class UnbufferedFile
{
    public static SafeFileHandle OpenHandle(
        string path,
        FileMode mode = FileMode.Open,
        FileAccess access = FileAccess.Read,
        FileShare share = FileShare.Read,
        FileOptions options = FileOptions.None) => Open(path, new FileStreamOptions
        { Mode = mode, Access = access, Share = share, Options = options })
        .SafeFileHandle;

    public static UnbufferedFileStream Open(string path, FileMode mode) =>
        Open(path, mode, FileAccess.ReadWrite);

    public static UnbufferedFileStream Open(string path, FileMode mode, FileAccess access) =>
        Open(path, mode, access, FileShare.None);

    public static UnbufferedFileStream Open(string path, FileMode mode, FileAccess access, FileShare share) =>
        Open(path, new FileStreamOptions { Mode = mode, Access = access, Share = share });

    public static UnbufferedFileStream Open(string path, FileStreamOptions options) => new(path, options);

    public static UnbufferedFileStream Create(string path) => Open(path, FileMode.Create);

    public static UnbufferedFileStream Create(string path, FileOptions options) =>
        new(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, options);

    public static UnbufferedFileStream OpenWrite(string path) =>
        new(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);

    public static UnbufferedFileStream OpenRead(string path) =>
        new(path, FileMode.Open, FileAccess.Read, FileShare.Read);


    internal static uint WithFileFlagsDisablingBuffering(this FileOptions options)
    {
        const uint unbufferedFileFlags = (uint)(FileFlags.FILE_FLAG_NO_BUFFERING | FileFlags.FILE_FLAG_WRITE_THROUGH);
        return (uint)options | unbufferedFileFlags;
    }
}
