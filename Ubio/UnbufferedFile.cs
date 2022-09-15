﻿using Microsoft.Win32.SafeHandles;
using System.Runtime.Versioning;

namespace System.IO;

[SupportedOSPlatform("windows")]
public static class UnbufferedFile
{
    #region UnbufferedFileStreamConstructors
    public static UnbufferedFileStream Create(string path) => Open(path, FileMode.Create);

    public static UnbufferedFileStream Create(string path, FileOptions options) =>
        new(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, options);

    public static UnbufferedFileStream Open(string path, FileMode mode) =>
        Open(path, mode, FileAccess.ReadWrite);

    public static UnbufferedFileStream Open(string path, FileMode mode, FileAccess access) =>
        Open(path, mode, access, FileShare.None);

    public static UnbufferedFileStream Open(string path, FileMode mode, FileAccess access, FileShare share) =>
        Open(path, new FileStreamOptions { Mode = mode, Access = access, Share = share });

    public static UnbufferedFileStream Open(string path, FileStreamOptions options) => new(path, options);

        public static SafeFileHandle OpenHandle(
            string path,
            FileMode mode = FileMode.Open,
            FileAccess access = FileAccess.Read,
            FileShare share = FileShare.Read,
            FileOptions options = FileOptions.None) => Open(path, new FileStreamOptions
            { Mode = mode, Access = access, Share = share, Options = options })
            .SafeFileHandle;
    #endregion

    #region Reading
    public static UnbufferedFileStream OpenRead(string path) =>
        new(path, FileMode.Open, FileAccess.Read, FileShare.Read);


    public static byte[] ReadAllBytes(string path) => ReadAllBytesAsync(path).Result;

    public static async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        var unbufferedFileStream = OpenRead(path);
        using var diskSectorReader = new DiskSectorReader(unbufferedFileStream);

        byte[] bytes = new byte[unbufferedFileStream.Length];
        int offset = 0;
        while (!diskSectorReader.EndOfStream)
            offset += await diskSectorReader.ReadPhysicalSectorAsync(bytes, offset, cancellationToken);

        return bytes;
    }


    public static byte[][] ReadAllLogicalSectors(string path) => ReadAllLogicalSectorsAsync(path).Result;

    public static async Task<byte[][]> ReadAllLogicalSectorsAsync(string path, CancellationToken cancellationToken = default) =>
        await _ReadAllSectorsAsync(path, ReadLogicalSectors, cancellationToken);

    public static byte[][] ReadAllPhysicalSectors(string path) => ReadAllPhysicalSectorsAsync(path).Result;

    public static async Task<byte[][]> ReadAllPhysicalSectorsAsync(string path, CancellationToken cancellationToken = default) =>
        await _ReadAllSectorsAsync(path, ReadPhysicalSectors, cancellationToken);

    static async Task<byte[][]> _ReadAllSectorsAsync(string path, Func<string, IEnumerable<byte[]>> reader, CancellationToken cancellationToken = default) =>
        (await Task.Run(
            () => reader(path),
            cancellationToken))
        .ToArray();


    public static IEnumerable<byte[]> ReadLogicalSectors(string path)
    {
        var unbufferedFileStream = OpenRead(path);
        return _ReadSectors(unbufferedFileStream, unbufferedFileStream.DiskSector.LogicalSize);
    }

    public static IEnumerable<byte[]> ReadPhysicalSectors(string path)
    {
        var unbufferedFileStream = OpenRead(path);
        return _ReadSectors(unbufferedFileStream, unbufferedFileStream.DiskSector.PhysicalSize);
    }

    static IEnumerable<byte[]> _ReadSectors(UnbufferedFileStream unbufferedFileStream, int sectorSize)
    {
        using var diskSectorReader = new DiskSectorReader(unbufferedFileStream);

        while (!diskSectorReader.EndOfStream)
        {
            var sector = new byte[sectorSize];
            diskSectorReader.ReadPhysicalSector(sector, 0);
            yield return sector;
        }
    }
    #endregion

    #region Writing
    public static UnbufferedFileStream OpenWrite(string path) =>
        new(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);

    public static void WriteAllBytes(string path, byte[] bytes) => WriteAllBytesAsync(path, bytes).Wait();

    public static async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        using var unbufferedFileStream = OpenWrite(path);

        var sectors = bytes.Chunk(unbufferedFileStream.DiskSector.PhysicalSize).ToArray();
        if (sectors.Last().Length < unbufferedFileStream.DiskSector.PhysicalSize)
        {
            var sector = new byte[unbufferedFileStream.DiskSector.PhysicalSize];
            sectors.Last().CopyTo(sector.AsMemory(0, sector.Length));
            sectors[^1] = sector;
        }

        await _WriteAllSectorsAsync(unbufferedFileStream, sectors, cancellationToken);
    }

    public static void AppendAllSectors(string path, IEnumerable<byte[]> sectors) =>
        AppendAllSectorsAsync(path, sectors).Wait();

    public static async Task AppendAllSectorsAsync(
        string path,
        IEnumerable<byte[]> sectors,
        CancellationToken cancellationToken = default)
    { await _WriteAllSectorsAsync(new UnbufferedFileStream(path, FileMode.Append), sectors, cancellationToken); }

    public static void WriteAllSectors(string path, IEnumerable<byte[]> sectors) =>
        WriteAllSectorsAsync(path, sectors).Wait();

    public static async Task WriteAllSectorsAsync(
        string path,
        IEnumerable<byte[]> sectors,
        CancellationToken cancellationToken = default)
    { await _WriteAllSectorsAsync(OpenWrite(path), sectors, cancellationToken); }

    static async Task _WriteAllSectorsAsync(
        UnbufferedFileStream unbufferedFileStream,
        IEnumerable<byte[]> sectors,
        CancellationToken cancellationToken = default)
    {
        using var diskSectorWriter = new DiskSectorWriter(unbufferedFileStream);
        foreach (byte[] sector in sectors)
            await diskSectorWriter.WritePhysicalSectorAsync(sector, cancellationToken);
    }
    #endregion


    internal static uint WithFileFlagsDisablingBuffering(this FileOptions options)
    {
        const uint unbufferedFileFlags = (uint)(FileFlags.FILE_FLAG_NO_BUFFERING | FileFlags.FILE_FLAG_WRITE_THROUGH);
        return (uint)options | unbufferedFileFlags;
    }
}
