using Microsoft.Extensions.DependencyInjection;
using Tools.Regex.Abstractions;
using Tools.Regex.Services;

namespace Tools.Regex
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRegexTools(this IServiceCollection services)
        {
            services.AddSingleton<IRegexService, RegexService>();
            
            return services;
        }
    }
}
