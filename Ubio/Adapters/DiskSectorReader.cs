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

public class DiskSectorReader : IDisposable
{
    readonly bool _leaveOpen;


    public UnbufferedFileStream UnbufferedFileStream { get; }
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


    public void SkipLogicalSector() => _SkipSectorCore(UnbufferedFileStream.DiskSector.LogicalSize);

    public void SkipPhysicalSector() => _SkipSectorCore(UnbufferedFileStream.DiskSector.PhysicalSize);

    void _SkipSectorCore(int sectorSize)
    {
        if (UnbufferedFileStream.Position + sectorSize <= UnbufferedFileStream.Length)
        { UnbufferedFileStream.Seek(sectorSize, SeekOrigin.Current); }
        else
        { UnbufferedFileStream.Seek(0, SeekOrigin.End); }
    }


    public int ReadLogicalSector(byte[] buffer, int index) => ReadLogicalSectorAsync(buffer, index).Result;

    public int ReadPhysicalSector(byte[] buffer, int index) => ReadPhysicalSectorAsync(buffer, index).Result;

    public async Task<int> ReadLogicalSectorAsync(byte[] buffer, int index, CancellationToken cancellationToken = default) =>
        await _ReadSectorAsync(buffer, index, UnbufferedFileStream.DiskSector.LogicalSize, cancellationToken);

    public async Task<int> ReadPhysicalSectorAsync(byte[] buffer, int index, CancellationToken cancellationToken = default) =>
        await _ReadSectorAsync(buffer, index, UnbufferedFileStream.DiskSector.PhysicalSize, cancellationToken);

    async Task<int> _ReadSectorAsync(byte[] buffer, int index, int sectorSize, CancellationToken cancellationToken = default) =>
        await _ReadSectorAsyncCore(buffer.AsMemory(index, sectorSize), cancellationToken);

    async Task<int> _ReadSectorAsyncCore(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
        await UnbufferedFileStream.ReadAsync(buffer, cancellationToken);


    public long ToPreviousLogicalSector() => _ToPreviousSectorCore(UnbufferedFileStream.DiskSector.LogicalSize);

    public long ToPreviousPhysicalSector() => _ToPreviousSectorCore(UnbufferedFileStream.DiskSector.PhysicalSize);

    long _ToPreviousSectorCore(int sectorSize) => UnbufferedFileStream.Position < sectorSize ?
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
