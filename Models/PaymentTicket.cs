using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using backend.Interfaces;

namespace backend.Models;

public class PaymentTicket
{

    [JsonPropertyName("payment_method")]
    public string? PaymentMethod { get; set; }

    [JsonPropertyName("pix")]
    public Pix? Pix { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }
}

public class Pix{
    [JsonPropertyName("ticket")]
    public string? Ticket { get; set; }

    [JsonPropertyName("qr_code")]
    public string? QrCode { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }
}