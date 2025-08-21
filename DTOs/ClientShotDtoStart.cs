using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models;

public class ClientShotDtoStart
{

    [JsonPropertyName("to")]
    public string? To { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("message_type")]
    public string? MessageType { get; set; } = "text";

    [JsonPropertyName("is_from_admin")]
    public bool? IsFromAdmin { get; set; } = true;

    // [JsonPropertyName("agent_number")]
    // public string? AgentNumber { get; set; }

    // [JsonPropertyName("whatsapp_token")]
    // public string? WhatsappToken { get; set; }

}