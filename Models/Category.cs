
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Interfaces;

namespace backend.Models;

public class Category : IHasEnterpriseId
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("name_normalized")]
    public string? NameNormalized { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("date_created")]
    public DateTime DateCreated { get; set; }

    [Column("enterprise_id")]
    public int EnterpriseId { get; set; }
}