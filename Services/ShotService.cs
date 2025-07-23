using backend.Models;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;
using System.Globalization;
namespace backend.Services;

public class ShotService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly TagService _tagService;
    private readonly IChatService _chatService;

    public ShotService(ApplicationDbContext context, ITenantService tenantService, TagService tagService, IChatService chatService)
    {
        _context = context;
        _tenantService = tenantService;
        _tagService = tagService;
        _chatService = chatService;
    }

    public async Task<IEnumerable<Shot>> GetAllShotsAsync()
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        return await _context.Shots
            .Where(c => c.EnterpriseId == enterpriseId)
            .ToListAsync();
    }

    public async Task<PaginatedResult<Shot>> SearchShotsAsync(
    string? searchTerm,
    int pageNumber,
    int pageSize,
    string order,
    string? startDate,
    string? endDate,
    int? status)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var query = _context.Shots
            .Where(c => c.EnterpriseId == enterpriseId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var normalizedSearchTerm = RemoveAccents(searchTerm.ToLower());
            query = query.Where(c =>
                c.NameNormalized.Contains(normalizedSearchTerm) ||
                c.Description.ToLower().Contains(searchTerm.ToLower()));
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var startDateParsed))
        {
            query = query.Where(c => c.DateCreated >= startDateParsed);
        }

        if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var endDateParsed))
        {
            query = query.Where(c => c.DateCreated <= endDateParsed);
        }

        // Ordenação
        query = order.ToLower() == "asc"
            ? query.OrderBy(c => c.DateCreated)
            : query.OrderByDescending(c => c.DateCreated);

        // Paginação
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<Shot>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Shot?> GetShotByIdAsync(int id)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        return await _context.Shots
            .Where(c => c.EnterpriseId == enterpriseId && c.Id == id)
            .FirstOrDefaultAsync();
    }

    private static string RemoveAccents(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        text = text.ToLower();
        text = text.Normalize(NormalizationForm.FormD);
        var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
        return new string(chars).Normalize(NormalizationForm.FormC);
    }

    public async Task<Shot> CreateChatAsync(Shot shot)
    {
        if (shot.Name != null)
            shot.NameNormalized = RemoveAccents(shot.Name);
        shot.Status = 1;
        shot.DateCreated = DateTime.Now;

        var json = JsonSerializer.Serialize(shot.ShotFields);
        Console.WriteLine($"Serialized ShotFields: {json}");

        shot.ShotFields ??= new List<ShotFields>();

        _context.Shots.Add(shot);
        await _context.SaveChangesAsync();

        var savedShot = await _context.Shots.FindAsync(shot.Id);
        var savedJson = JsonSerializer.Serialize(savedShot.ShotFields);
        Console.WriteLine($"Saved ShotFields: {savedJson}");
        return shot;
    }

    public async Task UpdateShotStatusAsync(int id, int newStatus)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        var shot = await _context.Shots
            .Where(c => c.Id == id && c.EnterpriseId == enterpriseId)
            .FirstOrDefaultAsync();

        if (shot == null)
        {
            throw new UnauthorizedAccessException("Disparo não pertence ao tenant atual");
        }

        shot.Status = newStatus;
        _context.Entry(shot).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateShotAsync(int id, string newName, string newDescription, int newStatus)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        var shot = await _context.Shots
            .Where(c => c.Id == id && c.EnterpriseId == enterpriseId)
            .FirstOrDefaultAsync();

        if (shot == null)
        {
            throw new UnauthorizedAccessException("Disparo não pertence ao tenant atual");
        }

        shot.Status = newStatus;
        shot.Description = newDescription;
        shot.Name = newName;
        shot.NameNormalized = RemoveAccents(newName);
        _context.Entry(shot).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task SendShot(int id, string agentNumber, List<ClientShotDto> clients)
    {

        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        var shot = await _context.Shots
            .Where(c => c.Id == id && c.EnterpriseId == enterpriseId)
            .FirstOrDefaultAsync();

        if (shot == null)
        {
            Console.WriteLine("Disparo não encontrado ou não pertence ao tenant");
            throw new UnauthorizedAccessException("Disparo não pertence ao tenant atual");
        }

        shot.ShotHistory ??= new List<ShotHistory>();

        var chats = await _context.Chats
            .Where(c => c.EnterpriseId == enterpriseId)
            .ToListAsync();

        if (chats.Any())
        {

            var clientsCount = clients.Count;

            var requestData = new
            {
                recipients = clients,
                model_name = shot.ModelName,
                model_language = "pt_BR",
                text = GetTextFromShotFields(shot.ShotFields),
                agentNumber
            };

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsJsonAsync("http://localhost:3007/send-multiple-model", requestData);
                    Console.WriteLine($"Resposta do serviço: {response.StatusCode}");
                    response.EnsureSuccessStatusCode();
                }

                shot.ShotHistory.Add(new ShotHistory
                {
                    DateSent = DateTime.UtcNow,
                    ClientsQtt = clientsCount,
                    SentClients = clients,
                    Status = 2 
                });

                shot.Status = 2;
                shot.SentClients = clients;
                shot.ClientsQtt = shot.SentClients?.Count ?? 0;
                shot.SendShotDate = DateTime.UtcNow;

                Console.WriteLine("Disparo enviado com sucesso");
            }
            catch (Exception ex)
            {
                shot.ShotHistory.Add(new ShotHistory
                {
                    DateSent = DateTime.UtcNow,
                    Status = 4,
                    ClientsQtt = 0
                });

                Console.WriteLine($"Erro ao enviar disparo: {ex.Message}");
                throw;
            }
        }
        else
        {
            shot.ShotHistory.Add(new ShotHistory
            {
                DateSent = DateTime.UtcNow,
                Status = 3,
                ClientsQtt = 0
            });

            shot.Status = 3;
            Console.WriteLine("Nenhum chat encontrado para envio");
        }

        _context.Entry(shot).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task SendShotStartingLeads(int id, string agentNumber, List<ClientShotDto> clients)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var shot = await _context.Shots
            .Where(c => c.Id == id && c.EnterpriseId == enterpriseId)
            .FirstOrDefaultAsync();

        if (shot == null)
        {
            Console.WriteLine("Disparo não encontrado ou não pertence ao tenant");
            throw new UnauthorizedAccessException("Disparo não pertence ao tenant atual");
        }

        shot.ShotHistory ??= new List<ShotHistory>();

        var clientsCount = clients.Count;

        var requestData = new
        {
            recipients = clients,
            model_name = "start_chat_leads_1", // Nome do modelo corrigido
            model_language = "pt_BR",
            agentNumber
        };

        try
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsJsonAsync("http://localhost:3007/send-multiple-model-start-chat-leads", requestData);
                Console.WriteLine($"Resposta do serviço: {response.StatusCode}");
                response.EnsureSuccessStatusCode();
            }

            shot.ShotHistory.Add(new ShotHistory
            {
                DateSent = DateTime.UtcNow,
                ClientsQtt = clientsCount,
                SentClients = clients,
                Status = 2 // Status de enviado
            });

            shot.Status = 2;
            shot.SentClients = clients;
            shot.ClientsQtt = shot.SentClients?.Count ?? 0;
            shot.SendShotDate = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            shot.ShotHistory.Add(new ShotHistory
            {
                DateSent = DateTime.UtcNow,
                Status = 4,
                ClientsQtt = 0
            });

            Console.WriteLine($"Erro ao enviar disparo: {ex.Message}");
            throw;
        }

        _context.Entry(shot).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task SendStartChatShot(ClientShotDto client, string textToSend, string myName, string agentNumber)
    {

        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        var requestData = new
        {
            recipient = client,
            model_name = "start_chat",
            model_language = "pt_BR",
            text = textToSend,
            client_name = myName,
            agentNumber
        };

        Console.WriteLine($"agentNumber (SendStartChatShot): {agentNumber}");

        try
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsJsonAsync("http://localhost:3007/send-start-chat", requestData);
                Console.WriteLine($"Resposta do serviço: {response.StatusCode}");
                response.EnsureSuccessStatusCode();
            }
            Console.WriteLine("Conversa iniciada com sucesso");
        }
        catch (Exception ex)
        {

            Console.WriteLine($"Erro ao iniciar conversa: {ex.Message}");
            throw;
        }

    }

    public async Task SendStartChatLeadsShot(ClientShotDto client, string agentNumber)
    {

        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        Console.WriteLine($"EnterpriseId: {enterpriseId}");

        var requestData = new
        {
            recipient = client,
            model_name = "start_chat",
            model_language = "pt_BR",
            agentNumber
        };

        try
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsJsonAsync("http://localhost:3007/send-start-chat-leads", requestData);
                Console.WriteLine($"Resposta do serviço: {response.StatusCode}");
                response.EnsureSuccessStatusCode();
            }
            Console.WriteLine("Conversa iniciada com sucesso");
        }
        catch (Exception ex)
        {

            Console.WriteLine($"Erro ao iniciar conversa: {ex.Message}");
            throw;
        }

    }

    private async Task<List<Chat>> GetChatsByShotFilters(ShotFilter? filters)
    {
        if (filters == null) return new List<Chat>();

        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var query = _context.Chats.Where(c => c.EnterpriseId == enterpriseId);

        return await query.ToListAsync();
    }

    private string GetTextFromShotFields(List<ShotFields>? shotFields)
    {
        if (shotFields == null || !shotFields.Any()) return string.Empty;

        var textField = shotFields.FirstOrDefault(f =>
            f.Name.Equals("text", StringComparison.OrdinalIgnoreCase) ||
            f.Name.Equals("message", StringComparison.OrdinalIgnoreCase));

        return textField?.Value ?? string.Empty;
    }

    public async Task DeleteShotAsync(int id)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var shot = await _context.Shots
            .Where(c => c.Id == id && c.EnterpriseId == enterpriseId)
            .FirstOrDefaultAsync();

        if (shot != null)
        {
            _context.Shots.Remove(shot);
            await _context.SaveChangesAsync();
        }
    }
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}