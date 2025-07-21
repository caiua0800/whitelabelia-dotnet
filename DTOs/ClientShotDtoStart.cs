using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models;

public class ClientShotDtoStart
{

    [JsonPropertyName("client_shot_dto")]
    public ClientShotDto ClientShotDto { get; set; }

    [JsonPropertyName("text_to_send")]
    public string TextToSend { get; set; }

    [JsonPropertyName("my_name")]
    public string MyName { get; set; }

    public ClientShotDtoStart(ClientShotDto clientShotDto, string textToSend, string myName)
    {
        ClientShotDto = clientShotDto;
        TextToSend = textToSend;
        MyName = myName;
    }

}