using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Interfaces;

namespace backend.Models;

public class Message : IHasEnterpriseId
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("chat_id")]
    public string? ChatId { get; set; }

    [Required]
    [Column("text")]
    public string Text { get; set; } = string.Empty;

    [Required]
    [Column("date_created")]
    public DateTime DateCreated { get; set; }

    [Required]
    [Column("enterprise_id")]
    public int EnterpriseId { get; set; }

    [Column("agent_number")]
    public string? AgentNumber { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    [Column("message_type")]
    public int MessageType { get; set; } = 1;

    [Column("is_reply")]
    public bool? IsReply { get; set; } = false; 
}
