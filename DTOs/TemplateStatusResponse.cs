
using System.Text.Json.Serialization;

namespace backend.DTOs;

public class TemplateStatusResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}