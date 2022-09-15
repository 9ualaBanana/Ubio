namespace System.IO;

public class DiskSectorReader : IDisposable
{
    bool _leaveOpen;
    bool _isDisposed;


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


    public void SkipLogicalSector() => ReadLogicalSector(Array.Empty<byte>(), default);

    public void SkipPhysicalSector() => ReadPhysicalSector(Array.Empty<byte>(), default);


    public virtual int ReadLogicalSector(byte[] buffer, int index) =>
        ReadSectorAsync(buffer.AsMemory(index, UnbufferedFileStream.DiskSector.LogicalSize)).Result;

    public virtual int ReadPhysicalSector(byte[] buffer, int index) =>
        ReadSectorAsync(buffer.AsMemory(index, UnbufferedFileStream.DiskSector.LogicalSize)).Result;

    public virtual async Task<int> ReadLogicalSectorAsync(byte[] buffer, int index, CancellationToken cancellationToken = default) =>
        await ReadSectorAsync(buffer.AsMemory(index, UnbufferedFileStream.DiskSector.LogicalSize), cancellationToken);

    public virtual async Task<int> ReadPhysicalSectorAsync(byte[] buffer, int index, CancellationToken cancellationToken = default) =>
        await ReadSectorAsync(buffer.AsMemory(index, UnbufferedFileStream.DiskSector.PhysicalSize), cancellationToken);

    async Task<int> ReadSectorAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
        await UnbufferedFileStream.ReadAsync(buffer, cancellationToken);


    public virtual long ToPreviousLogicalSector() => ToPreviousSector(UnbufferedFileStream.DiskSector.LogicalSize);

    public virtual long ToPreviousPhysicalSector() => ToPreviousSector(UnbufferedFileStream.DiskSector.PhysicalSize);

    long ToPreviousSector(int sectorSize) => UnbufferedFileStream.Position < sectorSize ?
        UnbufferedFileStream.Seek(0, SeekOrigin.Begin) : UnbufferedFileStream.Seek(-sectorSize, SeekOrigin.Current);


    #region IDisposable
    public void Dispose() => Dispose(true);

    protected virtual void Dispose(bool managed)
    { if (!_isDisposed && managed && !_leaveOpen) UnbufferedFileStream.Dispose(); }
    #endregion
}
