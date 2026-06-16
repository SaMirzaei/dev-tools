# Tools.Regex

A small, dependency-free .NET library for **testing** and **explaining** regular expressions.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../../LICENSE)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.1-512BD4.svg)](https://dotnet.microsoft.com/)

## Features

- **Test** a regex pattern against input text and get rich, structured results:
  - All matches with index, length, and value
  - Capture groups — both **named** and **numbered** — with success state
  - The applied options in a human-readable form
  - Execution time and validation/timeout status
- **Explain** a pattern with a lightweight, heuristic breakdown of common regex constructs.
- **Safe by design**: every evaluation runs with a configurable timeout to protect against catastrophic backtracking.
- **No external dependencies** — built on `System.Text.RegularExpressions`.

## Installation

Add a project reference to the library:

```bash
dotnet add reference ../Tools.Regex/Tools.Regex.csproj
```

Or include it in your solution and reference it from your project.

## Usage

### Testing a pattern

```csharp
using Tools.Regex;
using Tools.Regex.Models;

IRegexService service = new RegexService();

var response = service.Test(new RegexTestRequest
{
    Pattern = @"(?<year>\d{4})-(?<month>\d{2})",
    Text = "2026-06 and 2025-12",
    Flags = "i",      // optional
    TimeoutMs = 200,  // optional (clamped to 50–2000)
    MaxMatches = 200  // optional (clamped to 1–1000)
});

if (response.IsValid)
{
    Console.WriteLine($"Applied options: {response.AppliedOptions}");
    Console.WriteLine($"Total matches: {response.TotalMatches}");

    foreach (var match in response.Matches)
    {
        Console.WriteLine($"{match.Value} @ {match.Index} (len {match.Length})");

        foreach (var group in match.Groups)
        {
            if (group.Success)
            {
                Console.WriteLine($"  group '{group.Name}' = {group.Value}");
            }
        }
    }
}
else if (response.TimedOut)
{
    Console.WriteLine("Pattern timed out.");
}
else
{
    Console.WriteLine($"Invalid pattern: {response.Error}");
}
```

### Explaining a pattern

```csharp
using Tools.Regex;

IRegexService service = new RegexService();

var explanation = service.Explain(@"^\d{4}-\d{2}$", "im");

Console.WriteLine(explanation.Summary);
foreach (var line in explanation.Explanations)
{
    Console.WriteLine(line);
}
```

## API

### `IRegexService`

| Method | Description |
| --- | --- |
| `RegexTestResponse Test(RegexTestRequest request)` | Runs the pattern against the input text and returns matches and metadata. |
| `RegexExplainResponse Explain(string pattern, string flags)` | Returns a heuristic, human-readable explanation of the pattern. |

### Supported flags

Flags are passed as a string (for example `"ims"`). Unknown characters are ignored.

| Flag | Option | Meaning |
| --- | --- | --- |
| `i` | `IgnoreCase` | Case-insensitive matching |
| `m` | `Multiline` | `^` and `$` match at line boundaries |
| `s` | `Singleline` | `.` matches newline characters (dot-all) |
| `x` | `IgnorePatternWhitespace` | Ignore unescaped whitespace in the pattern |
| `n` | `ExplicitCapture` | Only named/numbered groups capture |
| `r` | `RightToLeft` | Match from right to left |

### Request model — `RegexTestRequest`

| Property | Type | Default | Notes |
| --- | --- | --- | --- |
| `Pattern` | `string` | `""` | The regular expression pattern. |
| `Text` | `string` | `""` | The input text to match against. |
| `Flags` | `string` | `""` | Combination of supported flag characters. |
| `TimeoutMs` | `int` | `200` | Match timeout in milliseconds (clamped to 50–2000). |
| `MaxMatches` | `int` | `200` | Maximum matches returned (clamped to 1–1000). |

### Response model — `RegexTestResponse`

| Property | Type | Notes |
| --- | --- | --- |
| `IsValid` | `bool` | `true` when the pattern compiled and executed successfully. |
| `TimedOut` | `bool` | `true` when execution exceeded the timeout. |
| `Error` | `string?` | Error message when the pattern is invalid or timed out. |
| `AppliedOptions` | `string` | Human-readable description of the applied options. |
| `TotalMatches` | `int` | Total number of matches found in the text. |
| `Matches` | `List<RegexMatchDto>` | The returned matches (up to `MaxMatches`). |
| `ElapsedMs` | `long` | Execution time in milliseconds. |

Each `RegexMatchDto` contains `Index`, `Length`, `Value`, and a list of `RegexGroupDto` (`Name`, `Value`, `Index`, `Length`, `Success`).

### Explain response — `RegexExplainResponse`

| Property | Type | Notes |
| --- | --- | --- |
| `Pattern` | `string` | The original pattern. |
| `Flags` | `string` | The original flags. |
| `Explanations` | `List<string>` | One entry per recognized construct found in the pattern. |
| `Summary` | `string` | Applied options and pattern length. |

> **Note:** `Explain` is a lightweight, heuristic tokenizer for common constructs. It is intended as a quick aid, not a full grammar-based parser.

## Target framework

- `netstandard2.1` — usable from .NET Core 3.x, .NET 5+, and Xamarin/Mono.

## Testing

Unit tests live in [`tests/Tools.Regex.Tests`](../../tests/Tools.Regex.Tests) and provide 100% line and branch coverage.

```bash
dotnet test ../../dev-tools.slnx
```

## License

Licensed under the [MIT License](../../LICENSE).
