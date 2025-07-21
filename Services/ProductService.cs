// IAgentService.cs
namespace backend.Services;

using System.Globalization;
using System.Text;
using backend.Interfaces;
using backend.Models;
using Microsoft.EntityFrameworkCore;

public class ProductService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly IAgentService _agentService;

    public ProductService(ApplicationDbContext context, ITenantService tenantService, IAgentService agentService)
    {
        _context = context;
        _tenantService = tenantService;
        _agentService = agentService;
    }

    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var products = await _context.Products
            .Where(a => a.EnterpriseId == enterpriseId)
            .ToListAsync();

        return products;
    }

    public async Task<IEnumerable<Product>> GetProductsByAgentNumberAsync(string number)
    {
        var enterpriseId = await _agentService.GetEnterpriseIdByAgentNumberAsync(number);
        var products = await _context.Products
            .Where(a => a.EnterpriseId == enterpriseId)
            .ToListAsync();

        return products;
    }

    public async Task<Product?> GetProductById(int id)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var product = await _context.Products
            .Where(a => a.Id == id)
            .FirstOrDefaultAsync();

        return product;
    }

        public async Task<Product?> GetProductById2(int id)
    {
        var product = await _context.Products
            .Where(a => a.Id == id)
            .FirstOrDefaultAsync();

        return product;
    }

    public async Task<PagedResult<Product>> SearchProductsAsync(
    string? searchTerm,
    int pageNumber,
    int pageSize,
    string? order = "desc",
    DateTime? startDate = null,
    DateTime? endDate = null,
    List<string>? categoryNames = null,
    int? status = null)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        var query = _context.Products
            .Where(c => c.EnterpriseId == enterpriseId);
            
        query = query.Where(c => c.Status != 4);

        if (status != null)
        {
            query = query.Where(c => c.Status == status && c.Status != 4);
        }


        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = $"%{RemoveAccents(searchTerm.ToLower())}%";
            query = query.Where(c =>
                EF.Functions.ILike(c.NameNormalized, term) ||
                EF.Functions.ILike(c.Id.ToString(), term));
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

        var totalCount = await query.CountAsync();

        query = order?.ToLower() switch
        {
            "asc" => query.OrderBy(c => c.DateCreated),
            _ => query.OrderByDescending(c => c.DateCreated)
        };

        var pagedProducts = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var products = new List<Product>();
        foreach (var product in pagedProducts)
        {

            products.Add(product);
        }

        return new PagedResult<Product>(products, totalCount, pageNumber, pageSize);
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        if (product.Name != null)
        {
            product.NameNormalized = RemoveAccents(product.Name.ToLower());

            var produtoExistente = await _context.Products
                .Where(p => p.EnterpriseId == enterpriseId &&
                       p.NameNormalized == product.NameNormalized)
                .FirstOrDefaultAsync();

            if (produtoExistente != null)
            {
                throw new InvalidOperationException("Já existe um produto com esse nome cadastrado.");
            }
        }

        product.Status ??= 1;
        product.DateCreated = DateTime.Now;
        product.EnterpriseId = enterpriseId;
        product.CategoryNames ??= Array.Empty<string>();
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return product;
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

    public async Task<Product> UpdateProductAsync(Product product)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id && p.EnterpriseId == enterpriseId);

        if (existingProduct == null)
        {
            throw new KeyNotFoundException("Produto não encontrado ou não pertence a este tenant.");
        }

        if (!string.IsNullOrEmpty(product.Name))
        {
            var normalizedName = RemoveAccents(product.Name.ToLower());
            var duplicateProduct = await _context.Products
                .Where(p => p.EnterpriseId == enterpriseId &&
                       p.NameNormalized == normalizedName &&
                       p.Id != product.Id)
                .FirstOrDefaultAsync();

            if (duplicateProduct != null)
            {
                throw new InvalidOperationException("Já existe outro produto com este nome.");
            }

            existingProduct.NameNormalized = normalizedName;
        }

        // Atualiza apenas os campos permitidos
        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.UnityPrice = product.UnityPrice;
        existingProduct.Status = product.Status;
        existingProduct.CategoryNames = product.CategoryNames ?? Array.Empty<string>();

        await _context.SaveChangesAsync();
        return existingProduct;
    }
}