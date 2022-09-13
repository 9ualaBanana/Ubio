namespace System.IO;

public static class FileInfoExtensions
{
    public static UnbufferedFileStream CreateUnbuffered(this FileInfo fileInfo) =>
        UnbufferedFile.Create(fileInfo.FullName);

    public static UnbufferedFileStream OpenWriteUnbuffered(this FileInfo fileInfo) =>
        UnbufferedFile.OpenWrite(fileInfo.FullName);

    public static UnbufferedFileStream OpenReadUnbuffered(this FileInfo fileInfo) =>
        UnbufferedFile.OpenRead(fileInfo.FullName);
}
