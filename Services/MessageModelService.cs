using backend.Models;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using backend.DTOs;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace backend.Services;

public class MessageModelService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

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

        // Validação do cabeçalho
        if (ContainsInvalidCharacters(createDto.HeaderText))
        {
            throw new ArgumentException("O cabeçalho não pode conter quebras de linha, emojis ou caracteres especiais");
        }

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

        await CreateWhatsAppTemplate(messageModel, createDto.AccountId, createDto.WhatsappToken);


        _context.MessageModels.Add(messageModel);
        await _context.SaveChangesAsync();

        return messageModel;
    }

    private async Task CreateWhatsAppTemplate(MessageModel messageModel, string accountId, string whatsappToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", whatsappToken);

        var requestUrl = $"https://graph.facebook.com/v23.0/{accountId}/message_templates";

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

        var requestBody = new
        {
            name = messageModel.Name.ToLower().Replace(" ", "_"),
            language = "pt_BR",
            category = "MARKETING",
            components = components
        };

        try
        {
            var jsonBody = JsonSerializer.Serialize(requestBody);
            Console.WriteLine($"Enviando para WhatsApp API: {jsonBody}");

            var response = await httpClient.PostAsJsonAsync(requestUrl, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro detalhado: {errorContent}");

                // Parse do erro específico do WhatsApp
                var whatsappError = JsonSerializer.Deserialize<WhatsAppErrorResponse>(errorContent);
                var errorMessage = "Falha ao criar template no WhatsApp";

                if (whatsappError?.Error != null)
                {
                    errorMessage = whatsappError.Error.ErrorUserMsg ?? whatsappError.Error.Message;
                }

                throw new Exception(errorMessage);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<WhatsAppTemplateResponseDto>(responseContent);
            messageModel.MetaTemplateId = responseData.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro completo: {ex}");
            throw;
        }
    }

    private bool ContainsInvalidCharacters(string text)
    {
        // Verifica por quebras de linha, emojis, asteriscos, etc.
        return text.Contains('\n') ||
               text.Contains('*') ||
               Regex.IsMatch(text, @"\p{Cs}") || // Emojis
               text.Any(c => char.IsSurrogate(c));
    }

    private class WhatsAppErrorResponse
    {
        public WhatsAppError Error { get; set; }
    }

    private class WhatsAppError
    {
        public string Message { get; set; }
        public string Type { get; set; }
        public int Code { get; set; }
        public string ErrorUserMsg { get; set; }
        public string ErrorUserTitle { get; set; }
    }
}