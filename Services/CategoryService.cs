// IAgentService.cs
namespace backend.Services;

using System.Globalization;
using System.Text;
using backend.Models;
using Microsoft.EntityFrameworkCore;

public class CategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public CategoryService(ApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var categories = await _context.Categories
            .Where(a => a.EnterpriseId == enterpriseId)
            .ToListAsync();

        return categories;
    }

    public async Task<Category?> GetCategoryById(int id)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var category = await _context.Categories
            .Where(a => a.EnterpriseId == enterpriseId && a.Id == id)
            .FirstOrDefaultAsync();

        return category;
    }


    public async Task<Category> CreateCategoryAsync(Category category)
    {
        category.DateCreated = DateTime.Now;
        category.NameNormalized = RemoveAccents(category.Name);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
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

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        // Verifica se a categoria existe e pertence ao tenant
        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == category.Id && c.EnterpriseId == enterpriseId);

        if (existingCategory == null)
        {
            throw new KeyNotFoundException("Categoria não encontrada ou não pertence a este tenant");
        }

        // Verifica se já existe outra categoria com o mesmo nome
        var normalizedName = RemoveAccents(category.Name.ToLower());
        var duplicateCategory = await _context.Categories
            .Where(c => c.EnterpriseId == enterpriseId &&
                       c.NameNormalized == normalizedName &&
                       c.Id != category.Id)
            .FirstOrDefaultAsync();

        if (duplicateCategory != null)
        {
            throw new InvalidOperationException("Já existe outra categoria com este nome");
        }

        // Atualiza os campos permitidos
        existingCategory.Name = category.Name;
        existingCategory.NameNormalized = normalizedName;
        existingCategory.Description = category.Description;
        // Não atualizamos DateCreated e EnterpriseId

        await _context.SaveChangesAsync();
        return existingCategory;
    }

    public async Task<Category> UpdateProductAsync(Category category)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var existingProduct = await _context.Categories
            .Where(p => p.Id == category.Id && p.EnterpriseId == enterpriseId)
            .FirstOrDefaultAsync();

        if (existingProduct == null)
        {
            throw new KeyNotFoundException("Product not found");
        }

        existingProduct.Name = category.Name;
        existingProduct.Description = category.Description;
        existingProduct.DateCreated = category.DateCreated;
        existingProduct.EnterpriseId = enterpriseId;

        _context.Categories.Update(existingProduct);
        await _context.SaveChangesAsync();

        return existingProduct;
    }
}