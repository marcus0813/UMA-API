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
                    ID = new Guid("FA7CFBB5-911A-418D-A39D-101DBD10C00B"),
                    FirstName = "Marcus",
                    LastName = "Kok",
                    Email = "marcus.kok@email.com",
                    Password = _passwordHasherService.Hash("marcuskok123"),
                    CreatedAt = DateTime.UtcNow,
                }, new User {

                    ID = new Guid("72F04621-E341-453D-B596-7427DA8BDD98"),
                    FirstName = "Alfred",
                    LastName = "Kok",
                    Email = "alfred.kok@email.com",
                    Password = _passwordHasherService.Hash("alfredkok123"),
                    CreatedAt = DateTime.UtcNow,
                });
            });

        }
    }
}