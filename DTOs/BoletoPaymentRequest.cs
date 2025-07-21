using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.DTOs;

public class BoletoPaymentRequest
{
    [Required]
    [JsonPropertyName("transaction_amount")]
    public double Transaction_amount { get; set; }

    [Required]
    [JsonPropertyName("description")]
    public string Description { get; set; }

    [Required]
    [EmailAddress]
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [Required]
    [JsonPropertyName("first_name")]
    public string First_name { get; set; }

    [Required]
    [JsonPropertyName("last_name")]
    public string Last_name { get; set; }

    [Required]
    [JsonPropertyName("zip_code")]
    public string Zip_code { get; set; }

    [Required]
    [JsonPropertyName("street_name")]
    public string Street_name { get; set; }

    [Required]
    [JsonPropertyName("street_number")]
    public string Street_number { get; set; }

    [Required]
    [JsonPropertyName("neighborhood")]
    public string Neighborhood { get; set; }

    [Required]
    [JsonPropertyName("city")]
    public string City { get; set; }

    [Required]
    [JsonPropertyName("federal_unit")]
    public string Federal_unit { get; set; }

    [Required]
    [JsonPropertyName("identification_type")]
    public string IdentificationType { get; set; }

    [Required]
    [JsonPropertyName("identification_number")]
    public string Number { get; set; }
}