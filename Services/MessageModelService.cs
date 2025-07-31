using backend.Models;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using backend.DTOs;

namespace backend.Services;

public class MessageModelService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public MessageModelService(
        ApplicationDbContext context,
        ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
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
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

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

        _context.MessageModels.Add(messageModel);
        await _context.SaveChangesAsync();

        return messageModel;
    }
}