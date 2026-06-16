using Tools.Regex.Models;

namespace Tools.Regex.Abstractions
{
    public interface IRegexService
    {
        RegexTestResponse Test(RegexTestRequest request);
        RegexExplainResponse Explain(string pattern, string flags);
    }

}
