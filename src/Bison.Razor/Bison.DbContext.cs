using Bison.Razor.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
public class BisonDbContext : IdentityDbContext<BisonUser>
{
    public DbSet<Post> Posts { get; set; }
    public DbSet<Observation> Observations { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Proposal> Proposals { get; set; }
    public DbSet<Taxon> Taxons { get; set; }
    public DbSet<Author> Authors { get; set; }

    public BisonDbContext(DbContextOptions<BisonDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // A tabel pr class tabel
        modelBuilder.Entity<Post>().UseTptMappingStrategy();
    }
}