// Admin.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models;

public class MessageModel
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; }

    [Column("enterprise_id")]
    public int? EnterpriseId { get; set; }

    [Column("header")]
    public HeaderMessageModel Header { get; set; }

    [Column("body")]
    public BodyMessageModel Body { get; set; }

    [Column("date_created")]
    public DateTime DateCreated { get; set; }
}

public class HeaderMessageModel
{

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("param")]
    public string? Param { get; set; }

}

public class BodyMessageModel
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("params")]
    public List<BodyTextParams>? Params { get; set; }
}

public class BodyTextParams
{

    [JsonPropertyName("key")]
    public int? Key { get; set; }

    [JsonPropertyName("param")]
    public string? Param { get; set; }
}