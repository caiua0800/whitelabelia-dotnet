using backend.Models;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;

namespace backend.Services;

public interface IMessageService
{
    Task<IEnumerable<Message>> GetMessagesByChatIdAsync(string chatId, string agentNumber);
    Task<Message> GetMessageByIdAsync(int id);
    Task<Message> SendMessageAsync(Message message);
    Task UpdateMessageAsync(Message message);
    Task DeleteMessageAsync(int id);
}

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly IChatService _chatService;
    private readonly IAgentService _agentService;

    private readonly WebSocketConnections _webSocketConnections;

    public MessageService(
        ApplicationDbContext context,
        ITenantService tenantService,
        IChatService chatService,
        IAgentService agentService,
        WebSocketConnections webSocketConnections) // Injetar o novo serviço
    {
        _context = context;
        _tenantService = tenantService;
        _chatService = chatService;
        _agentService = agentService;
        _webSocketConnections = webSocketConnections;
    }

    public async Task<IEnumerable<Message>> GetMessagesByChatIdAsync(string chatId, string agentNumber)
    {
        return await _context.Messages
            .IgnoreQueryFilters() // Ignora o HasQueryFilter
            .Where(m => m.ChatId == chatId && m.AgentNumber == agentNumber)
            .OrderBy(m => m.DateCreated)
            .ToListAsync();
    }

    public async Task<Message> GetMessageByIdAsync(int id)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var message = await _context.Messages.FindAsync(id);

        if (message == null || message.EnterpriseId != enterpriseId)
        {
            return null;
        }

        return message;
    }


    public async Task<Message> SendMessageAsync(Message message)
    {
        message.IsReply = message.IsReply ?? false;
        var enterpriseId = await _agentService.GetEnterpriseIdByAgentNumberAsync(message.AgentNumber);
        Console.WriteLine($"message.AgentNumber do sen {message.AgentNumber}");
        if (!enterpriseId.HasValue)
        {
            throw new InvalidOperationException("Não foi possível determinar o EnterpriseId");
        }

        message.EnterpriseId = enterpriseId.Value;
        message.DateCreated = DateTime.UtcNow;

        var chat = await _chatService.GetChatByIdAsync(message.ChatId);
        if (chat == null)
        {
            chat = new Chat
            {
                Id = message.ChatId,
                EnterpriseId = enterpriseId.Value,
                DateCreated = DateTime.UtcNow,
                Status = 1,
                AgentNumber = message.AgentNumber,
                LastMessages = new List<LastMessageDto>()
            };
            await _chatService.CreateChatAsync(chat);
        }

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Atualiza ou adiciona a última mensagem
        var existingLastMessage = chat.LastMessages?.FirstOrDefault(m => m.AgentNumber == message.AgentNumber);

        if (existingLastMessage != null)
        {
            // Atualiza a mensagem existente
            existingLastMessage.Text = message.Text;
            existingLastMessage.IsSeen = false;
            existingLastMessage.IsReply = message.IsReply ?? false;
            existingLastMessage.DateCreated = DateTime.UtcNow;
        }
        else
        {
            // Adiciona nova mensagem se não existir para este AgentNumber
            chat.LastMessages ??= new List<LastMessageDto>();
            chat.LastMessages.Add(new LastMessageDto
            {
                AgentNumber = message.AgentNumber,
                Text = message.Text,
                IsSeen = false,
                IsReply = message.IsReply ?? false,
                DateCreated = DateTime.UtcNow
            });
        }

        // Mantém apenas as últimas N mensagens (ex: 5) por AgentNumber
        chat.LastMessages = chat.LastMessages
            .GroupBy(m => m.AgentNumber)
            .SelectMany(g => g.OrderByDescending(m => m.DateCreated).Take(1))
            .OrderByDescending(m => m.DateCreated)
            .Take(5)
            .ToList();

        await _chatService.UpdateChatAsync(chat);

        var notification = new
        {
            type = "new_message",
            chatId = message.ChatId,
            message = new
            {
                id = message.Id,
                text = message.Text,
                sender = message.ChatId,
                isReply = message.IsReply,
                agentNumber = message.AgentNumber,
                messageType = message.MessageType,
                chatId = message.ChatId,
                isRead = message.IsRead,
                dateCreated = message.DateCreated
            }
        };

        await _webSocketConnections.BroadcastAsync(JsonSerializer.Serialize(notification));

        return message;
    }

    public async Task UpdateMessageAsync(Message message)
    {
        // message.EnterpriseId = _tenantService.GetCurrentEnterpriseId();
        message.EnterpriseId = 1;
        _context.Entry(message).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteMessageAsync(int id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message != null)
        {
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
        }
    }
}