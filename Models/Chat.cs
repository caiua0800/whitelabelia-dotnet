using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Interfaces;

namespace backend.Models;

// Chat.cs
public class Chat : IHasEnterpriseId
{
    [Key]
    [Column("id")]
    public string Id { get; set; }

    [Required]
    [Column("status")]
    public int Status { get; set; }

    [Column("last_message_text")]
    public string? LastMessageText { get; set; }

    [Column("agent_number")]
    public string? AgentNumber { get; set; }

    [Column("date_created", TypeName = "timestamp with time zone")]
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    [Column("last_message_date", TypeName = "timestamp with time zone")]
    public DateTime? LastMessageDate { get; set; }

    [Column("last_message_is_reply")]
    public bool? LastMessageIsReply { get; set; }

    [Column("last_message_is_seen")]
    public bool? LastMessageIsSeen { get; set; }

    [Required]
    [Column("enterprise_id")]
    public int EnterpriseId { get; set; }

    [Column("custom_prompt")]
    public string? CustomPrompt { get; set; }
}