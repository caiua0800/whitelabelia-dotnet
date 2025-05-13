using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using backend.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backend.Interfaces;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace backend.Models
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Enterprise> Enterprises { get; set; }
        public DbSet<Address> EnterpriseAddresses { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionType> SubscriptionTypes { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Agent> Agents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== Enterprise =====
            modelBuilder.Entity<Enterprise>(entity =>
            {
                entity.ToTable("enterprises");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
            });

            modelBuilder.Entity<Agent>(entity =>
            {
                entity.ToTable("agents");

                entity.HasKey(a => a.Id);
                entity.Property(a => a.Id).HasColumnName("id");
                entity.Property(a => a.Number)
                    .HasColumnType("text")
                    .IsRequired();
                entity.Property(a => a.EnterpriseId).HasColumnName("enterprise_id");
                entity.Property(a => a.Prompt).HasColumnName("prompt");
            });

            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("enterprise_addresses");  // Tabela para o endereço
                entity.HasKey(a => a.Id);  // Chave primária do endereço
                entity.Property(a => a.Id).HasColumnName("address_id").ValueGeneratedOnAdd(); // Coluna de ID gerada automaticamente
                entity.Property(a => a.EnterpriseId).HasColumnName("enterprise_id");
            });

            // ===== ADMIN =====
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("admins");

                entity.Property(e => e.Permissions)
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<AdminPermission>>(v, (JsonSerializerOptions?)null))
                    .Metadata.SetValueComparer(new ValueComparer<List<AdminPermission>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

                entity.Property(e => e.EnterpriseId).HasColumnName("enterprise_id");  // Chave estrangeira corretamente mapeada
                entity.Property(e => e.RefreshToken)  // Mapeamento do RefreshToken
                .HasColumnName("refresh_token")
                .HasMaxLength(500);  // Limite do tamanho do RefreshToken, caso queira limitar

            });

            // ===== SUBSCRIPTION =====
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.ToTable("subscriptions");  // Nome da tabela minúsculo
                entity.HasQueryFilter(s => EF.Property<int>(s, "enterprise_id") == CurrentEnterpriseId);

                entity.Property(s => s.EnterpriseId).HasColumnName("enterprise_id");  // Chave estrangeira corretamente mapeada

                entity.HasOne(s => s.SubscriptionType)
                    .WithMany()
                    .HasForeignKey(s => s.SubscriptionTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== CHAT =====
            modelBuilder.Entity<Chat>(entity =>
            {
                entity.ToTable("chats");

                // Mapeamento explícito PRIMEIRO
                entity.Property(c => c.EnterpriseId)
                    .HasColumnName("enterprise_id")
                    .IsRequired();

                entity.Property(c => c.DateCreated)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                entity.Property(c => c.LastMessageDate)
                    .HasColumnType("timestamp with time zone");
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("messages");
                entity.HasQueryFilter(m => EF.Property<int>(m, "enterprise_id") == CurrentEnterpriseId);
                entity.Property(m => m.Id).HasColumnName("id");
                entity.Property(m => m.ChatId).HasColumnName("chat_id");
                entity.Property(m => m.Text).HasColumnName("text");
                entity.Property(m => m.DateCreated).HasColumnName("date_created");
                entity.Property(m => m.EnterpriseId).HasColumnName("enterprise_id");
                entity.Property(m => m.IsRead).HasColumnName("is_read").HasDefaultValue(false);
                entity.Property(m => m.IsReply).HasColumnName("is_reply").HasDefaultValue(false);
                entity.Property(m => m.MessageType).HasColumnName("message_type").HasDefaultValue(1);
            });
        }

        private int? CurrentEnterpriseId
        {
            get
            {
                var enterprise = _httpContextAccessor.HttpContext?.Items["CurrentEnterprise"] as Enterprise;
                return enterprise?.Id;
            }
        }

        public override int SaveChanges()
        {
            SetTenantIds();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTenantIds();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetTenantIds()
        {
            var enterpriseId = CurrentEnterpriseId;
            if (enterpriseId == null) return;

            foreach (var entry in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added && e.Entity is IHasEnterpriseId))
            {
                var entity = (IHasEnterpriseId)entry.Entity;
                if (entity.EnterpriseId == 0)
                {
                    entity.EnterpriseId = enterpriseId.Value;
                }
            }
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Adicione isso antes do base.OnModelCreating
            configurationBuilder.Properties<DateTime>()
                .HaveConversion<DateTimeToUtcConverter>();
        }

        public class DateTimeToUtcConverter : ValueConverter<DateTime, DateTime>
        {
            public DateTimeToUtcConverter()
                : base(v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                       v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
            { }
        }
    }
}
