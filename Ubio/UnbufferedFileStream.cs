global using Ubio.Internal;
using Microsoft.Win32.SafeHandles;
using System.Runtime.Versioning;

namespace System.IO;

/// <summary>
/// Provides an unbuffered <see cref="Stream"/> for a file,
/// supporting both synchronous and asynchronous read and write operations.
/// </summary>
/// <remarks>
/// <b>*WARNING*</b> Certain requirements regarding file access sizes must be met when working with unbuffered files.<br/>
/// <seealso href="https://docs.microsoft.com/en-us/windows/win32/fileio/file-buffering#alignment-and-file-access-requirements"/><br/>
/// Use <see cref="DiskSectorReader"/> and <see cref="DiskSectorWriter"/> to simplify working with <see cref="UnbufferedFileStream"/>.
/// </remarks>
[SupportedOSPlatform("windows")]
public class UnbufferedFileStream : Stream
{
    readonly FileAccess _fileAccess;


    IntPtr _Handle => SafeFileHandle.DangerousGetHandle();

    /// <summary>
    /// Gets a <see cref="Microsoft.Win32.SafeHandles.SafeFileHandle"/> object that represents the operating system file handle
    /// for the file that the current <see cref="UnbufferedFileStream"/> object encapsulates.
    /// </summary>
    /// <returns>
    /// An object that represents the operating system file handle for the file that the current <see cref="UnbufferedFileStream"/> object encapsulates.
    /// </returns>
    public readonly SafeFileHandle SafeFileHandle;

    /// <summary>
    /// <see cref="IO.DiskSector"/> information of the disk where the current <see cref="UnbufferedFileStream"/> is located.
    /// </summary>
    public readonly DiskSector DiskSector;

    /// <summary>
    /// Gets the absolute path of the file opened in <see cref="UnbufferedFileStream"/>.
    /// </summary>
    /// <returns>
    /// A string that is the absolute path of the file.</returns>
    public string Name { get; }

    /// <inheritdoc/>
    public override long Length => _length;
    long _length;

    /// <inheritdoc/>
    public override long Position
    {
        get => _position;
        set => Seek(value, SeekOrigin.Begin);
    }
    long _position;

    /// <summary>
    /// Gets a value that indicates whether the <see cref="UnbufferedFileStream"/> was opened asynchronously or synchronously.
    /// </summary>
    /// <returns><see langword="true"/> if the <see cref="UnbufferedFileStream"/> was opened asynchronously;
    /// otherwise, <see langword="false"/>.</returns>
    public bool IsAsync { get; }


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
        SafeFileHandle = new(
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
        Name = path;
        _fileAccess = options.Access;
        _length = Win32.GetFileSize(_Handle);
        _position = Win32.GetFilePointerPosition(_Handle);
        IsAsync = options.Options.HasFlag(FileOptions.Asynchronous);
    }
    #endregion


    /// <inheritdoc/>
    public override bool CanRead => _fileAccess.HasFlag(FileAccess.Read);
    /// <inheritdoc/>
    public override bool CanSeek => true;
    /// <inheritdoc/>
    public override bool CanWrite => _fileAccess.HasFlag(FileAccess.Write);
    /// <summary>Does nothing.</summary>
    public override void Flush() { }
    
    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        DiskSector.EnsureIsAligned(count);
        return _PerformPositionChangingOperation(() => Win32.ReadFile(_Handle, buffer, offset, count));
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        DiskSector.EnsureIsAligned(offset);
        return _position = Win32.SetFilePointerPosition(_Handle, offset, origin);
    }

    /// <inheritdoc/>
    public override void SetLength(long value)
    {
        var position = _position;
        _SetLengthCore(value);
        _position = position;
    }

    void _SetLengthCore(long value)
    {
        Seek(value, SeekOrigin.Begin);
        Win32.SetFileEnd(_Handle);
        _length = value;
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        DiskSector.EnsureIsAligned(count);
        _PerformPositionChangingOperation(
            () => Win32.WriteFile(_Handle, buffer, offset, count),
            setLength: true);
    }

    int _PerformPositionChangingOperation(Func<int> operation, bool setLength = false)
    {
        int positionChange;

        long newPosition = Position + (positionChange = operation());
        if (newPosition > Length) _length += newPosition - Length;
        Position = newPosition;
        if (setLength) SetLength(Length);

        return positionChange;
    }

    /// <inheritdoc/>
    public void Lock(long position, long length) => Win32.LockFile(_Handle, position, length);

    /// <inheritdoc cref="FileStream.Unlock(long, long)"/>
    public void Unlock(long position, long length) => Win32.UnlockFile(_Handle, position, length);


    #region IDisposable
    /// <inheritdoc/>
    protected override void Dispose(bool managed)
    {
        if (!_isDisposed && managed)
            SafeFileHandle.Dispose();
        
        _isDisposed = true;
    }

    bool _isDisposed;
    #endregion
}
