namespace System.IO;

public class DiskSector
{
    public readonly int LogicalSize;
    public readonly int PhysicalSize;
    public bool IsAligned(long fileAccessSize) => fileAccessSize % LogicalSize == 0;


    internal DiskSector(string path)
    {
        (LogicalSize, PhysicalSize) = Win32.GetSectorSizes(Path.IsPathRooted(path) ? Path.GetPathRoot(path) : null);
    }


    public int EnsureIsAligned(int fileAccessSize) => (int)EnsureIsAligned((long)fileAccessSize);
    public long EnsureIsAligned(long fileAccessSize)
    {
        if (!IsAligned(fileAccessSize))
            throw new ArgumentOutOfRangeException(
                nameof(fileAccessSize),
                "Unbuffered file access sizes must be for a number of bytes that is an integer multiple of the volume sector size.");
        return fileAccessSize;
    }
}
