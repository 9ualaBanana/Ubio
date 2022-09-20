namespace Ubio.Test;

public class UnbufferedFileTest
{
    [Fact]
    public void WriteAllBytes_SectorAlignedNumberOfBytes_WritesThatNumberOfBytes()
    {
        var path = Path.GetTempFileName();
        int sectorAlignedNumberOfBytes;
        using (var unbufferedFileStream = UnbufferedFile.OpenRead(path))
            sectorAlignedNumberOfBytes = unbufferedFileStream.DiskSector.PhysicalSize;

        UnbufferedFile.WriteAllBytes(path, new byte[sectorAlignedNumberOfBytes]);

        UnbufferedFile.OpenRead(path).Length.Should().Be(sectorAlignedNumberOfBytes);
    }
    
    [Fact]
    public void Write_SectorAlignedNumberOfBytes_WritesThatNumberOfBytes()
    {
        var path = Path.GetTempFileName();
        int numberOfBytes;
        using (var unbufferedFileStream = TestData.TestFile)
        { numberOfBytes = unbufferedFileStream.DiskSector.PhysicalSize * 2; }

        UnbufferedFile.WriteAllBytes(path, new byte[numberOfBytes]);

        UnbufferedFile.OpenRead(path).Length.Should().Be(numberOfBytes);
    }
    
    [Theory]
    [MemberData(nameof(TestData.NonSectorAlignedNumberOfBytes), MemberType = typeof(TestData))]
    public void WriteAllBytes_NonSectorAlignedNumberOfBytes_WritesExtraSectorAlignedNumberOfBytes(
        (int NonSectorAligned, int SectorAligned) numberOfBytes
        )
    {
        var path = Path.GetTempFileName();

        UnbufferedFile.WriteAllBytes(path, new byte[numberOfBytes.NonSectorAligned]);

        UnbufferedFile.OpenRead(path).Length.Should().Be(numberOfBytes.SectorAligned);
    }
}
