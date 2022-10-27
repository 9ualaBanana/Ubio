//MIT License

//Copyright (c) 2022 9ualaBanana

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

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
    void _SetLengthWithNoKernelCall(long value) => _length = value;
    long _length;

    /// <inheritdoc/>
    /// <remarks>
    /// Setter is an alias for <see cref="Seek(long, SeekOrigin)"/> so the same performance considerations apply
    /// (<i>see </i><see cref="SetLength(long)"/> <i>remarks</i>).
    /// </remarks>
    public override long Position
    {
        get => _position;
        set => Seek(value, SeekOrigin.Begin);
    }
    void _SetPositionWithNoKernelCall(long value) => _position = value;
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
            Kernel32.CreateFile(
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
        _length = Kernel32.GetFileSize(_Handle);
        _position = Kernel32.GetFilePointerPosition(_Handle);
        IsAsync = options.Options.HasFlag(FileOptions.Asynchronous);
    }
    #endregion


    /// <inheritdoc/>
    /// <remarks>
    /// Settings this property to <see langword="true"/> (<i>via constructors that accept</i> <see cref="FileAccess"/> <i>or</i> <see cref="FileShare"/>)
    /// requires tracking <see cref="Position"/> and <see cref="Length"/> on kernel level which reduces write speed upto 5 times.
    /// </remarks>
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
        return _PerformPositionChangingOperation(() => Kernel32.ReadFile(_Handle, buffer[offset..], _position, count));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Sets file pointer on kernel level (<i>reduces write speed upto 5 times</i>).
    /// </remarks>
    public override long Seek(long offset, SeekOrigin origin)
    {
        DiskSector.EnsureIsAligned(offset);
        return _position = Kernel32.SetFilePointerPosition(_Handle, offset, origin);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Sets file length on kernel level (<i>reduces write speed upto 5 times</i>).
    /// </remarks>
    public override void SetLength(long value)
    {
        var position = _position;
        _SetLengthCore(value);
        _position = position;
    }

    void _SetLengthCore(long value)
    {
        Seek(value, SeekOrigin.Begin);
        Kernel32.SetFileEnd(_Handle);
        _length = value;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Performs upto 5 times slower if <see cref="CanRead"/> is <see langword="true"/> due to overhead caused by kernel calls.
    /// </remarks>
    public override void Write(byte[] buffer, int offset, int count)
    {
        DiskSector.EnsureIsAligned(count);
        _PerformPositionChangingOperation(
            () => Kernel32.WriteFile(_Handle, buffer[offset..], _position, count),
            setLength: true);
    }

    int _PerformPositionChangingOperation(Func<int> operation, bool setLength = false)
    {
        int positionChange;

       long newPosition = _ChangePositionBy(positionChange = operation());

        if (setLength)
            if (CanRead)
            { Position = newPosition; if (newPosition > _length) SetLength(newPosition); }
            else
            { _SetPositionWithNoKernelCall(newPosition); if (newPosition > _length) _SetLengthWithNoKernelCall(newPosition); }

        return positionChange;
    }

    long _ChangePositionBy(int offset)
    {
        long newPosition = Position + offset;
        if (newPosition > _length) _length += newPosition - _length;
        return newPosition;
    }

    /// <inheritdoc/>
    public void Lock(long position, long length) => Kernel32.LockFile(_Handle, position, length);

    /// <inheritdoc cref="FileStream.Unlock(long, long)"/>
    public void Unlock(long position, long length) => Kernel32.UnlockFile(_Handle, position, length);


    #region Finalization
    /// <inheritdoc/>
    ~UnbufferedFileStream() => Dispose(false);

    /// <inheritdoc/>
    protected override void Dispose(bool managed)
    {
        if (!_isDisposed)
        {
            SetLength(_length);
            if (managed)
                SafeFileHandle.Dispose();
        }
        
        _isDisposed = true;
    }

    bool _isDisposed;
    #endregion
}
