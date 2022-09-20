namespace Ubio.Test.Adapters;

public class DiskSectorReaderTest
{
    [Fact]
    public void SkipLogicalPosition_ChangesPositions_ByLogicalSectorSize()
    {
        using var unbufferedFileStream = TestData.TestFile;
        using var reader = new DiskSectorReader(unbufferedFileStream);
        using var writer = new DiskSectorWriter(unbufferedFileStream, leaveOpen: true);
        writer.WritePhysicalSector(new byte[unbufferedFileStream.DiskSector.PhysicalSize]);
        writer.UnbufferedFileStream.Position = 0;

        reader.SkipLogicalSector();

        unbufferedFileStream.Position.Should().Be(unbufferedFileStream.DiskSector.LogicalSize);
    }
    
    [Fact]
    public void SkipPhysicalPosition_ChangesPositions_ByPhysicalSectorSize()
    {
        using var unbufferedFileStream = TestData.TestFile;
        using var reader = new DiskSectorReader(unbufferedFileStream);
        using var writer = new DiskSectorWriter(unbufferedFileStream, leaveOpen: true);
        writer.WritePhysicalSector(new byte[unbufferedFileStream.DiskSector.PhysicalSize]);
        writer.UnbufferedFileStream.Position = 0;

        reader.SkipPhysicalSector();

        unbufferedFileStream.Position.Should().Be(unbufferedFileStream.DiskSector.PhysicalSize);
    }

    [Theory]
    [InlineData(new object[] { 1 })]
    [InlineData(new object[] { 2 })]
    [InlineData(new object[] { 5 })]
    public void ReadLogicalSector_ReadsLogicalSectorSizeNumberOfBytes(int numberOfOperations)
    {
        using var unbufferedFileStream = TestData.TestFile;
        using var reader = new DiskSectorReader(unbufferedFileStream);
        using var writer = new DiskSectorWriter(unbufferedFileStream, leaveOpen: true);
        for (int i = 0; i < numberOfOperations; i++)
            writer.WritePhysicalSector(new byte[unbufferedFileStream.DiskSector.PhysicalSize]);
        writer.UnbufferedFileStream.Position = 0;

        for (int i = 0; i < numberOfOperations; i++)
        {
            int bytesRead = reader.ReadLogicalSector(new byte[unbufferedFileStream.DiskSector.LogicalSize], 0);
            bytesRead.Should().Be(unbufferedFileStream.DiskSector.LogicalSize);
        }
    }

    [Theory]
    [InlineData(new object[] { 1 })]
    [InlineData(new object[] { 2 })]
    [InlineData(new object[] { 5 })]
    public void ReadPhysicalSector_ReadsPhysicalSectorSizeNumberOfBytes(int numberOfOperations)
    {
        using var unbufferedFileStream = TestData.TestFile;
        using var reader = new DiskSectorReader(unbufferedFileStream);
        using var writer = new DiskSectorWriter(unbufferedFileStream, leaveOpen: true);
        for (int i = 0; i < numberOfOperations; i++)
            writer.WritePhysicalSector(new byte[unbufferedFileStream.DiskSector.PhysicalSize]);
        writer.UnbufferedFileStream.Position = 0;

        for (int i = 0; i < numberOfOperations; i++)
        {
            int bytesRead = reader.ReadPhysicalSector(new byte[unbufferedFileStream.DiskSector.PhysicalSize], 0);
            bytesRead.Should().Be(unbufferedFileStream.DiskSector.PhysicalSize);
        }
    }

    [Theory]
    [InlineData(new object[] { 1 })]
    [InlineData(new object[] { 2 })]
    [InlineData(new object[] { 5 })]
    public void ToPreviousLogicalSector_RevertsPositionBack_ByLogicalSectorSize(int numberOfWrites)
    {
        using var unbufferedFileStream = TestData.TestFile;
        using var reader = new DiskSectorReader(unbufferedFileStream);
        using var writer = new DiskSectorWriter(unbufferedFileStream, leaveOpen: true);
        for (int i = 0; i < numberOfWrites; i++)
            writer.WritePhysicalSector(new byte[unbufferedFileStream.DiskSector.PhysicalSize]);
        long positionBefore = unbufferedFileStream.Position;

        reader.ToPreviousLogicalSector();
        var positionAfter = unbufferedFileStream.Position;

        positionAfter.Should().Be(positionBefore - unbufferedFileStream.DiskSector.LogicalSize);
    }

    [Theory]
    [InlineData(new object[] { 1 })]
    [InlineData(new object[] { 2 })]
    [InlineData(new object[] { 5 })]
    public void ToPreviousPhysicalSector_RevertsPositionBack_ByPhysicalSectorSize(int numberOfWrites)
    {
        using var unbufferedFileStream = TestData.TestFile;
        using var reader = new DiskSectorReader(unbufferedFileStream);
        using var writer = new DiskSectorWriter(unbufferedFileStream, leaveOpen: true);
        for (int i = 0; i < numberOfWrites; i++)
            writer.WritePhysicalSector(new byte[unbufferedFileStream.DiskSector.PhysicalSize]);
        long positionBefore = unbufferedFileStream.Position;

        reader.ToPreviousPhysicalSector();
        var positionAfter = unbufferedFileStream.Position;

        positionAfter.Should().Be(positionBefore - unbufferedFileStream.DiskSector.PhysicalSize);
    }
}
