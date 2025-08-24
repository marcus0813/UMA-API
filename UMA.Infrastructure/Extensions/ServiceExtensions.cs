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
            //Serilog Configuration 
            Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) 
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

            //Integrate with Database
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            //To get claim from the token on request header
            services.AddHttpContextAccessor();

            //JWT Configuration, checking the claims based on configured claims from jwTokenService
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

            //Authorized from anonymous API calls
            services.AddAuthorization();

            //Register custom services and repo for DI
            services.AddSingleton<IPasswordHasherService, PasswordHasherService>();

            services.AddScoped<IJwTokenService, JwTokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAzureBlobStorageService, AzureBlobStorageService>();

            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }

}
