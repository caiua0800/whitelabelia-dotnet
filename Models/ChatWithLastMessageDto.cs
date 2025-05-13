namespace backend.Models;

public class ChatWithLastMessageDto
{
    public string Id { get; set; }
    public int EnterpriseId { get; set; }
    public int Status { get; set; }
    public DateTime DateCreated { get; set; }
    public string? LastMessageText { get; set; }
    public DateTime? LastMessageDate { get; set; }
    public bool LastMessageIsReply { get; set; }
}
