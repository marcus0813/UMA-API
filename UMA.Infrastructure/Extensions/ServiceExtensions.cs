using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using UMA.Application.Services;
using UMA.Application.Interfaces;
using UMA.Domain.Repositories;
using UMA.Domain.Services;
using UMA.Infrastructure.Persistence;
using UMA.Infrastructure.Persistence.Repositories;
using UMA.Infrastructure.Services;
using Serilog.Events;

namespace UMA.Infrastructure.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        { 
            Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Information) 
                            .Enrich.FromLogContext()
                            .WriteTo.Logger(
                                lc => lc.Filter.ByIncludingOnly(e => e.Properties.ContainsKey("RequestPath"))
                                    .WriteTo.File(
                                        path: "logs/http-requests.log",
                                        rollingInterval: RollingInterval.Day,
                                        retainedFileCountLimit: 7
                                    )
                            )

                            .WriteTo.Logger(
                                lc => lc.Filter.ByIncludingOnly(e => e.Exception != null || e.Level == LogEventLevel.Error)
                                    .WriteTo.File(
                                        path: "logs/exceptions.log",
                                        rollingInterval: RollingInterval.Day,
                                        retainedFileCountLimit: 7
                                    )
                            )
                            .CreateLogger();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddHttpContextAccessor();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey= true
                };
            });

            services.AddAuthorization();

            services.AddSingleton<IPasswordHasherService, PasswordHasherService>();

            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAzureBlobStorageService, AzureBlobStorageService>();

            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }

}
