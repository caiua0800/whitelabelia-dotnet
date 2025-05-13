using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Interfaces;

namespace backend.Models;

public class AgentPrompt : IHasEnterpriseId
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("enterprise_id")]
    public int EnterpriseId { get; set; }

    [Column("text")]
    public string? Text { get; set; }

}