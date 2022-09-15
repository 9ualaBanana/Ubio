namespace System.IO;

public class DiskSectorWriter : IDisposable
{
    readonly bool _leaveOpen;


    public virtual UnbufferedFileStream UnbufferedFileStream { get; }


    #region Initialization
    public DiskSectorWriter(string path) : this(UnbufferedFile.OpenWrite(path))
    {
    }

    public DiskSectorWriter(string path, FileStreamOptions options)
        : this(new UnbufferedFileStream(path, options))
    {
    }

    public DiskSectorWriter(UnbufferedFileStream unbufferedFileStream, bool leaveOpen = false)
    { UnbufferedFileStream = unbufferedFileStream; _leaveOpen = leaveOpen; }
    #endregion


    public virtual void WriteLogicalSectorAsync(byte[] sector) =>
        UnbufferedFileStream.Write(sector.AsSpan(0, UnbufferedFileStream.DiskSector.LogicalSize));

    public virtual void WritePhysicalSectorAsync(byte[] sector) =>
        UnbufferedFileStream.Write(sector.AsSpan(0, UnbufferedFileStream.DiskSector.PhysicalSize));

    public virtual async Task WriteLogicalSectorAsync(byte[] sector, CancellationToken cancellationToken = default) =>
        await UnbufferedFileStream.WriteAsync(sector.AsMemory(0, UnbufferedFileStream.DiskSector.LogicalSize), cancellationToken);

    public virtual async Task WritePhysicalSectorAsync(byte[] sector, CancellationToken cancellationToken = default) =>
        await UnbufferedFileStream.WriteAsync(sector.AsMemory(0, UnbufferedFileStream.DiskSector.PhysicalSize), cancellationToken);


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
