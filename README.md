# dev-tools

A collection of free, lightweight, dependency-light developer tools — built to be reused as plain .NET libraries.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-net10.0-512BD4.svg)](https://dotnet.microsoft.com/)

## Overview

This repository hosts a growing set of small, focused tools that solve common developer tasks. Each tool is a self-contained library with its own unit tests, so you can pick only what you need.

## Tools

Each tool is documented in its own README. Click a tool below for details, usage, and API reference.

| Tool | Description | Target Framework | Docs |
| --- | --- | --- | --- |
| `Tools.Base64` | Encode, decode, and validate Base64 input with text/file-aware responses and MIME-type hints. | `netstandard2.1` | [README](src/Tools.Base64/README.md) |
| `Tools.Regex` | Test regular expressions against input text and get a lightweight, human-readable explanation of a pattern. | `netstandard2.1` | [README](src/Tools.Regex/README.md) |

> More tools are on the way. See [Roadmap](#roadmap).

## Repository structure

```text
dev-tools/
├── dev-tools.slnx          # Solution file
├── src/                    # Tool libraries
│   ├── Tools.Base64/       # Base64 encoding/decoding/validation tool
│   └── Tools.Regex/        # Regex testing & explanation tool
├── tests/                  # Unit tests (xUnit)
│   ├── Tools.Base64.Tests/
│   └── Tools.Regex.Tests/
├── samples/                # Usage samples
├── LICENSE
└── README.md
```

## Getting started

### Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) or later

### Build

```bash
dotnet build dev-tools.slnx
```

### Test

```bash
dotnet test dev-tools.slnx
```

### Test with code coverage

```bash
dotnet test dev-tools.slnx --collect:"XPlat Code Coverage"
```

## Contributing

Contributions are welcome! When adding a new tool:

1. Create the library under `src/Tools.<Name>/`.
2. Add a matching test project under `tests/Tools.<Name>.Tests/`.
3. Register both projects in [`dev-tools.slnx`](dev-tools.slnx).
4. Keep tools dependency-light and well covered by tests.
5. Add a dedicated `README.md` inside the tool's folder (`src/Tools.<Name>/README.md`).
6. Add a row for the tool in the [Tools](#tools) table above, linking to its README.

## Roadmap

- [ ] Additional developer tools (JSON, encoding/decoding, formatting, etc.)
- [ ] Usage samples under `samples/`

## License

Licensed under the [MIT License](LICENSE).
