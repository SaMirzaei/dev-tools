using Microsoft.Extensions.DependencyInjection;
using Tools.Regex.Abstractions;
using Tools.Regex.Services;

namespace Tools.Regex
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSaeidRegexTools(this IServiceCollection services)
        {
            services.AddSingleton<IRegexService, RegexService>();
            return services;
        }
    }
}
