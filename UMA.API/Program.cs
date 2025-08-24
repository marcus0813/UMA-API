using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using UMA.Application.Extensions;
using UMA.Infrastructure.Extensions;
using UMA.Infrastructure.Persistence;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Add CORS services and define a policy
string allowSpecificOrigins = "_allowedSpecificOrigins";
var corsOrigins = builder.Configuration.GetSection("CorsOrigins:Allowed").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowSpecificOrigins,
                      policy =>
                      {
                          // Use the array of strings directly from configuration
                          // The `WithOrigins` method can accept a string array.
                          policy.WithOrigins(corsOrigins)
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

builder.Services.AddSwaggerGen(options =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Enter your JWT Token",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddApplication()
                .AddInfrastructure(builder.Configuration);

builder.Host.UseSerilog();

var app = builder.Build();

try
{
    Console.WriteLine("Applying database migrations...");
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (dbContext.Database.CanConnect())
        {
            Console.WriteLine("Successfully connected to the database!");
        }
        else
        {
            Console.WriteLine("Failed to connect to the database.");
        }
        Console.WriteLine("Database migrations applied successfully!");
    }
}
catch (Exception ex)
{
    Console.WriteLine("An error occurred while applying migrations or connecting to the database.");
    Console.WriteLine(ex.Message);
    // You can also log the full exception here if you have a logger configured
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(allowSpecificOrigins);

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
