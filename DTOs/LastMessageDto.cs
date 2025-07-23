
using System.Text.Json.Serialization;
using System;

public class LastMessageDto
{
    [JsonPropertyName("agent_number")]
    public string AgentNumber { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("is_seen")]
    public bool IsSeen { get; set; }

    [JsonPropertyName("is_reply")]
    public bool IsReply { get; set; }

    [JsonPropertyName("date_created")]
    public DateTime DateCreated { get; set; }
}