using Microsoft.EntityFrameworkCore;
using UMA.Domain.Entities;
using UMA.Domain.Services;

namespace UMA.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        private readonly IPasswordHasherService _passwordHasherService;

        public AppDbContext(DbContextOptions<AppDbContext> options, IPasswordHasherService passwordHasherService) : base(options)
        {
            _passwordHasherService = passwordHasherService;
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            base.OnModelCreating(modelBuilder);

            _ = modelBuilder.Entity<User>(builder =>
            {
                builder.HasData(new User
                {
                    ID = Guid.NewGuid(),
                    FirstName = "Marcus",
                    LastName = "Kok",
                    Email = "marcus.kok@email.com",
                    Password = _passwordHasherService.Hash("marcuskok123"),
                    CreatedAt = DateTime.UtcNow,
                });
            });

        }
    }
}