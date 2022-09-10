namespace System.IO;

[Flags]
internal enum FileFlags : uint
{
    FILE_FLAG_NO_BUFFERING = 0x20000000,
    FILE_FLAG_WRITE_THROUGH = 0x80000000
}
