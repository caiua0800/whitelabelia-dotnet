using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models;

public class ClientShotDtoStartLeads
{

    [JsonPropertyName("client_shot_dto")]
    public ClientShotDto ClientShotDto { get; set; }

    public ClientShotDtoStartLeads(ClientShotDto clientShotDto)
    {
        ClientShotDto = clientShotDto;
    }

}