using System.Text.Json.Serialization;

namespace backend.DTOs;

public class MessageModelCreateDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("headerText")]
    public string? HeaderText { get; set; }

    [JsonPropertyName("headerParam")]
    public string? HeaderParam { get; set; }

    [JsonPropertyName("bodyText")]
    public string? BodyText { get; set; }

    [JsonPropertyName("enterprise_id")]
    public int? EnterpriseId { get; set; }

    [JsonPropertyName("footerText")]
    public string? FooterText { get; set; }

    [JsonPropertyName("account_id")]
    public string? AccountId { get; set; } 

    [JsonPropertyName("bodyParams")]
    public List<BodyTextParamsDto>? BodyParams { get; set; }
    
}

public class BodyTextParamsDto
{
    [JsonPropertyName("key")]
    public int? Key { get; set; }

    [JsonPropertyName("param")]
    public string? Param { get; set; }
}