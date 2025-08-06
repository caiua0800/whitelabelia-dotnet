using backend.Models;
using SixLabors.ImageSharp;

public interface IChatService
{
    Task<IEnumerable<Chat>> GetAllChatsAsync();
    Task<List<ClientShotDto>> GetAllClientShotsDtoAsync();
    Task<string?> GetCustomPromptByChatId(string id);
    Task UpdateChatLastMessageIsSeenAsync(string id, string agentNumber);
    Task UpdateCustomPrompt(string id, string prompt);
    Task<List<ChatDto>> GetAllChatsWithLastMessageAsync(string agentNumber);
    Task<bool?> GetLastMessageIsSeenAsync(string id, string agentNumber);
    Task UpdateLastMessageAsync(string number, string text1, string agentNumber, bool boolzin);
    Task<Chat?> GetChatByIdAsync(string id);
    Task<List<string>?> GetChatByIdAsync(List<int> tagIds);
    Task<Chat> CreateChatAsync(Chat chat);
    Task<Chat> UpdateChatAsync(Chat chat);
    Task DeleteChatAsync(string id);
    Task UpdateChatStatusAsync(string id, int newStatus);
    Task UpdateAllChatStatusAsync(int newStatus);
    Task UpdateChatClientNameAsync(string id, string newName);
    Task UpdateChatClientEmailAsync(string id, string newEmail);
    Task UpdateChatClientCpfCnpjAsync(string id, string newCpfCnpj);
    Task<bool?> UpdateChatTagsAsync(string id, List<int> newTags);
    Task<int> GetClientShotsCountAsync();
    Task<int> CreateMultipleChatsAsync(List<MultipleChat> chats);

    Task<PagedResult<ChatDto>> SearchChatsAsync(
        string? searchTerm,
        int pageNumber,
        int pageSize,
        string agentNumber,
        string? order = "desc",
        DateTime? startDate = null,
        DateTime? endDate = null,
        List<int>? tagIds = null,
        bool? withMessage = false);
}