using System.Text.Json.Serialization;

namespace backend.DTOs;

public class PaymentPayerAddressRequest
{
    [JsonPropertyName("zip_code")]
    public string ZipCode { get; set; }

    [JsonPropertyName("street_name")]
    public string StreetName { get; set; }

    [JsonPropertyName("street_number")]
    public string StreetNumber { get; set; }

    [JsonPropertyName("neighborhood")]
    public string Neighborhood { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("federal_unit")]
    public string FederalUnit { get; set; }
}