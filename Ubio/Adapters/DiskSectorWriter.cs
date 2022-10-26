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


    public void WriteSector(byte[] sector) => WriteSectorAsync(sector).Wait();

    public async Task WriteSectorAsync(byte[] sector, CancellationToken cancellationToken = default)
    {
        if (sector.Length >= UnbufferedFileStream.DiskSector.PhysicalSize)
        { await WritePhysicalSectorAsync(sector, cancellationToken); }
        else
        { await WriteLogicalSectorAsync(sector, cancellationToken); }
    }

    // TODO: Wrap ArgumentOutOfRange exceptions thrown by .AsSpan() into more descriptive ones.
    public virtual void WriteLogicalSector(byte[] sector) =>
        UnbufferedFileStream.Write(sector.AsSpan(0, UnbufferedFileStream.DiskSector.LogicalSize));

    public virtual void WritePhysicalSector(byte[] sector) =>
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
