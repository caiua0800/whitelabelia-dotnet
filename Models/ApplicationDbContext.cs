// Models/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;

namespace backend.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Client> Clients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>()
            .HasKey(c => c.Id); // Define Id como chave primária
            
        modelBuilder.Entity<Client>()
            .Property(c => c.Id)
            .IsRequired();
    }
}