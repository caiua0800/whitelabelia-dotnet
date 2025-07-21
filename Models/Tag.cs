using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Interfaces;

namespace backend.Models;

public class Tag : IHasEnterpriseId
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("date_created")]
    public DateTime DateCreated { get; set; }

    [Required]
    [Column("enterprise_id")]
    public int EnterpriseId { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("color")]
    public string? Color { get; set; } = "white";

    [Column("background_color")]
    public string? BackgroundColor { get; set; } = "purple";
}
