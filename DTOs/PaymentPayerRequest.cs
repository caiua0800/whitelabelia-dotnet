using System.Text.Json.Serialization;

namespace backend.DTOs;

public class PaymentPayerRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string LastName { get; set; }

    [JsonPropertyName("identification")]
    public PaymentPayerIdentificationRequest Identification { get; set; }

    [JsonPropertyName("address")]
    public PaymentPayerAddressRequest Address { get; set; }
}