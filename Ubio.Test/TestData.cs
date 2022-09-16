namespace Ubio.Test;

internal static class TestData
{
    internal static UnbufferedFileStream TestFile => new UnbufferedFileStream(
        Path.GetRandomFileName(),
        new FileStreamOptions { Mode = FileMode.Create, Access = FileAccess.ReadWrite, Options = FileOptions.DeleteOnClose });
}
