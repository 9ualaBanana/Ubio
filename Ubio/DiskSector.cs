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

namespace System.IO;

public class DiskSector
{
    public readonly int LogicalSize;
    public readonly int PhysicalSize;
    public bool IsAligned(long fileAccessSize) => fileAccessSize % LogicalSize == 0;


    internal DiskSector(string path)
    {
        (LogicalSize, PhysicalSize) = Kernel32.GetSectorSizes(Path.IsPathRooted(path) ?
            Path.GetPathRoot(path) : null/*current working directory root*/);
    }


    public void EnsureIsAligned(long fileAccessSize)
    {
        if (!IsAligned(fileAccessSize))
            throw new ArgumentOutOfRangeException(
                nameof(fileAccessSize),
                "Unbuffered file access sizes must be for a number of bytes that is an integer multiple of the volume sector size.");
    }
}
