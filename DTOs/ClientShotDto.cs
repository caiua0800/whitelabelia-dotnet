using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models;

public class ClientShotDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("number")]
    public string Number { get; set; }

    public ClientShotDto(string name, string number)
    {
        Name = name;
        Number = number;
    }

}