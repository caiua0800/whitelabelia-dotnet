using backend.Models;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;
using System.Globalization;

namespace backend.Services;


public class ChatService : IChatService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly IAgentService _agentService;
    private readonly TagService _tagService;
    public ChatService(ApplicationDbContext context, ITenantService tenantService, IAgentService agentService, TagService tagService)
    {
        _context = context;
        _tenantService = tenantService;
        _agentService = agentService;
        _tagService = tagService;
    }

    public async Task<IEnumerable<Chat>> GetAllChatsAsync()
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        return await _context.Chats
            .Where(c => c.EnterpriseId == enterpriseId)
            .ToListAsync();
    }

    public async Task<List<ClientShotDto>> GetAllClientShotsDtoAsync()
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var chats = await _context.Chats
            .Where(c => c.EnterpriseId == enterpriseId)
            .ToListAsync();

        List<ClientShotDto> result = new List<ClientShotDto>();

        foreach (var chat in chats)
        {
            result.Add(new ClientShotDto(chat.ClientName != null ? chat.ClientName : "", chat.Id));
        }

        return result;
    }

    public async Task<int> GetClientShotsCountAsync()
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        return await _context.Chats
            .Where(c => c.EnterpriseId == enterpriseId)
            .CountAsync();
    }

    public async Task<int> CreateMultipleChatsAsync(List<MultipleChat> chats)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var agent = await _agentService.GetAgent(enterpriseId);
        var createdCount = 0;

        foreach (var chat in chats)
        {
            try
            {
                var fullContact = $"55{chat.Contact}";

                // Verificar se o chat já existe
                var existingChat = await _context.Chats
                    .FirstOrDefaultAsync(c => c.Id == fullContact);

                if (existingChat != null)
                {
                    Console.WriteLine($"Chat já existe para o número {fullContact} - pulando");
                    continue;
                }

                var newChat = new Chat
                {
                    Id = fullContact,
                    ClientName = chat.Name,
                    ClientEmail = chat.ClientEmail,
                    ClientCpfCnpj = chat.ClientCpfCnpj,
                    AgentNumber = agent?.Number,
                    Status = 1,
                    EnterpriseId = enterpriseId,
                    DateCreated = DateTime.UtcNow,
                    ClientNameNormalized = RemoveAccents(chat.Name),
                    Tags = chat.Tags,
                };

                _context.Chats.Add(newChat);
                createdCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar chat para {chat.Name}: {ex.Message}");
            }
        }

        await _context.SaveChangesAsync();
        return createdCount;
    }

    public async Task<PagedResult<ChatDto>> SearchChatsAsync(
    string? searchTerm,
    int pageNumber,
    int pageSize,
    string? agentNumber = "",
    string? order = "desc",
    DateTime? startDate = null,
    DateTime? endDate = null,
    List<int>? tagIds = null,
    bool? withMessage = false)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        // Primeiro obtemos os chats sem filtrar por LastMessages
        var query = _context.Chats
            .Where(c => c.EnterpriseId == enterpriseId)
        .Where(c => c.EnterpriseId == enterpriseId)
        .AsQueryable();

        // Agora podemos usar métodos LINQ to Objects
        if (withMessage.HasValue && withMessage.Value)
        {
            query = query.Where(c =>
                c.LastMessages != null &&
                c.LastMessages.Any(m =>
                    (string.IsNullOrEmpty(agentNumber) || m.AgentNumber == agentNumber) &&
                    m.Text != null));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = $"%{RemoveAccents(searchTerm.ToLower())}%";
            query = query.Where(c =>
                (c.ClientNameNormalized != null && EF.Functions.ILike(c.ClientNameNormalized, term)) ||
                EF.Functions.ILike(c.Id, term));
        }

        if (startDate.HasValue)
        {
            query = query.Where(c => c.DateCreated >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var endDateInclusive = endDate.Value.AddDays(1);
            query = query.Where(c => c.DateCreated < endDateInclusive);
        }

        // Filtro por tags
        if (tagIds != null && tagIds.Any())
        {
            query = query.Where(c => c.Tags != null && tagIds.All(tagId => c.Tags.Contains(tagId)));
        }

        var totalCount = query.Count();

        // Ordenação modificada para considerar agentNumber null
        query = order?.ToLower() switch
        {
            "asc" => query.OrderBy(c =>
                c.LastMessages?.LastOrDefault(m =>
                    string.IsNullOrEmpty(agentNumber) || m.AgentNumber == agentNumber)?.DateCreated ?? DateTime.MinValue),
            _ => query.OrderByDescending(c =>
                c.LastMessages?.LastOrDefault(m =>
                    string.IsNullOrEmpty(agentNumber) || m.AgentNumber == agentNumber)?.DateCreated ?? DateTime.MinValue)
        };

        var pagedChats = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var chatDtos = new List<ChatDto>();
        foreach (var chat in pagedChats)
        {
            List<string> tagNames = new List<string>();
            if (chat.Tags != null)
            {
                foreach (var tagId in chat.Tags)
                {
                    var tag = await _tagService.GetTagByIdAsync(tagId);
                    if (tag != null)
                        tagNames.Add(tag.Name);
                }
            }

            // Modificado para considerar agentNumber null
            var lastMessage = string.IsNullOrEmpty(agentNumber)
                ? chat.LastMessages?.LastOrDefault()
                : chat.LastMessages?.LastOrDefault(m => m.AgentNumber == agentNumber);

            chatDtos.Add(new ChatDto
            {
                Id = chat.Id,
                ClientName = chat.ClientName,
                Status = chat.Status,
                ClientCpfCnpj = chat.ClientCpfCnpj,
                ClientEmail = chat.ClientEmail,
                LastMessageText = lastMessage?.Text,
                AgentNumber = chat.AgentNumber,
                DateCreated = chat.DateCreated,
                LastMessageDate = lastMessage?.DateCreated ?? DateTime.MinValue,
                LastMessageIsReply = lastMessage?.IsReply ?? false,
                LastMessageIsSeen = lastMessage?.IsSeen ?? false,
                EnterpriseId = chat.EnterpriseId,
                CustomPrompt = chat.CustomPrompt,
                Street = chat.Street,
                Number = chat.Number,
                Neighborhood = chat.Neighborhood,
                Zipcode = chat.Zipcode,
                City = chat.City,
                Complement = chat.Complement,
                Country = chat.Country,
                State = chat.State,
                Tags = chat.Tags,
                TagNames = tagNames
            });
        }

        return new PagedResult<ChatDto>(chatDtos, totalCount, pageNumber, pageSize);
    }

    public async Task<List<string>?> GetChatByIdAsync(List<int> tagIds)
    {
        List<string> tagNames = new List<string>();

        foreach (var tagId in tagIds)
        {
            var aux = await _tagService.GetTagByIdAsync(tagId);
            if (aux != null)
                tagNames.Add(aux.Name);
        }

        return tagNames;
    }


    private static string RemoveAccents(string text)
    {
        // FIRST, check if the input is null or whitespace.
        if (string.IsNullOrWhiteSpace(text))
        {
            return text; // Return the original value (null or whitespace) immediately.
        }

        // Now it's safe to perform operations on the text.
        text = text.ToLower();
        text = text.Normalize(NormalizationForm.FormD);
        var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
        return new string(chars).Normalize(NormalizationForm.FormC);
    }

    public async Task<Chat?> GetChatByIdAsync(string id)
    {
        return await _context.Chats
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<bool?> GetLastMessageIsSeenAsync(string id, string agentNumber)
    {
        var chat = await _context.Chats
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        foreach (var i in chat.LastMessages)
        {
            if (agentNumber == i.AgentNumber)
            {
                return i.IsSeen;
            }
        }

        return false;
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
        if (chat.ClientName != null)
            chat.ClientNameNormalized = RemoveAccents(chat.ClientName);
        chat.Status = 1;
        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();
        return chat;
    }

    public async Task<List<ChatDto>> GetAllChatsWithLastMessageAsync(string agentNumber)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        // Primeiro obtemos os chats do banco de dados
        var chats = await _context.Chats
            .Where(c => c.EnterpriseId == enterpriseId)
            .ToListAsync(); // Materializa a consulta no banco de dados

        // Agora podemos ordenar na memória
        var orderedChats = chats
            .OrderByDescending(c =>
                c.LastMessages?.LastOrDefault(m => m.AgentNumber == agentNumber)?.DateCreated ?? DateTime.MinValue)
            .ToList(); // Usamos ToList() em vez de ToListAsync()

        var chatDtos = new List<ChatDto>();

        foreach (var chat in orderedChats)
        {
            var chatTags = chat.Tags;
            List<string> tagNames = new List<string>();

            if (chatTags != null)
            {
                foreach (var tag in chatTags)
                {
                    var aux = await _tagService.GetTagByIdAsync(tag);
                    if (aux != null)
                        tagNames.Add(aux.Name);
                }
            }

            var lastMessage = chat.LastMessages?.LastOrDefault(m => m.AgentNumber == agentNumber);

            chatDtos.Add(new ChatDto
            {
                Id = chat.Id,
                Status = chat.Status,
                ClientEmail = chat.ClientEmail,
                ClientCpfCnpj = chat.ClientCpfCnpj,
                LastMessageText = lastMessage?.Text,
                AgentNumber = chat.AgentNumber,
                DateCreated = chat.DateCreated,
                LastMessageDate = lastMessage?.DateCreated ?? DateTime.MinValue,
                LastMessageIsReply = lastMessage?.IsReply ?? false,
                LastMessageIsSeen = lastMessage?.IsSeen ?? false,
                EnterpriseId = chat.EnterpriseId,
                CustomPrompt = chat.CustomPrompt,
                ClientName = chat.ClientName,
                ClientNameNormalized = chat.ClientNameNormalized,
                Street = chat.Street,
                Number = chat.Number,
                Neighborhood = chat.Neighborhood,
                Zipcode = chat.Zipcode,
                City = chat.City,
                Complement = chat.Complement,
                Country = chat.Country,
                State = chat.State,
                Tags = chat.Tags,
                TagNames = tagNames
            });
        }

        return chatDtos;
    }

    public async Task<Chat> UpdateChatAsync(Chat chat)
    {
        Console.WriteLine($"Chat recebido para atualização: {JsonSerializer.Serialize(chat)}");

        // var enterpriseId = _tenantService.TryGetCurrentEnterpriseId();

        // if (!enterpriseId.HasValue && !string.IsNullOrEmpty(chat.AgentNumber))
        // {
        //     Console.WriteLine("Obtendo EnterpriseId pelo número do agente");
        //     enterpriseId = await _agentService.GetEnterpriseIdByAgentNumberAsync(chat.AgentNumber);
        // }

        var enterpriseId = await _agentService.GetEnterpriseIdByAgentNumberAsync(chat.AgentNumber);

        if (!enterpriseId.HasValue)
        {
            Console.WriteLine("EnterpriseId não encontrado");
            throw new InvalidOperationException("Não foi possível determinar o EnterpriseId");
        }

        Console.WriteLine($"EnterpriseId determinado: {enterpriseId}");

        var existingChat = await _context.Chats
            .Where(c => c.Id == chat.Id && c.EnterpriseId == enterpriseId)
            .FirstOrDefaultAsync();

        if (existingChat == null)
        {
            Console.WriteLine($"Chat não encontrado para o EnterpriseId: {enterpriseId}");
            throw new UnauthorizedAccessException("Chat não pertence ao tenant atual");
        }

        existingChat.ClientName = chat.ClientName;
        existingChat.ClientNameNormalized = RemoveAccents(chat.ClientName);
        existingChat.Street = chat.Street;
        existingChat.Neighborhood = chat.Neighborhood;
        existingChat.Number = chat.Number;
        existingChat.City = chat.City;
        existingChat.State = chat.State;
        existingChat.Country = chat.Country;
        existingChat.Zipcode = chat.Zipcode;
        existingChat.Complement = chat.Complement;

        await _context.SaveChangesAsync();
        return existingChat;
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

    public async Task UpdateChatClientNameAsync(string id, string newName)
    {
        var chat = await _context.Chats
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (chat == null)
        {
            throw new UnauthorizedAccessException("Chat não pertence ao tenant atual");
        }

        chat.ClientName = newName;
        _context.Entry(chat).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateChatClientEmailAsync(string id, string newString)
    {
        var chat = await _context.Chats
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (chat == null)
        {
            throw new UnauthorizedAccessException("Chat não pertence ao tenant atual");
        }

        chat.ClientEmail = newString;
        _context.Entry(chat).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateChatClientCpfCnpjAsync(string id, string newString)
    {
        var chat = await _context.Chats
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (chat == null)
        {
            throw new UnauthorizedAccessException("Chat não pertence ao tenant atual");
        }

        chat.ClientCpfCnpj = newString;
        _context.Entry(chat).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateChatLastMessageIsSeenAsync(string id, string agentNumber)
    {
        var chat = await _context.Chats
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (chat == null)
        {
            throw new UnauthorizedAccessException("Chat não encontrado");
        }

        chat.LastMessages.Find(c => c.AgentNumber == agentNumber).IsSeen = true;
        _context.Entry(chat).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<bool?> UpdateChatTagsAsync(string chatId, List<int> newTags)
    {
        var chat = await _context.Chats
            .Where(c => c.Id == chatId)
            .FirstOrDefaultAsync();

        if (chat == null)
        {
            return null;
        }

        chat.Tags = newTags;
        _context.Entry(chat).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return true;
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

