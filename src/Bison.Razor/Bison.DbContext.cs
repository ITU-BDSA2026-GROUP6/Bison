using Bison.Razor.Models;
using Microsoft.EntityFrameworkCore;

public class BisonDbContext : DbContext
{
    public DbSet<Observation> Observations { get; set; }
    public DbSet<User> Users { get; set; }

    public BisonDbContext(DbContextOptions<BisonDbContext> options)
        : base(options)
    {
    }
}