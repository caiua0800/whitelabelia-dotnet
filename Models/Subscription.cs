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

    [Column("date_created")]
    public DateTime? DateCreated { get; set; }

    [Column("date_paid")]
    public DateTime? DatePaid { get; set; }

    [Column("expiration_date")]
    public DateTime? ExpirationDate { get; set; }

    [Column("status")]
    public int? Status { get; set; }

    [Column("enterprise_id")]
    public int EnterpriseId { get; set; }

    [Column("avaliable_shots")]
    public int? AvaliableShots { get; set; }

    [Column("avaliable_users")]
    public int? AvaliableUsers { get; set; }

    [Column("avaliable_start_chats")]
    public int? AvaliableStartChats { get; set; }


}