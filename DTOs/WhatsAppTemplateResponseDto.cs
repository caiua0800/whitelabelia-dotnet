using System.Text.Json.Serialization;

namespace backend.DTOs;

public class WhatsAppTemplateResponseDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; }
    
    [JsonPropertyName("category")]
    public string Category { get; set; }
    
    [JsonPropertyName("error")]
    public WhatsAppError? Error { get; set; }
}

public class WhatsAppError
{
    [JsonPropertyName("message")]
    public string Message { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("code")]
    public int Code { get; set; }
    
    [JsonPropertyName("error_data")]
    public object ErrorData { get; set; }
}