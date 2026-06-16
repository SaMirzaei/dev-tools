using System.Collections.Generic;

namespace Tools.Regex.Models
{
    public class RegexTestResponse
    {
        public bool IsValid { get; set; }

        public bool TimedOut { get; set; }

        public string? Error { get; set; }

        public string AppliedOptions { get; set; } = string.Empty; // human readable flags

        public int TotalMatches { get; set; }

        public List<RegexMatchDto> Matches { get; set; } = new List<RegexMatchDto>();

        public long ElapsedMs { get; set; }
    }

    public class RegexMatchDto
    {
        public int Index { get; set; }

        public int Length { get; set; }

        public string Value { get; set; } = string.Empty;

        public List<RegexGroupDto> Groups { get; set; } = new List<RegexGroupDto>();
    }

    public class RegexGroupDto
    {
        public string Name { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public int Index { get; set; }

        public int Length { get; set; }

        public bool Success { get; set; }
    }

    public class RegexTestRequest
    {
        public string Pattern { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public string Flags { get; set; } = string.Empty; // e.g., gimxs

        public int TimeoutMs { get; set; } = 200;

        public int MaxMatches { get; set; } = 200;
    }

    public class RegexExplainResponse
    {
        public string Pattern { get; set; } = string.Empty;

        public string Flags { get; set; } = string.Empty;

        public List<string> Explanations { get; set; } = new List<string>();

        public string Summary { get; set; } = string.Empty;
    }
}
