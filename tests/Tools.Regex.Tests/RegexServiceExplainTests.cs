using Tools.Regex.Abstractions;
using Tools.Regex.Services;

namespace Tools.Regex.Tests
{
    public class RegexServiceExplainTests
    {
        private readonly IRegexService _service = new RegexService();

        [Fact]
        public void Explain_ReturnsPatternAndFlags()
        {
            var result = _service.Explain(@"\d+", "i");

            Assert.Equal(@"\d+", result.Pattern);
            Assert.Equal("i", result.Flags);
        }

        [Fact]
        public void Explain_DetectsKnownTokens()
        {
            var result = _service.Explain(@"\d", "");

            Assert.Contains(result.Explanations, e => e.Contains("\\d") && e.Contains("Digit"));
        }

        [Fact]
        public void Explain_PatternWithAllTokens_ProducesManyExplanations()
        {
            // Pattern crafted to literally contain each tokenized construct.
            var pattern = @"^$.\d\D\w\W\s\S*+?{n}{n,}{n,m}[...](...)(?:...)(?=...)(?!...)(?<=...)(?<!...)|";

            var result = _service.Explain(pattern, "");

            // 24 tokens are defined in the service; ensure the bulk are detected.
            Assert.True(result.Explanations.Count >= 20);
            Assert.Contains(result.Explanations, e => e.Contains("Start of line"));
            Assert.Contains(result.Explanations, e => e.Contains("End of line"));
            Assert.Contains(result.Explanations, e => e.Contains("Positive lookahead"));
            Assert.Contains(result.Explanations, e => e.Contains("Negative lookbehind"));
            Assert.Contains(result.Explanations, e => e.Contains("Alternation (OR)"));
        }

        [Fact]
        public void Explain_PatternWithNoKnownTokens_ReturnsNoExplanations()
        {
            var result = _service.Explain("abc", "");

            Assert.Empty(result.Explanations);
        }

        [Fact]
        public void Explain_ShortPattern_SummaryMentionsOptionsAndLength()
        {
            var result = _service.Explain("abc", "i");

            Assert.Contains("Options:", result.Summary);
            Assert.Contains("ignoreCase(i)", result.Summary);
            Assert.Contains("Pattern length: 3", result.Summary);
        }

        [Fact]
        public void Explain_NoFlags_SummaryReportsNoneOptions()
        {
            var result = _service.Explain("abc", "");

            Assert.Contains("Options: none", result.Summary);
        }

        [Fact]
        public void Explain_LongPattern_SummaryReportsLength()
        {
            var pattern = new string('a', 300);

            var result = _service.Explain(pattern, "");

            Assert.Contains("Pattern length: 300", result.Summary);
        }
    }
}
