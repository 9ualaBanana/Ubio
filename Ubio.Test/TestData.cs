namespace Ubio.Test;

internal static class TestData
{
    internal static UnbufferedFileStream TestFile => new UnbufferedFileStream(
        Path.GetRandomFileName(),
        new FileStreamOptions { Mode = FileMode.Create, Access = FileAccess.ReadWrite, Options = FileOptions.DeleteOnClose });

    public static IEnumerable<object[]> NonSectorAlignedNumberOfBytes
    {
        get
        {
            int sectorAlignedNumberOfBytes;
            using (var unbufferedFileStream = UnbufferedFile.OpenRead(Path.GetTempFileName()))
                sectorAlignedNumberOfBytes = unbufferedFileStream.DiskSector.PhysicalSize;

            yield return new object[] { (sectorAlignedNumberOfBytes - 1, sectorAlignedNumberOfBytes) };
            yield return new object[] { (sectorAlignedNumberOfBytes + 1, 2 * sectorAlignedNumberOfBytes) };
        }
    }
}
