namespace System.IO;

public static class FileInfoExtensions
{
    public static StreamWriter CreateTextUnbuffered(this FileInfo fileInfo) =>
        UnbufferedFile.CreateText(fileInfo.FullName);

    public static UnbufferedFileStream CreateUnbuffered(this FileInfo fileInfo) =>
        UnbufferedFile.Create(fileInfo.FullName);

    public static UnbufferedFileStream OpenWriteUnbuffered(this FileInfo fileInfo) =>
        UnbufferedFile.OpenWrite(fileInfo.FullName);

    public static StreamWriter AppendTextUnbuffered(this FileInfo fileInfo) =>
        UnbufferedFile.AppendText(fileInfo.FullName);

    public static StreamReader OpenTextUnbuffered(this FileInfo fileInfo) =>
        new(OpenReadUnbuffered(fileInfo));

    public static UnbufferedFileStream OpenReadUnbuffered(this FileInfo fileInfo) =>
        UnbufferedFile.OpenRead(fileInfo.FullName);
}
