using backend.Models;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;

namespace backend.Services;


public class TagService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public TagService(
        ApplicationDbContext context,
        ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Tag> CreateTagAsync(Tag tag)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        if (tag == null) return null;
        if (tag.Description == null || tag.Description.Trim() == "") return null;
        if (tag.Name == null || tag.Name.Trim() == "") return null;
        if (enterpriseId == null) return null;

        tag.DateCreated = DateTime.UtcNow;
        tag.EnterpriseId = enterpriseId;

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    public async Task<Tag?> GetTagByIdAsync(int id)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        return await _context.Tags
            .IgnoreQueryFilters()
            .Where(m => m.Id == id && m.EnterpriseId == enterpriseId)
            .OrderBy(m => m.DateCreated)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Tag>> GetAllTagsAsync()
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        return await _context.Tags
            .IgnoreQueryFilters()
            .Where(c => c.EnterpriseId == enterpriseId)
            .OrderBy(m => m.DateCreated)
            .ToListAsync();
    }

    public async Task<IEnumerable<Tag?>> GetTagsByEnterpriseIdAsync(int enterpriseId)
    {
        return await _context.Tags
            .IgnoreQueryFilters()
            .Where(c => c.EnterpriseId == enterpriseId)
            .OrderBy(m => m.DateCreated)
            .ToListAsync();
    }

    public async Task<Tag> UpdateTagAsync(Tag tag)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var existingTag = await _context.Tags
            .FirstOrDefaultAsync(t => t.Id == tag.Id && t.EnterpriseId == enterpriseId);

        if (existingTag == null)
            throw new KeyNotFoundException("Tag não encontrada ou não pertence a esta empresa");

        existingTag.Name = tag.Name;
        existingTag.Description = tag.Description;

        await _context.SaveChangesAsync();
        return existingTag;
    }

    public async Task DeleteTagAsync(int id)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        var tag = await _context.Tags.FindAsync(id);
        if (tag != null && tag.EnterpriseId == enterpriseId)
        {
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Tag>> SearchTagsByNameAsync(string name)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        if (string.IsNullOrWhiteSpace(name))
            return await GetAllTagsAsync(); // Retorna todas se o nome for vazio

        return await _context.Tags
            .IgnoreQueryFilters()
            .Where(t => t.EnterpriseId == enterpriseId &&
                        EF.Functions.ILike(t.Name, $"%{name}%")) // Case-insensitive + busca parcial
            .OrderBy(t => t.Name)
            .ToListAsync();
    }
}