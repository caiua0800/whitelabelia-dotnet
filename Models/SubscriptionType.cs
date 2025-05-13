using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("subscription_types")]
public class SubscriptionType
{

    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    public required int Name { get; set; }

    [Required]
    [Column("value")]
    public required int Value { get; set; }

    [Required]
    [Column("implantation_value")]
    public required double ImplantationValue { get; set; }

    [Required]
    [Column("duration")]
    public required int? Duration { get; set; }

    [Required]
    [Column("date_created")]
    public DateTime? DateCreated { get; set; }

    [Required]
    [Column("enterprise_id")]
    public int EnterpriseId { get; set; }
}