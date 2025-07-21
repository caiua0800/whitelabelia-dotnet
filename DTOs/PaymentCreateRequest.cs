using System.Text.Json.Serialization;

namespace backend.DTOs;

public class PaymentCreateRequest
{
    [JsonPropertyName("transaction_amount")]
    public double TransactionAmount { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("payment_method_id")]
    public string PaymentMethodId { get; set; }

    [JsonPropertyName("payer")]
    public PaymentPayerRequest Payer { get; set; }

    [JsonPropertyName("date_of_expiration")]
    public string DateOfExpiration { get; set; }
}