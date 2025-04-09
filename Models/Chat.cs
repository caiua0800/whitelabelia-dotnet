
using backend.Models;
namespace backend.Models;
public class Chat{
    public string Id { get; set; }
    public List<Message> Messages { get; set; } = new List<Message>();
}