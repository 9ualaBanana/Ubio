namespace Ubio.Test.Adapters;

public class DiskWriterTest
{
    [Fact]
    public void WriteSector_WritesLogicalSector()
    {
        var unbufferedFileStream = TestData.TestFile;
        using var diskSectorWriter = new DiskSectorWriter(unbufferedFileStream);

        diskSectorWriter.WriteSector(new byte[unbufferedFileStream.DiskSector.LogicalSize]);

        unbufferedFileStream.Length.Should().Be(unbufferedFileStream.DiskSector.LogicalSize);
    }
    
    [Fact]
    public void WriteSector_WritesPhysicalSector()
    {
        var unbufferedFileStream = TestData.TestFile;
        using var diskSectorWriter = new DiskSectorWriter(unbufferedFileStream);

        diskSectorWriter.WriteSector(new byte[unbufferedFileStream.DiskSector.PhysicalSize]);

        unbufferedFileStream.Length.Should().Be(unbufferedFileStream.DiskSector.PhysicalSize);
    }

    [Fact]
    public void WriteLogicalSector_LogicalSectorSizeNumberOfBytes_WritesThatNumberOfBytes()
    {
        var unbufferedFileStream = TestData.TestFile;
        using var diskSectorWriter = new DiskSectorWriter(unbufferedFileStream);

        diskSectorWriter.WriteLogicalSector(new byte[unbufferedFileStream.DiskSector.LogicalSize]);

        unbufferedFileStream.Length.Should().Be(unbufferedFileStream.DiskSector.LogicalSize);
    }
    
    [Fact]
    public void WriteLogicalSector_PhysicalSectorSizeNumberOfBytes_WritesLogicalSectorSizeNumberOfBytes()
    {
        var unbufferedFileStream = TestData.TestFile;
        using var diskSectorWriter = new DiskSectorWriter(unbufferedFileStream);

        diskSectorWriter.WriteLogicalSector(new byte[unbufferedFileStream.DiskSector.PhysicalSize]);

        unbufferedFileStream.Length.Should().Be(unbufferedFileStream.DiskSector.LogicalSize);
    }
    
    [Fact]
    public void WritePhysicalSector_PhysicalSectorSizeNumberOfBytes_WritesThatNumberOfBytes()
    {
        var unbufferedFileStream = TestData.TestFile;
        using var diskSectorWriter = new DiskSectorWriter(unbufferedFileStream);

        diskSectorWriter.WritePhysicalSector(new byte[unbufferedFileStream.DiskSector.PhysicalSize]);

        unbufferedFileStream.Length.Should().Be(unbufferedFileStream.DiskSector.PhysicalSize);
    }
    
    [Fact]
    public void WritePhysicalSector_LogicalSectorSizeNumberOfBytes_Throws()
    {
        var unbufferedFileStream = TestData.TestFile;
        using var diskSectorWriter = new DiskSectorWriter(unbufferedFileStream);

        Action writing = () => diskSectorWriter.WritePhysicalSector(new byte[unbufferedFileStream.DiskSector.LogicalSize]);

        writing.Should().Throw<ArgumentOutOfRangeException>();
    }

}
