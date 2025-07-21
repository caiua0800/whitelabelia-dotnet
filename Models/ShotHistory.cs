using System.Text.Json.Serialization;
using backend.Models;

public class ShotHistory
{
    [JsonPropertyName("date_sent")]
    public DateTime DateSent { get; set; }

    [JsonPropertyName("clients_qtt")]
    public int ClientsQtt { get; set; }

    [JsonPropertyName("sent_clients")]
    public List<ClientShotDto>? SentClients { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; } = 1;
}