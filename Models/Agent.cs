using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Interfaces;

namespace backend.Models;

public class Agent
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("number", TypeName = "text")]
    public string Number { get; set; }

    [Column("real_whatsapp_number")]
    public string? RealWhatsappNumber { get; set; }

    [Column("enterprise_id")]
    public int EnterpriseId { get; set; }

    [Column("prompt")]
    public string? Prompt { get; set; }

    [Column("date_created")]
    public DateTime? DateCreated { get; set; }

    [Column("date_updated")]
    public DateTime? DateUpdated { get; set; }
}