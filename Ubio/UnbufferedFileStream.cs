using Microsoft.Win32.SafeHandles;
using System.Runtime.Versioning;

namespace System.IO;

[SupportedOSPlatform("windows")]
public class UnbufferedFileStream : Stream
{
    IntPtr _Handle => _safeFileHandle.DangerousGetHandle();
    readonly SafeFileHandle _safeFileHandle;
    readonly FileAccess _fileAccess;
    
    
    public readonly DiskSector DiskSector;
    public override long Length => _length;
    long _length;
    public override long Position
    {
        get => _position;
        set => Seek(value, SeekOrigin.Begin);
    }
    long _position;


    #region Constructors
    public UnbufferedFileStream(string path, FileMode mode)
        : this(path, new FileStreamOptions
        {
            Mode = mode,
            Access = mode.HasFlag(FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite,
            Share = FileShare.Read
        })
    {
    }

    public UnbufferedFileStream(string path, FileMode mode, FileAccess access)
        : this(path, new FileStreamOptions { Mode = mode, Access = access, Share = FileShare.Read })
    {
    }

    public UnbufferedFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        : this(path, new FileStreamOptions { Mode = mode, Access = access, Share = share })
    {
    }

    public UnbufferedFileStream(string path, FileMode mode, FileAccess access, FileShare share, bool useAsync)
        : this(path, new FileStreamOptions { Mode = mode, Access = access, Share = share, Options = useAsync ?
            FileOptions.Asynchronous : FileOptions.None })
    {
    }
    
    public UnbufferedFileStream(string path, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        : this(path, new FileStreamOptions { Mode = mode, Access = access, Share = share, Options = options })
    {
    }

    public UnbufferedFileStream(string path, FileStreamOptions options)
    {
        _safeFileHandle = new(
            Win32.CreateFile(
                path,
                options.Access,
                options.Share,
                IntPtr.Zero,
                options.Mode,
                options.Options.WithFileFlagsDisablingBuffering(),
                IntPtr.Zero),
            ownsHandle: true);
        DiskSector = new(path);
        _fileAccess = options.Access;
        _length = Win32.GetFileSize(_Handle);
        _position = Win32.GetFilePointerPosition(_Handle);
    }
    #endregion


    public override bool CanRead => _fileAccess.HasFlag(FileAccess.Read);
    public override bool CanSeek => true;
    public override bool CanWrite => _fileAccess.HasFlag(FileAccess.Write);
    public override void Flush() { }

    public override int Read(byte[] buffer, int offset, int count)
    {
        DiskSector.EnsureIsAligned(count);
        return Win32.ReadFile(_Handle, buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        DiskSector.EnsureIsAligned(offset);
        return _position = Win32.SetFilePointerPosition(_Handle, offset, origin);
    }

    public override void SetLength(long value)
    {
        var position = _position;
        _SetLength(value);
        _position = position;
    }

    void _SetLength(long value)
    {
        Seek(value, SeekOrigin.Begin);
        Win32.SetFileEnd(_Handle);
        _length = value;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        DiskSector.EnsureIsAligned(count);
        Position = _length = Win32.WriteFile(_Handle, buffer, offset, count);
    }

    public void Lock(long position, long length) => Win32.LockFile(_Handle, position, length);

    public void Unlock(long position, long length) => Win32.UnlockFile(_Handle, position, length);
}
