using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Tools.Regex.Abstractions;
using Tools.Regex.Models;

namespace Tools.Regex.Services
{

    public class RegexService : IRegexService
    {
        public RegexTestResponse Test(RegexTestRequest request)
        {
            var sw = Stopwatch.StartNew();

            var resp = new RegexTestResponse();

            try
            {
                var options = ParseOptions(request.Flags);
                resp.AppliedOptions = DescribeOptions(options);

                var timeout = TimeSpan.FromMilliseconds(Math.Clamp(request.TimeoutMs, 50, 2000));

                var regex = new System.Text.RegularExpressions.Regex(request.Pattern, options, timeout);

                var matches = regex.Matches(request.Text);
                int count = 0;

                foreach (Match m in matches)
                {
                    if (!m.Success) continue;

                    var dto = new RegexMatchDto
                    {
                        Index = m.Index,
                        Length = m.Length,
                        Value = m.Value
                    };

                    for (int i = 0; i < m.Groups.Count; i++)
                    {
                        var g = m.Groups[i];
                        dto.Groups.Add(new RegexGroupDto
                        {
                            Name = regex.GroupNameFromNumber(i),
                            Value = g.Value,
                            Index = g.Index,
                            Length = g.Length,
                            Success = g.Success
                        });
                    }
                    resp.Matches.Add(dto);
                    count++;

                    if (count >= Math.Clamp(request.MaxMatches, 1, 1000)) break;
                }

                resp.TotalMatches = matches.Count;
                resp.IsValid = true;
            }
            catch (RegexMatchTimeoutException)
            {
                resp.TimedOut = true;
                resp.IsValid = false;
                resp.Error = "Regex execution timed out. Try simplifying the pattern or reducing the input size.";
            }
            catch (ArgumentException ex)
            {
                resp.IsValid = false;
                resp.Error = ex.Message;
            }
            finally
            {
                sw.Stop();
                resp.ElapsedMs = sw.ElapsedMilliseconds;
            }

            return resp;
        }

        public RegexExplainResponse Explain(string pattern, string flags)
        {
            var list = new List<string>();
            var summary = new List<string>();

            // Very lightweight explainer without external AI: tokenize common constructs
            // This is heuristic and safe for server-side.
            var tokens = new (string token, string meaning)[]
            {
                ("^", "Start of line"),
                ("$", "End of line"),
                (".", "Any character except newline (unless Singleline)"),
                ("\\d", "Digit"),
                ("\\D", "Non-digit"),
                ("\\w", "Word character (letter, digit, underscore)"),
                ("\\W", "Non-word character"),
                ("\\s", "Whitespace"),
                ("\\S", "Non-whitespace"),
                ("*", "0 or more (greedy)"),
                ("+", "1 or more (greedy)"),
                ("?", "0 or 1, or makes quantifier lazy if after quantifier"),
                ("{n}", "Exactly n repetitions"),
                ("{n,}", "At least n repetitions"),
                ("{n,m}", "Between n and m repetitions"),
                ("[...]", "Character class"),
                ("(...)", "Capturing group"),
                ("(?:...)", "Non-capturing group"),
                ("(?=...)", "Positive lookahead"),
                ("(?!...)", "Negative lookahead"),
                ("(?<=...)", "Positive lookbehind"),
                ("(?<!...)", "Negative lookbehind"),
                ("|", "Alternation (OR)")
            };

            foreach (var t in tokens)
            {
                if (pattern.Contains(t.token))
                {
                    list.Add($"Contains '{t.token}': {t.meaning}");
                }
            }

            summary.Add(pattern.Length < 256 ? 
                "Pattern is relatively short" : 
                "Pattern is long; ensure performance");

            var opts = ParseOptions(flags);
            var optsDesc = DescribeOptions(opts);

            return new RegexExplainResponse
            {
                Pattern = pattern,
                Flags = flags,
                Explanations = list,
                Summary = $"Options: {optsDesc}. Pattern length: {pattern.Length}"
            };
        }

        private static RegexOptions ParseOptions(string flags)
        {
            var options = RegexOptions.None;

            if (string.IsNullOrEmpty(flags)) return options;

            foreach (var c in flags)
            {
                switch (c)
                {
                    case 'i': options |= RegexOptions.IgnoreCase; break;
                    case 'm': options |= RegexOptions.Multiline; break;
                    case 's': options |= RegexOptions.Singleline; break;
                    case 'x': options |= RegexOptions.IgnorePatternWhitespace; break;
                    case 'n': options |= RegexOptions.ExplicitCapture; break; // only named/numbered groups with ?<name>
                    case 'r': options |= RegexOptions.RightToLeft; break;
                    default: break;
                }
            }

            return options;
        }

        private static string DescribeOptions(RegexOptions options)
        {
            var parts = new List<string>();

            if (options.HasFlag(RegexOptions.IgnoreCase)) parts.Add("ignoreCase(i)");

            if (options.HasFlag(RegexOptions.Multiline)) parts.Add("multiline(m)");

            if (options.HasFlag(RegexOptions.Singleline)) parts.Add("dotAll(s)");

            if (options.HasFlag(RegexOptions.IgnorePatternWhitespace)) parts.Add("extended(x)");

            if (options.HasFlag(RegexOptions.ExplicitCapture)) parts.Add("explicitCapture(n)");

            if (options.HasFlag(RegexOptions.RightToLeft)) parts.Add("rightToLeft(r)");

            return parts.Count == 0 ? "none" : string.Join(", ", parts);
        }
    }

}
