using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Models;

public class SignaturePix
{
    [JsonPropertyName("ticket_id")]
    public string? TicketId { get; set; }

    [JsonPropertyName("ticket_url")]
    public string? TicketUrl { get; set; }

    [JsonPropertyName("qr_code")]
    public string? QrCode { get; set; }
}