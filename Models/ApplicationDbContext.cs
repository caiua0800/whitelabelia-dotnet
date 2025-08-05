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
using backend.DTOs;
using System.Text.Json.Serialization;

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
        public DbSet<AgentPrompt> AgentPrompts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Shot> Shots { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<MessageModel> MessageModels { get; set; }

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
                entity.ToTable("enterprise_addresses");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Id).HasColumnName("address_id").ValueGeneratedOnAdd(); // Coluna de ID gerada automaticamente
                entity.Property(a => a.EnterpriseId).HasColumnName("enterprise_id");
            });

            modelBuilder.Entity<AgentPrompt>(entity =>
            {
                entity.ToTable("agent_prompts");
            });



            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");
                entity.Property(p => p.CategoryNames)
                .HasColumnType("text[]")
                .HasConversion(
                    v => v,
                    v => v,
                    new ValueComparer<string[]>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToArray())
                );
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
            });

            // Add this to your OnModelCreating method in ApplicationDbContext
            modelBuilder.Entity<MessageModel>(entity =>
            {
                entity.ToTable("message_models");

                entity.HasKey(m => m.Id);
                entity.Property(m => m.Id).HasColumnName("id").ValueGeneratedOnAdd();

                entity.Property(m => m.Name)
                    .HasColumnName("name")
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(m => m.EnterpriseId)
                    .HasColumnName("enterprise_id");

                // Configure Header as JSONB
                entity.Property(m => m.Header)
                    .HasColumnName("header")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        }),
                        v => JsonSerializer.Deserialize<HeaderMessageModel>(v, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }),
                        new ValueComparer<HeaderMessageModel>(
                            (c1, c2) => JsonSerializer.Serialize(c1, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            }) == JsonSerializer.Serialize(c2, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            }),
                            c => JsonSerializer.Serialize(c, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            }).GetHashCode(),
                            c => JsonSerializer.Deserialize<HeaderMessageModel>(
                                JsonSerializer.Serialize(c, new JsonSerializerOptions
                                {
                                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                }),
                                new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                }))
                    );

                entity.Property(m => m.Body)
                    .HasColumnName("body")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        }),
                        v => JsonSerializer.Deserialize<BodyMessageModel>(v, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }),
                        new ValueComparer<BodyMessageModel>(
                            (c1, c2) => JsonSerializer.Serialize(c1, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            }) == JsonSerializer.Serialize(c2, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            }),
                            c => JsonSerializer.Serialize(c, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            }).GetHashCode(),
                            c => JsonSerializer.Deserialize<BodyMessageModel>(
                                JsonSerializer.Serialize(c, new JsonSerializerOptions
                                {
                                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                }),
                                new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                }))
                    );

                entity.Property(m => m.DateCreated)
                    .HasColumnName("date_created")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                // Add query filter for multi-tenancy if needed
                entity.HasQueryFilter(m => EF.Property<int>(m, "enterprise_id") == CurrentEnterpriseId);
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

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payments");

                entity.Property(p => p.Id).HasColumnName("id");
                entity.Property(p => p.Status).HasColumnName("status");
                entity.Property(p => p.StatusDetail).HasColumnName("status_detail");
                entity.Property(p => p.PaymentMethodId).HasColumnName("payment_method_id");

                // Alterado para usar os nomes corretos das colunas
                entity.Property(p => p.PointOfInteraction)
                    .HasColumnName("point_of_interaction") // Nome correto da coluna
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<PointOfInteraction>(v, (JsonSerializerOptions)null));

                entity.Property(p => p.TransactionDetails)
                    .HasColumnName("transaction_details") // Nome correto da coluna
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<TransactionDetails>(v, (JsonSerializerOptions)null));
            });
            
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.ToTable("subscriptions");

                entity.OwnsOne(s => s.Ticket, t =>
                {
                    t.Property(x => x.TicketId).HasColumnName("ticket_id");
                    t.Property(x => x.TicketUrl).HasColumnName("ticket_url");
                    t.Property(x => x.QrCode).HasColumnName("qr_code");
                    
                    t.ToJson();
                });
            });
            modelBuilder.Entity<SubscriptionType>(entity =>
            {
                entity.ToTable("subscription_types");
            });

            modelBuilder.Entity<Sale>(entity =>
            {
                entity.ToTable("sales");
            });

            modelBuilder.Entity<SaleItem>(entity =>
            {
                entity.ToTable("sale_items");
            });


            // ===== CHAT =====
            // ===== CHAT =====
            modelBuilder.Entity<Chat>(entity =>
            {
                entity.ToTable("chats");

                entity.Property(c => c.EnterpriseId)
                    .HasColumnName("enterprise_id")
                    .IsRequired();

                entity.Property(c => c.DateCreated)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                entity.Property(c => c.LastMessages)
                    .HasColumnName("last_messages")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            WriteIndented = false
                        }),
                        v => JsonSerializer.Deserialize<List<LastMessageDto>>(v, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }),
                        new ValueComparer<List<LastMessageDto>>(
                            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.ToList()));
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.ToTable("tags");
                entity.Property(c => c.EnterpriseId)
                .HasColumnName("enterprise_id")
                .IsRequired();

                entity.Property(c => c.DateCreated)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            });

            // modelBuilder.Entity<Shot>(entity =>
            // {
            //     entity.ToTable("shots");
            //     entity.Property(c => c.EnterpriseId)
            //         .HasColumnName("enterprise_id")
            //         .IsRequired();

            //     entity.Property(s => s.ShotFields)
            //     .HasColumnName("shot_fields")
            //     .HasColumnType("jsonb")
            //     .HasConversion(
            //         v => JsonSerializer.Serialize(v, new JsonSerializerOptions
            //         {
            //             PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            //             WriteIndented = false
            //         }),
            //         v => JsonSerializer.Deserialize<List<ShotFields>>(v, new JsonSerializerOptions
            //         {
            //             PropertyNameCaseInsensitive = true, // Adicione esta linha
            //             PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            //         }),
            //         new ValueComparer<List<ShotFields>>(
            //             (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            //             c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            //             c => c.ToList()));

            //     entity.Property(s => s.SentClients)
            //     .HasColumnName("sent_clients")
            //     .HasColumnType("jsonb")
            //     .HasConversion(
            //         v => JsonSerializer.Serialize(v, new JsonSerializerOptions
            //         {
            //             PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            //             WriteIndented = false
            //         }),
            //         v => JsonSerializer.Deserialize<List<ClientShotDto>>(v, new JsonSerializerOptions
            //         {
            //             PropertyNameCaseInsensitive = true,
            //             PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            //         }),
            //         new ValueComparer<List<ClientShotDto>>(
            //             (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            //             c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            //             c => c.ToList()));

            //     entity.Property(s => s.ShotHistory)
            //     .HasColumnName("shot_history")
            //     .HasColumnType("jsonb")
            //     .HasConversion(
            //         v => JsonSerializer.Serialize(v, new JsonSerializerOptions
            //         {
            //             PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            //             WriteIndented = false
            //         }),
            //         v => JsonSerializer.Deserialize<List<ShotHistory>>(v, new JsonSerializerOptions
            //         {
            //             PropertyNameCaseInsensitive = true,
            //             PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            //         }),
            //         new ValueComparer<List<ShotHistory>>(
            //             (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            //             c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            //             c => c.ToList()));

            //     entity.OwnsOne(s => s.ShotFilters, sf =>
            //     {
            //         sf.Property(s => s.TagFilterStatus).HasColumnName("tag_filter_status");
            //         sf.Property(s => s.TagFilter).HasColumnName("tag_filter")
            //             .HasColumnType("jsonb")
            //             .HasConversion(
            //                 v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            //                 v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null));

            //         sf.Property(s => s.TypeFilterStatus).HasColumnName("type_filter_status");
            //         sf.Property(s => s.TypeFilter).HasColumnName("type_filter");

            //         sf.Property(s => s.SelectedClientsStatus).HasColumnName("selected_clients_status");
            //         sf.Property(s => s.SelectedClients).HasColumnName("selected_clients")
            //             .HasColumnType("jsonb")
            //             .HasConversion(
            //                 v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            //                 v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null));
            //     });
            // });
            modelBuilder.Entity<Shot>(entity =>
            {
                entity.ToTable("shots");

                entity.Property(s => s.Id).HasColumnName("id");
                entity.Property(s => s.Status).HasColumnName("status").HasDefaultValue(1);
                entity.Property(s => s.DateCreated)
                    .HasColumnName("date_created")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                entity.Property(s => s.ActivationDate)
                    .HasColumnName("activation_date")
                    .HasColumnType("timestamp with time zone");
                entity.Property(s => s.EnterpriseId).HasColumnName("enterprise_id").IsRequired();
                entity.Property(s => s.Name).HasColumnName("name").IsRequired();
                entity.Property(s => s.NameNormalized).HasColumnName("name_normalized");
                entity.Property(s => s.ModelName).HasColumnName("model_name");
                entity.Property(s => s.MessageModelId).HasColumnName("message_model_id");
                entity.Property(s => s.SendShotDate).HasColumnName("send_shot_date").HasColumnType("timestamp with time zone");
                entity.Property(s => s.EndShotDate).HasColumnName("end_shot_date").HasColumnType("timestamp with time zone");
                entity.Property(s => s.ClientsQtt).HasColumnName("clients_qtt");

                entity.Property(s => s.ShotFields)
                    .HasColumnName("shot_fields")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }),
                        v => JsonSerializer.Deserialize<List<ShotFields>>(v, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }));

                entity.Property(s => s.SentClients)
                    .HasColumnName("sent_clients")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }),
                        v => JsonSerializer.Deserialize<List<ClientShotDto>>(v, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }));

                entity.Property(s => s.ShotHistory)
                    .HasColumnName("shot_history")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }),
                        v => JsonSerializer.Deserialize<List<ShotHistory>>(v, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }));

                // Header and Body as JSONB
                entity.Property(s => s.Header)
                    .HasColumnName("header")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }),
                        v => JsonSerializer.Deserialize<ItemHeaderBody>(v, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }));

                entity.Property(s => s.Body)
                    .HasColumnName("body")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }),
                        v => JsonSerializer.Deserialize<ItemHeaderBody>(v, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }));

                // Owned entity for ShotFilters
                entity.OwnsOne(s => s.ShotFilters, sf =>
                {
                    sf.Property(s => s.TagFilterStatus).HasColumnName("tag_filter_status");
                    sf.Property(s => s.TagFilter).HasColumnName("tag_filter")
                        .HasColumnType("jsonb")
                        .HasConversion(
                            v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                            v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions)null));

                    sf.Property(s => s.TypeFilterStatus).HasColumnName("type_filter_status");
                    sf.Property(s => s.TypeFilter).HasColumnName("type_filter");

                    sf.Property(s => s.SelectedClientsStatus).HasColumnName("selected_clients_status");
                    sf.Property(s => s.SelectedClients).HasColumnName("selected_clients")
                        .HasColumnType("jsonb")
                        .HasConversion(
                            v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null));
                });

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
