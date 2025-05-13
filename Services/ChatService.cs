using backend.Models;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public interface IChatService
{
    Task<IEnumerable<Chat>> GetAllChatsAsync();
    Task<string?> GetCustomPromptByChatId(string id);
    Task UpdateChatLastMessageIsSeenAsync(string id);
    Task UpdateCustomPrompt(string id, string prompt);
    Task<List<Chat>> GetAllChatsWithLastMessageAsync();
    Task<bool?> GetLastMessageIsSeenAsync(string id);
    Task<Chat?> GetChatByIdAsync(string id);
    Task<Chat> CreateChatAsync(Chat chat);
    Task UpdateChatAsync(Chat chat);
    Task DeleteChatAsync(string id);

    Task UpdateChatStatusAsync(string id, int newStatus);
}

public class ChatService : IChatService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly IAgentService _agentService;
    public ChatService(ApplicationDbContext context, ITenantService tenantService, IAgentService agentService)
    {
        _context = context;
        _tenantService = tenantService;
        _agentService = agentService;
    }

    public async Task<IEnumerable<Chat>> GetAllChatsAsync()
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        return await _context.Chats
            .Where(c => c.EnterpriseId == enterpriseId)
            .ToListAsync();
    }

    public async Task<Chat?> GetChatByIdAsync(string id)
    {
        return await _context.Chats
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<bool?> GetLastMessageIsSeenAsync(string id)
    {
        var chat = await _context.Chats
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        return chat?.LastMessageIsSeen;
    }

    public async Task<string?> GetCustomPromptByChatId(string id)
    {
        var chat = await _context.Chats
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();
        return chat?.CustomPrompt;
    }

    public async Task<Chat> CreateChatAsync(Chat chat)
    {
        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();
        return chat;
    }

    public async Task<List<Chat>> GetAllChatsWithLastMessageAsync()
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var chats = await _context.Chats
            .Where(c => c.EnterpriseId == enterpriseId)
            .ToListAsync();
        return chats;
    }

    public async Task UpdateChatAsync(Chat chat)
    {
        var enterpriseId = _tenantService.TryGetCurrentEnterpriseId();

        if (!enterpriseId.HasValue && !string.IsNullOrEmpty(chat.AgentNumber))
        {
            Console.WriteLine($"Não foi o carai: {chat.AgentNumber}");
            enterpriseId = await _agentService.GetEnterpriseIdByAgentNumberAsync(chat.AgentNumber);
        }

        if (!enterpriseId.HasValue)
        {
            throw new InvalidOperationException("Não foi possível determinar o EnterpriseId");
        }

        var existingChat = await _context.Chats
            .Where(c => c.Id == chat.Id && c.EnterpriseId == enterpriseId)
            .FirstOrDefaultAsync();

        if (existingChat == null)
        {
            throw new UnauthorizedAccessException("Chat não pertence ao tenant atual");
        }

        chat.EnterpriseId = enterpriseId.Value;
        _context.Entry(chat).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateChatStatusAsync(string id, int newStatus)
    {
        var chat = await _context.Chats
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (chat == null)
        {
            throw new UnauthorizedAccessException("Chat não pertence ao tenant atual");
        }

        chat.Status = newStatus;
        _context.Entry(chat).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateChatLastMessageIsSeenAsync(string id)
    {
        var chat = await _context.Chats
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (chat == null)
        {
            throw new UnauthorizedAccessException("Chat não encontrado");
        }

        chat.LastMessageIsSeen = true;
        _context.Entry(chat).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCustomPrompt(string id, string prompt)
    {
        var chat = await _context.Chats
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (chat == null)
        {
            throw new UnauthorizedAccessException("Chat não encontrado");
        }

        chat.CustomPrompt = prompt;
        _context.Entry(chat).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteChatAsync(string id)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var chat = await _context.Chats
            .Where(c => c.Id == id && c.EnterpriseId == enterpriseId)
            .FirstOrDefaultAsync();

        if (chat != null)
        {
            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();
        }
    }


}