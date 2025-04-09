using Microsoft.EntityFrameworkCore;

namespace backend.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Admin> Admins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).IsRequired();
        });

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.ToTable("Admins");
            entity.HasKey(a => a.Email);
            entity.Property(a => a.Email)
                    .IsRequired()
                    .HasMaxLength(255);

            entity.OwnsOne(a => a.Power, p =>
            {
                p.OwnsOne(p => p.Sales);
                p.OwnsOne(p => p.Chats);
                p.OwnsOne(p => p.Users);
            });
        });
    }
}
