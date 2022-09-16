using System.Runtime.Versioning;

namespace Ubio.Internal;

internal readonly struct OrderedBytes
{
    internal readonly int Low;
    internal readonly int High;


    #region Initialization
    internal OrderedBytes(int value) => (Low, High) = Init(BitConverter.GetBytes(value), value);

    internal OrderedBytes(long value) => (Low, High) = Init(BitConverter.GetBytes(value), value);

    static (int Low, int High) Init<T>(byte[] bytes, T value)
    {
        var (left, right) = ToLeftRightBytes(bytes, value);
        return BitConverter.IsLittleEndian ? (left, right) : (right, left);
    }

    static Span<byte> LeftBytes(byte[] bytes) => bytes.AsSpan()[..(bytes.Length / 2)];
    static Span<byte> RightBytes(byte[] bytes) => bytes.AsSpan()[(bytes.Length / 2)..];

    static (int Left, int Right) ToLeftRightBytes<T>(byte[] bytes, T value) => value switch
    {
        int { } => (BitConverter.ToInt16(LeftBytes(bytes)), BitConverter.ToInt16(RightBytes(bytes))),
        long { } => (BitConverter.ToInt32(LeftBytes(bytes)), BitConverter.ToInt32(RightBytes(bytes))),
        _ => throw new NotImplementedException()
    };
    #endregion


    [SupportedOSPlatform("windows")]
    internal NativeOverlapped AsNativeOverlapped => new() { OffsetLow = Low, OffsetHigh = High };
}
