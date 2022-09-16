namespace System.IO;

public class DiskSectorReader : IDisposable
{
    readonly bool _leaveOpen;


    public virtual UnbufferedFileStream UnbufferedFileStream { get; }
    public bool EndOfStream => UnbufferedFileStream.Position >= UnbufferedFileStream.Length;


    #region Initialization
    public DiskSectorReader(string path) : this(UnbufferedFile.OpenRead(path))
    {
    }

    public DiskSectorReader(string path, FileStreamOptions options)
        : this(new UnbufferedFileStream(path, options))
    {
    }

    public DiskSectorReader(UnbufferedFileStream unbufferedFileStream, bool leaveOpen = false)
    { UnbufferedFileStream = unbufferedFileStream; _leaveOpen = leaveOpen; }
    #endregion


    public void SkipLogicalSector() => _SkipSector(UnbufferedFileStream.DiskSector.LogicalSize);

    public void SkipPhysicalSector() => _SkipSector(UnbufferedFileStream.DiskSector.PhysicalSize);

    void _SkipSector(int sectorSize)
    {
        if (UnbufferedFileStream.Position + sectorSize <= UnbufferedFileStream.Length)
        { UnbufferedFileStream.Seek(sectorSize, SeekOrigin.Current); }
        else
        { UnbufferedFileStream.Seek(0, SeekOrigin.End); }
    }


    public virtual int ReadLogicalSector(byte[] buffer, int index) => ReadLogicalSectorAsync(buffer, index).Result;

    public virtual int ReadPhysicalSector(byte[] buffer, int index) => ReadPhysicalSectorAsync(buffer, index).Result;

    public virtual async Task<int> ReadLogicalSectorAsync(byte[] buffer, int index, CancellationToken cancellationToken = default) =>
        await _ReadSectorAsync(buffer, index, UnbufferedFileStream.DiskSector.LogicalSize);

    public virtual async Task<int> ReadPhysicalSectorAsync(byte[] buffer, int index, CancellationToken cancellationToken = default) =>
        await _ReadSectorAsync(buffer, index, UnbufferedFileStream.DiskSector.PhysicalSize);

    async Task<int> _ReadSectorAsync(byte[] buffer, int index, int sectorSize, CancellationToken cancellationToken = default) =>
        await _ReadSectorAsync(buffer.AsMemory(index, sectorSize), cancellationToken);

    async Task<int> _ReadSectorAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
        await UnbufferedFileStream.ReadAsync(buffer, cancellationToken);


    public virtual long ToPreviousLogicalSector() => _ToPreviousSector(UnbufferedFileStream.DiskSector.LogicalSize);

    public virtual long ToPreviousPhysicalSector() => _ToPreviousSector(UnbufferedFileStream.DiskSector.PhysicalSize);

    long _ToPreviousSector(int sectorSize) => UnbufferedFileStream.Position < sectorSize ?
        UnbufferedFileStream.Seek(0, SeekOrigin.Begin) : UnbufferedFileStream.Seek(-sectorSize, SeekOrigin.Current);


    #region IDisposable
    public void Dispose() => Dispose(true);

    protected virtual void Dispose(bool managed)
    {
        if (!_isDisposed && managed && !_leaveOpen)
            UnbufferedFileStream.Dispose();

        _isDisposed = true;
    }

    bool _isDisposed;
    #endregion
}
