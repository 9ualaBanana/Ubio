namespace Ubio.Test;

public class UnbufferedFileStreamTest
{
    [Fact]
    public void Seek_ToSectorAlignedPosition_ChangesPosition()
    {
        using var file = TestData.TestFile;
        var sectorAllignedPosition = file.DiskSector.LogicalSize;

        file.Seek(sectorAllignedPosition, SeekOrigin.Begin);

        file.Position.Should().Be(sectorAllignedPosition);
    }

    [Fact]
    public void Seek_ToNonSectorAlignedPosition_Throws()
    {
        using var file = TestData.TestFile;
        var nonSectorAllignedPosition = file.DiskSector.LogicalSize + 1;

        Action seekingToNonSectorAlignedPosition = () => file.Seek(nonSectorAllignedPosition, SeekOrigin.Begin);

        seekingToNonSectorAlignedPosition.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetLength_ToSectorAlignedPosition_SetsLength()
    {
        using var file = TestData.TestFile;
        var sectorAllignedPosition = file.DiskSector.LogicalSize;

        file.SetLength(sectorAllignedPosition);

        file.Length.Should().Be(sectorAllignedPosition);
    }

    [Fact]
    public void SetLength_ToNonSectorAlignedPosition_Throws()
    {
        using var file = TestData.TestFile;
        var nonSectorAllignedPosition = file.DiskSector.LogicalSize + 1;

        Action settingLengthToNonSectorAlignedPosition = () => file.SetLength(nonSectorAllignedPosition);

        settingLengthToNonSectorAlignedPosition.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Write_LoficalSizeNumberOfBytes_WritesThatNumberOfBytes()
    {
        using var file = TestData.TestFile;

        var array = new byte[file.DiskSector.LogicalSize];
        file.Write(array, 0, file.DiskSector.LogicalSize);

        file.Length.Should().Be(file.DiskSector.LogicalSize);
    }

    [Fact]
    public void Write_NonAlignedNumberOfBytes_Throws()
    {
        using var file = TestData.TestFile;
        var nonAlignedNumberOfBytes = file.DiskSector.LogicalSize + 1;
        
        Action writingNonAlignedNumberOfBytes = () => file.Write(new byte[nonAlignedNumberOfBytes], 0, nonAlignedNumberOfBytes);

        writingNonAlignedNumberOfBytes.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public void Read_AlignedNumberOfBytes_ReadsThatNumberOfBytes()
    {
        using var file = TestData.TestFile;
        var alignedNumberOfBytes = file.DiskSector.LogicalSize;
        var array = new byte[alignedNumberOfBytes];
        file.Write(array, 0, alignedNumberOfBytes);
        file.Position = 0;

        file.Read(new byte[alignedNumberOfBytes], 0, alignedNumberOfBytes).Should().Be(alignedNumberOfBytes);
    }

    [Fact]
    public void Read_NonAlignedNumberOfBytes_Throws()
    {
        using var file = TestData.TestFile;
        var alignedNumberOfBytes = file.DiskSector.LogicalSize;
        var nonAlignedNumberOfBytes = file.DiskSector.LogicalSize + 1;
        var array = new byte[nonAlignedNumberOfBytes];
        file.Write(array, 0, alignedNumberOfBytes);

        Action readingNonAlignedNumberOfBytes = () => file.Read(new byte[nonAlignedNumberOfBytes], 0, nonAlignedNumberOfBytes);

        readingNonAlignedNumberOfBytes.Should().Throw<ArgumentOutOfRangeException>();
    }

}