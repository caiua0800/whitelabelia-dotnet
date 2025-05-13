using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Interfaces;

namespace backend.Models;

public class Agent
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("number", TypeName = "text")] 
    public string Number { get; set; }

    [Column("enterprise_id")]
    public int EnterpriseId { get; set; }

    [Column("prompt")]
    public string? Prompt { get; set; }
}