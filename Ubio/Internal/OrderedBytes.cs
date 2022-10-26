//MIT License

//Copyright (c) 2022 9ualaBanana

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

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
