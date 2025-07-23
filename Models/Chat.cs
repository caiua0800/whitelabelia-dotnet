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

    [Column("last_messages")]
    public List<LastMessageDto>? LastMessages { get; set; }

    [Column("agent_number")]
    public string? AgentNumber { get; set; }

    [Column("date_created", TypeName = "timestamp with time zone")]
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("enterprise_id")]
    public int EnterpriseId { get; set; }

    [Column("custom_prompt")]
    public string? CustomPrompt { get; set; }

    [Column("client_name")]
    public string? ClientName { get; set; }

    [Column("client_email")]
    public string? ClientEmail { get; set; }

    [Column("client_cpf_cnpj")]
    public string? ClientCpfCnpj { get; set; }

    [Column("client_name_normalized")]
    public string? ClientNameNormalized { get; set; }

    [Column("street")]
    public string? Street { get; set; }
    [Column("number")]
    public string? Number { get; set; }
    [Column("neighborhood")]
    public string? Neighborhood { get; set; }
    [Column("zipcode")]
    public string? Zipcode { get; set; }
    [Column("city")]
    public string? City { get; set; }
    [Column("complement")]
    public string? Complement { get; set; }
    [Column("country")]
    public string? Country { get; set; }
    [Column("state")]
    public string? State { get; set; }
    [Column("tags")]
    public List<int>? Tags { get; set; }
}