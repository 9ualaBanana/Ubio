
# Ubio

Ubio is a .NET library designed for file manipulation with OS-level read/write buffering disabled. This functionality is particularly useful for developers requiring direct and unbuffered file I/O operations, ensuring that data is written to or read from storage devices without intermediate buffering by the operating system.

## Features

- **Unbuffered File Access**: Perform file read and write operations with OS-level buffering disabled, allowing for direct interaction with storage hardware.

- **Synchronous and Asynchronous Operations**: Support for both synchronous and asynchronous file I/O methods to accommodate various application requirements.

- **Configurable Buffer Sizes**: Specify custom buffer sizes for read and write operations to optimize performance based on specific use cases.

## Installation

To include Ubio in your project, add the following package reference to your `.csproj` file:

```xml
<PackageReference Include="Ubio" Version="1.0.0" />
```

Alternatively, install via the .NET CLI:

```bash
dotnet add package Ubio --version 1.0.0
```

## Usage

Here's an example of how to use Ubio for unbuffered file writing and reading:

```csharp
using Ubio;

// Writing to a file with unbuffered access
string filePath = "example.txt";
byte[] dataToWrite = Encoding.UTF8.GetBytes("Hello, Ubio!");

using (var unbufferedWriter = new UnbufferedFileStream(filePath, FileMode.Create, FileAccess.Write))
{
    await unbufferedWriter.WriteAsync(dataToWrite, 0, dataToWrite.Length);
}

// Reading from a file with unbuffered access
byte[] buffer = new byte[1024];

using (var unbufferedReader = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.Read))
{
    int bytesRead = await unbufferedReader.ReadAsync(buffer, 0, buffer.Length);
    string content = Encoding.UTF8.GetString(buffer, 0, bytesRead);
    Console.WriteLine(content);
}
```

In this example, `UnbufferedFileStream` is a custom stream class provided by Ubio that facilitates unbuffered file access. Replace `"example.txt"` with your target file path and adjust buffer sizes as needed.

## Contributing

Contributions are welcome! Feel free to open issues or submit pull requests to enhance the functionality of Ubio.

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/9ualaBanana/Ubio/blob/main/LICENSE) file for details.

For more information and to access the source code, visit the [Ubio GitHub repository](https://github.com/9ualaBanana/Ubio).
