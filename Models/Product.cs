
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Interfaces;

namespace backend.Models;

public class Product : IHasEnterpriseId
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("name_normalized")]
    public string? NameNormalized { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("unity_price")]
    public double UnityPrice { get; set; }

    [Column("date_created")]
    public DateTime? DateCreated { get; set; }

    [Column("status")]
    public int? Status { get; set; } = 1;

   [Column("category_names", TypeName = "text[]")]
    public string[]? CategoryNames { get; set; } = Array.Empty<string>();

    [Column("enterprise_id")]
    public int EnterpriseId { get; set; }
}