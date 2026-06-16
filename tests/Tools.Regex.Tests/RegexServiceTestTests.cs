using System.Linq;
using Tools.Regex.Abstractions;
using Tools.Regex.Models;
using Tools.Regex.Services;

namespace Tools.Regex.Tests
{
    public class RegexServiceTestTests
    {
        private readonly IRegexService _service = new RegexService();

        [Fact]
        public void Test_SimpleMatch_ReturnsMatch()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = @"\d+",
                Text = "abc 123 def"
            });

            Assert.True(resp.IsValid);
            Assert.False(resp.TimedOut);
            Assert.Null(resp.Error);
            Assert.Equal(1, resp.TotalMatches);
            Assert.Single(resp.Matches);

            var match = resp.Matches[0];
            Assert.Equal(4, match.Index);
            Assert.Equal(3, match.Length);
            Assert.Equal("123", match.Value);
            Assert.True(resp.ElapsedMs >= 0);
        }

        [Fact]
        public void Test_MultipleMatches_CountsAll()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = @"\d+",
                Text = "1 22 333 4444"
            });

            Assert.True(resp.IsValid);
            Assert.Equal(4, resp.TotalMatches);
            Assert.Equal(4, resp.Matches.Count);
        }

        [Fact]
        public void Test_NoMatch_ReturnsEmpty()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = "xyz",
                Text = "abc def"
            });

            Assert.True(resp.IsValid);
            Assert.Equal(0, resp.TotalMatches);
            Assert.Empty(resp.Matches);
        }

        [Fact]
        public void Test_NumberedGroups_AreCaptured()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = @"(\d+)-(\d+)",
                Text = "12-34"
            });

            Assert.True(resp.IsValid);
            var match = Assert.Single(resp.Matches);

            // group 0 = whole match, group 1, group 2
            Assert.Equal(3, match.Groups.Count);
            Assert.Equal("0", match.Groups[0].Name);
            Assert.Equal("12-34", match.Groups[0].Value);
            Assert.Equal("1", match.Groups[1].Name);
            Assert.Equal("12", match.Groups[1].Value);
            Assert.Equal("2", match.Groups[2].Name);
            Assert.Equal("34", match.Groups[2].Value);
            Assert.True(match.Groups[1].Success);
        }

        [Fact]
        public void Test_NamedGroup_UsesGroupName()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = @"(?<year>\d{4})",
                Text = "2024"
            });

            Assert.True(resp.IsValid);
            var match = Assert.Single(resp.Matches);
            Assert.Contains(match.Groups, g => g.Name == "year" && g.Value == "2024");
        }

        [Fact]
        public void Test_UnsuccessfulGroup_ReportsSuccessFalse()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = @"a(b)?c",
                Text = "ac"
            });

            Assert.True(resp.IsValid);
            var match = Assert.Single(resp.Matches);
            var optionalGroup = match.Groups[1];
            Assert.False(optionalGroup.Success);
            Assert.Equal(string.Empty, optionalGroup.Value);
        }

        [Fact]
        public void Test_MaxMatches_LimitsReturnedMatchesButCountsTotal()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = "a",
                Text = new string('a', 10),
                MaxMatches = 3
            });

            Assert.True(resp.IsValid);
            Assert.Equal(3, resp.Matches.Count);
            Assert.Equal(10, resp.TotalMatches);
        }

        [Fact]
        public void Test_MaxMatches_BelowMinimum_ClampsToOne()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = "a",
                Text = "aaaa",
                MaxMatches = 0
            });

            Assert.True(resp.IsValid);
            Assert.Single(resp.Matches);
            Assert.Equal(4, resp.TotalMatches);
        }

        [Fact]
        public void Test_MaxMatches_AboveMaximum_ClampsToThousand()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = "a",
                Text = new string('a', 1100),
                MaxMatches = 5000
            });

            Assert.True(resp.IsValid);
            Assert.Equal(1000, resp.Matches.Count);
            Assert.Equal(1100, resp.TotalMatches);
        }

        [Fact]
        public void Test_InvalidPattern_ReturnsError()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = "(",
                Text = "abc"
            });

            Assert.False(resp.IsValid);
            Assert.False(resp.TimedOut);
            Assert.NotNull(resp.Error);
            Assert.NotEmpty(resp.Error!);
        }

        [Fact]
        public void Test_CatastrophicBacktracking_TimesOut()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = @"(a+)+$",
                Text = new string('a', 40) + "!",
                TimeoutMs = 50
            });

            Assert.True(resp.TimedOut);
            Assert.False(resp.IsValid);
            Assert.NotNull(resp.Error);
            Assert.Contains("timed out", resp.Error!, System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Test_TimeoutMs_BelowMinimum_IsClamped()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = @"\d",
                Text = "5",
                TimeoutMs = 1
            });

            Assert.True(resp.IsValid);
        }

        [Fact]
        public void Test_TimeoutMs_AboveMaximum_IsClamped()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = @"\d",
                Text = "5",
                TimeoutMs = 999999
            });

            Assert.True(resp.IsValid);
        }

        [Fact]
        public void Test_NoFlags_AppliedOptionsIsNone()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = "a",
                Text = "a",
                Flags = ""
            });

            Assert.Equal("none", resp.AppliedOptions);
        }

        [Fact]
        public void Test_IgnoreCaseFlag_MatchesDifferentCase()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = "abc",
                Text = "ABC",
                Flags = "i"
            });

            Assert.True(resp.IsValid);
            Assert.Single(resp.Matches);
            Assert.Contains("ignoreCase(i)", resp.AppliedOptions);
        }

        [Fact]
        public void Test_MultilineFlag_MatchesEachLineStart()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = "^a",
                Text = "a\na",
                Flags = "m"
            });

            Assert.True(resp.IsValid);
            Assert.Equal(2, resp.TotalMatches);
            Assert.Contains("multiline(m)", resp.AppliedOptions);
        }

        [Fact]
        public void Test_SinglelineFlag_DotMatchesNewline()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = "a.b",
                Text = "a\nb",
                Flags = "s"
            });

            Assert.True(resp.IsValid);
            Assert.Single(resp.Matches);
            Assert.Contains("dotAll(s)", resp.AppliedOptions);
        }

        [Fact]
        public void Test_IgnorePatternWhitespaceFlag_AllowsSpacing()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = @"\d \d",
                Text = "12",
                Flags = "x"
            });

            Assert.True(resp.IsValid);
            Assert.Single(resp.Matches);
            Assert.Contains("extended(x)", resp.AppliedOptions);
        }

        [Fact]
        public void Test_ExplicitCaptureFlag_IgnoresUnnamedGroups()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = @"(\d)(?<named>\d)",
                Text = "12",
                Flags = "n"
            });

            Assert.True(resp.IsValid);
            var match = Assert.Single(resp.Matches);
            // With ExplicitCapture, only group 0 and the named group are captured.
            Assert.Equal(2, match.Groups.Count);
            Assert.Contains(match.Groups, g => g.Name == "named");
            Assert.Contains("explicitCapture(n)", resp.AppliedOptions);
        }

        [Fact]
        public void Test_RightToLeftFlag_IsApplied()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = @"\d+",
                Text = "12 34",
                Flags = "r"
            });

            Assert.True(resp.IsValid);
            Assert.Contains("rightToLeft(r)", resp.AppliedOptions);
        }

        [Fact]
        public void Test_UnknownFlag_IsIgnored()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = "a",
                Text = "a",
                Flags = "z"
            });

            Assert.True(resp.IsValid);
            Assert.Equal("none", resp.AppliedOptions);
        }

        [Fact]
        public void Test_AllFlagsCombined_DescribesAllOptions()
        {
            var resp = _service.Test(new RegexTestRequest
            {
                Pattern = "a",
                Text = "a",
                Flags = "imsxn"
            });

            Assert.True(resp.IsValid);
            Assert.Contains("ignoreCase(i)", resp.AppliedOptions);
            Assert.Contains("multiline(m)", resp.AppliedOptions);
            Assert.Contains("dotAll(s)", resp.AppliedOptions);
            Assert.Contains("extended(x)", resp.AppliedOptions);
            Assert.Contains("explicitCapture(n)", resp.AppliedOptions);
        }
    }
}
