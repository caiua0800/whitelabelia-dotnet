
using System.Text.Json.Serialization;
using backend.Models;

public class CreateSaleDto
{
    [JsonPropertyName("sale")]
    public Sale Sale { get; set; }

    [JsonPropertyName("client")]
    public ClientInfo Client { get; set; }

    [JsonPropertyName("enterprise_id")]
    public int? EnterpriseId { get; set; }
}

public class ClientInfo
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("number")]
    public string? Number { get; set; }
}