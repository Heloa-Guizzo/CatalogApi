using CatalogAPI.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Game> Games => Set<Game>();

        public DbSet<UserLibrary> Libraries =>Set<UserLibrary>();
    }
}
