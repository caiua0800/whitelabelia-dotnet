using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("subscriptions")]
public class Subscription
{

    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("subscription_type_id")]
    public int? SubscriptionTypeId { get; set; }

    [ForeignKey("SubscriptionTypeId")]
    public SubscriptionType? SubscriptionType { get; set; }

    [Required]
    [Column("date_created")]
    public DateTime? DateCreated { get; set; }

    [Required]
    [Column("status")]
    public int? Status { get; set; }

    [Required]
    [Column("enterprise_id")]
    public int EnterpriseId { get; set; }

}