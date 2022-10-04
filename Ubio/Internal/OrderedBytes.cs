using System.Runtime.Versioning;

namespace Ubio.Internal;

[SupportedOSPlatform("windows")]
internal readonly struct OrderedBytes
{
    internal int Low { get; init; }
    internal int High { get; init; }
    internal NativeOverlapped AsNativeOverlapped { get; init; }


    #region Initialization
    internal OrderedBytes(int value) => (Low, High, AsNativeOverlapped) = Init(BitConverter.GetBytes(value), value);

    internal OrderedBytes(long value) => (Low, High, AsNativeOverlapped) = Init(BitConverter.GetBytes(value), value);

    static (int Low, int High, NativeOverlapped AsNativeOverlapped) Init<T>(byte[] bytes, T value)
    {
        var (left, right) = ToLeftRightBytes(bytes, value);
        var (low, high) = BitConverter.IsLittleEndian ? (left, right) : (right, left);
        var asNativeOverlapped = new NativeOverlapped() { OffsetLow = low, OffsetHigh = high };

        return (low, high, asNativeOverlapped);
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
}
