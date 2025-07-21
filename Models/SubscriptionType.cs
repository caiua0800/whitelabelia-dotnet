using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class SubscriptionType
{

    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; }

    [Required]
    [Column("value")]
    public double Value { get; set; }

    [Required]
    [Column("implantation_value")]
    public double? ImplantationValue { get; set; }

    [Column("duration")]
    public int? Duration { get; set; }

    [Column("date_created")]
    public DateTime? DateCreated { get; set; }

    [Column("shots_qtt")]
    public int? ShotsQtt { get; set; }

    [Column("users_qtt")]
    public int? UsersQtt { get; set; }

    [Column("start_chats_qtt")]
    public int? StartChatsQtt { get; set; }

}