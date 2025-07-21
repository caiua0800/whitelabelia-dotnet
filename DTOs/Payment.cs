using System;
using System.Text.Json.Serialization;

namespace backend.DTOs;

public class Payment
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("status_detail")]
    public string StatusDetail { get; set; }

    [JsonPropertyName("payment_method_id")]
    public string PaymentMethodId { get; set; }

    // Para PIX
    [JsonPropertyName("point_of_interaction")]
    public PointOfInteraction PointOfInteraction { get; set; }

    // Para Boleto
    [JsonPropertyName("transaction_details")]
    public TransactionDetails TransactionDetails { get; set; }
}

public class PointOfInteraction
{
    [JsonPropertyName("transaction_data")]
    public PixTransactionData TransactionData { get; set; }
}

public class PixTransactionData
{
    [JsonPropertyName("qr_code")]
    public string QrCode { get; set; }

    [JsonPropertyName("qr_code_base64")]
    public string QrCodeBase64 { get; set; }

    [JsonPropertyName("ticket_url")]
    public string TicketUrl { get; set; }
}

