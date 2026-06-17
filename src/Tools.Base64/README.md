# Tools.Base64

A small, dependency-light .NET library for **encoding**, **decoding**, and **validating** Base64 data.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../../LICENSE)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.1-512BD4.svg)](https://dotnet.microsoft.com/)

## Features

- **Encode** plain text as Base64.
- **Decode** Base64 back into UTF-8 text, or preserve it for binary/file-oriented workflows.
- **Validate** Base64 input and get structured metadata:
  - Valid/invalid state
  - Decoded size in bytes
  - Heuristic MIME type detection for common file signatures
- **Whitespace-tolerant** decoding and validation: spaces and line breaks are ignored.
- **File-aware** requests and responses with optional filename, MIME type, and file size metadata.
- **No heavy dependencies** — built on the .NET base class library.

## Installation

Add a project reference to the library:

```bash
dotnet add reference ../Tools.Base64/Tools.Base64.csproj
```

Or include it in your solution and reference it from your project.

## Usage

### Encoding text

```csharp
using Tools.Base64;
using Tools.Base64.Models;

IBase64Service service = new Base64Service();

var response = service.Encode(new Base64EncodeRequest
{
    Input = "Hello, Base64!"
});

if (response.Success)
{
    Console.WriteLine(response.Output);
}
else
{
    Console.WriteLine($"Encoding failed: {response.Error}");
}
```

### Decoding Base64

```csharp
using Tools.Base64;
using Tools.Base64.Models;

IBase64Service service = new Base64Service();

var response = service.Decode(new Base64DecodeRequest
{
    Input = "SGVsbG8sIEJhc2U2NCE=",
    OutputAsFile = false
});

if (response.Success && response.IsTextOutput)
{
    Console.WriteLine(response.Output);
}
else if (response.Success)
{
    Console.WriteLine($"Binary content detected: {response.MimeType} ({response.FileSize} bytes)");
}
else
{
    Console.WriteLine($"Decoding failed: {response.Error}");
}
```

### Validating Base64

```csharp
using Tools.Base64;
using Tools.Base64.Models;

IBase64Service service = new Base64Service();

var validation = service.Validate(new Base64ValidateRequest
{
    Input = "SGVsbG8sIEJhc2U2NCE="
});

if (validation.IsValid)
{
    Console.WriteLine($"Decoded size: {validation.DecodedSize} bytes");
    Console.WriteLine($"Estimated MIME type: {validation.EstimatedMimeType}");
}
else
{
    Console.WriteLine($"Invalid Base64: {validation.Error}");
}
```

### Dependency injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using Tools.Base64;

var services = new ServiceCollection();
services.AddBase64Tools();
```

## API

### `IBase64Service`

| Method | Description |
| --- | --- |
| `Base64Response Encode(Base64EncodeRequest request)` | Encodes plain text to Base64, or validates and returns file-oriented Base64 input. |
| `Base64Response Decode(Base64DecodeRequest request)` | Decodes Base64 and returns either UTF-8 text or binary/file metadata. |
| `Base64ValidateResponse Validate(Base64ValidateRequest request)` | Validates Base64 input and returns decoded-size and MIME-type hints. |

### Request model — `Base64EncodeRequest`

| Property | Type | Default | Notes |
| --- | --- | --- | --- |
| `Input` | `string` | `""` | The text to encode, or Base64 file content when `IsFile` is `true`. |
| `IsFile` | `bool` | `false` | When `true`, the service treats `Input` as Base64 file data and validates it instead of re-encoding text. |
| `FileName` | `string?` | `null` | Optional original filename to carry through the response. |
| `MimeType` | `string?` | `null` | Optional MIME type to carry through the response. |

### Request model — `Base64DecodeRequest`

| Property | Type | Default | Notes |
| --- | --- | --- | --- |
| `Input` | `string` | `""` | The Base64 string to decode. Whitespace and newlines are ignored. |
| `OutputAsFile` | `bool` | `false` | When `true`, returns file-oriented output metadata instead of attempting UTF-8 text output. |

### Response model — `Base64Response`

| Property | Type | Notes |
| --- | --- | --- |
| `Success` | `bool` | `true` when the operation completed successfully. |
| `Output` | `string?` | Encoded Base64, decoded text, or preserved Base64 depending on the operation. |
| `Error` | `string?` | Error message when the operation fails, or a binary-content hint during decode. |
| `FileName` | `string?` | Optional filename metadata. |
| `MimeType` | `string?` | Provided or detected MIME type for file/binary content. |
| `FileSize` | `int?` | Estimated or decoded file size in bytes when applicable. |
| `IsTextOutput` | `bool` | `true` when `Output` contains decoded text. |

### Request model — `Base64ValidateRequest`

| Property | Type | Default | Notes |
| --- | --- | --- | --- |
| `Input` | `string` | `""` | The Base64 string to validate. |

### Validate response — `Base64ValidateResponse`

| Property | Type | Notes |
| --- | --- | --- |
| `IsValid` | `bool` | `true` when the input is valid Base64. |
| `Error` | `string?` | Error message when validation fails. |
| `DecodedSize` | `int?` | The decoded payload size in bytes. |
| `EstimatedMimeType` | `string?` | Heuristic MIME type inferred from common file signatures. |

### MIME type detection

The library includes heuristic detection for several common payload types, including:

- PNG, JPEG, GIF, WebP, SVG, TIFF, BMP
- PDF, ZIP, Office/OpenXML, legacy Office formats
- MP3, OGG, WAV, MP4
- GZip and RAR archives

If no known signature matches, the fallback MIME type is `application/octet-stream`.

## Target framework

- `netstandard2.1` — usable from .NET Core 3.x, .NET 5+, and Xamarin/Mono.

## Testing

There is currently no dedicated `Tools.Base64` test project in this repository. A basic validation step is to build the solution:

```bash
dotnet build ../../dev-tools.slnx
```

## License

Licensed under the [MIT License](../../LICENSE).
