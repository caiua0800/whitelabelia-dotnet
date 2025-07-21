using System.Text.Json.Serialization;

public class TransactionDetails
{
    [JsonPropertyName("external_resource_url")]
    public string ExternalResourceUrl { get; set; }

    [JsonPropertyName("financial_institution")]
    public string FinancialInstitution { get; set; }

    [JsonPropertyName("barcode")]
    public object Barcode { get; set; }
}