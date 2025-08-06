using Microsoft.EntityFrameworkCore;

using Auth.Core.Entities;

namespace Auth.Infrastructure.Data
{
    public class AppDbContext : DbContext 
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<User> Users { get; set; }
    }
}