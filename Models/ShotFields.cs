using System.Text.Json.Serialization;

public class ShotFields
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    public ShotFields() { }

    public ShotFields(string name, string value)
    {
        Name = name;
        Value = value;
    }
}