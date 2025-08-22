using Microsoft.Extensions.DependencyInjection;
using UMA.Application.Interfaces;
using UMA.Application.Services;

namespace UMA.Application.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }

}
