using backend.Models;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using backend.DTOs;
using System.Net.Http.Headers;

namespace backend.Services;

public class MessageModelService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly string _whatsappToken;
    private readonly string _accountId;

    public MessageModelService(
    ApplicationDbContext context,
    ITenantService tenantService,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration)
    {
        _context = context;
        _tenantService = tenantService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _whatsappToken = _configuration["WhatsappUserTokenApi"];
        _accountId = _configuration["MetaAccountId"];
    }

    public async Task<IEnumerable<MessageModel>> GetMessageModels()
    {
        return await _context.MessageModels
            .IgnoreQueryFilters()
            .OrderBy(m => m.DateCreated)
            .ToListAsync();
    }

    public async Task<MessageModel?> GetMessageModelByIdAsync(int id)
    {
        return await _context.MessageModels
            .IgnoreQueryFilters()
            .Where(m => m.Id == id)
            .OrderBy(m => m.DateCreated)
            .FirstOrDefaultAsync();
    }

    public async Task<MessageModel> CreateMessageModelAsync(MessageModelCreateDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.HeaderText))
            throw new ArgumentException("Texto do cabeçalho é obrigatório");

        if (string.IsNullOrWhiteSpace(createDto.BodyText))
            throw new ArgumentException("Texto do corpo é obrigatório");

        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        var existingModelName = await _context.MessageModels
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.Name == createDto.Name.Trim() && m.EnterpriseId == enterpriseId);

        if (existingModelName != null)
        {
            throw new Exception("Já existe um modelo com esse nome");
        }

        var messageModel = new MessageModel
        {
            Name = createDto.Name,
            EnterpriseId = enterpriseId,
            Header = new HeaderMessageModel
            {
                Text = createDto.HeaderText,
                Param = createDto.HeaderParam
            },
            Body = new BodyMessageModel
            {
                Text = createDto.BodyText,
                Params = createDto.BodyParams?.Select(p => new BodyTextParams
                {
                    Key = p.Key,
                    Param = p.Param
                }).ToList()
            },
            DateCreated = DateTime.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(createDto.FooterText))
        {
            messageModel.Footer = new FooterMessageModel
            {
                Text = createDto.FooterText
            };
        }

        await CreateWhatsAppTemplate(messageModel);

        _context.MessageModels.Add(messageModel);
        await _context.SaveChangesAsync();

        return messageModel;
    }

    private async Task CreateWhatsAppTemplate(MessageModel messageModel)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _whatsappToken);

        var requestUrl = $"https://graph.facebook.com/v18.0/{_accountId}/message_templates";

        var components = new List<object>
        {
            new
            {
                type = "HEADER",
                format = "TEXT",
                text = messageModel.Header.Text
            },
            new
            {
                type = "BODY",
                text = messageModel.Body.Text
            }
        };

        if (messageModel.Footer != null && !string.IsNullOrWhiteSpace(messageModel.Footer.Text))
        {
            components.Add(new
            {
                type = "FOOTER",
                text = messageModel.Footer.Text
            });
        }

        // Adicionar footer apenas se houver parâmetros
        if (messageModel.Body.Params?.Any() == true)
        {
            components.Add(new
            {
                type = "FOOTER",
                text = "Texto do rodapé se necessário" // Ou use createDto.FooterText se disponível
            });
        }

        // Corpo da requisição ajustado conforme exemplo que funciona
        var requestBody = new
        {
            name = messageModel.Name.ToLower().Replace(" ", "_"),
            language = "pt_BR",
            category = "MARKETING",
            components = components
        };

        try
        {
            // Serializar manualmente para debug
            var jsonBody = JsonSerializer.Serialize(requestBody);
            Console.WriteLine($"Enviando para WhatsApp API: {jsonBody}");

            var response = await httpClient.PostAsJsonAsync(requestUrl, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro detalhado: {errorContent}");
                throw new Exception($"Falha ao criar template: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Resposta completa: {responseContent}");

            var responseData = JsonSerializer.Deserialize<WhatsAppTemplateResponseDto>(responseContent);
            messageModel.MetaTemplateId = responseData.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro completo: {ex.ToString()}");
            throw new Exception("Falha ao criar template no WhatsApp. Verifique os logs para detalhes.");
        }
    }
}