using System.Runtime.Versioning;

namespace System.IO;

[SupportedOSPlatform("windows")]
public static class UnbufferedFile
{
    internal static uint WithFileFlagsDisablingBuffering(this FileOptions options)
    {
        const uint unbufferedFileFlags = (uint)(FileFlags.FILE_FLAG_NO_BUFFERING | FileFlags.FILE_FLAG_WRITE_THROUGH);
        return (uint)options | unbufferedFileFlags;
    }
}
