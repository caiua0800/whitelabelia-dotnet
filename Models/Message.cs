
using backend.Models;
namespace backend.Models;
public class Message{

    public string Id { get; set; }
    public string ChatId { get; set; }
    public string Text { get; set; }
    public string From{ get; set; }
    public string To{ get; set; }

}