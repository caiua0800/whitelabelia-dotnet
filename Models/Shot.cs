using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using backend.Interfaces;

namespace backend.Models;

public class Shot : IHasEnterpriseId
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("status")]
    public int Status { get; set; } = 1;

    [Column("date_created", TypeName = "timestamp with time zone")]
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    [Column("activation_date", TypeName = "timestamp with time zone")]
    public DateTime? ActivationDate { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("enterprise_id")]
    public int EnterpriseId { get; set; }

    [Column("tags")]
    public List<int>? Tags { get; set; }

    [Column("model_name")]
    public string? ModelName { get; set; }

    [Column("message_model_id")]
    public int? MessageModelId { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; }

    [Column("name_normalized")]
    public string? NameNormalized { get; set; }

    [Column("shot_filters")]
    public ShotFilter? ShotFilters { get; set; }

    [Column("send_shot_date")]
    public DateTime? SendShotDate { get; set; }

    [Column("end_shot_date")]
    public DateTime? EndShotDate { get; set; }

    [Column("clients_qtt")]
    public int? ClientsQtt { get; set; }

    [Column("sent_clients")]
    public List<ClientShotDto>? SentClients { get; set; }

    [Column("shot_fields")]
    public List<ShotFields>? ShotFields { get; set; }

    [Column("shot_history")]
    public List<ShotHistory>? ShotHistory { get; set; }

    [Column("header")]
    public ItemHeaderBody? Header { get; set; }

    [Column("body")]
    public ItemHeaderBody? Body { get; set; }
}


public class ItemHeaderBody
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("params")]
    public List<Param>? Params { get; set; }
}

public class Param
{

    [JsonPropertyName("key")]
    public int? Key { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}