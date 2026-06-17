using Microsoft.Extensions.DependencyInjection;
using Tools.Base64.Abstractions;
using Tools.Base64.Services;

namespace Tools.Base64
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBase64Tools(this IServiceCollection services)
        {
            services.AddSingleton<IBase64Service, Base64Service>();
            
            return services;
        }
    }
}
