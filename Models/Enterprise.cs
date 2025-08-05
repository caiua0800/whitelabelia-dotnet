using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("enterprises")]
public class Enterprise
{

    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    public required string Name { get; set; }

    [Required]
    [Column("cnpj")]
    public required string Cnpj { get; set; }

    [Column("subdomain")]
    public string? Subdomain { get; set; }

    [Required]
    [Column("contact")]
    public required string Contact { get; set; }

    [Required]
    [Column("email")]
    public required string Email { get; set; }

    [Required]
    [Column("date_created")]
    public DateTime? DateCreated { get; set; }

    [Column("subscription_id")]
    public int? SubscriptionId { get; set; }

    [Column("domain")]
    public string? Domain { get; set; }

    [Required]
    [Column("status")]
    public int? Status { get; set; }

    [Column("primary_color")]
    public string? PrimaryColor { get; set; } = "#4f46e5";

    [Column("secondary_color")]
    public string? SecondaryColor { get; set; } = "#6366f1";

    [Column("logo_url")]
    public string? LogoUrl { get; set; }

    [Column("favicon_url")]
    public string? FaviconUrl { get; set; }

    [Column("brand_name")]
    public string? BrandName { get; set; }
}