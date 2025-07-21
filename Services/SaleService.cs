using backend.Models;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;
using System.Globalization;
using backend.DTOs;

namespace backend.Services;

public class SaleService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly IChatService _chatService;
    private readonly ProductService _productService;
    private readonly MercadoPagoService _mercadoPagoService;
    private readonly SaleItemService _saleItemService;


    public SaleService(ApplicationDbContext context, ITenantService tenantService, IChatService chatService, ProductService productService, MercadoPagoService mercadoPagoService, SaleItemService saleItemService)
    {
        _context = context;
        _tenantService = tenantService;
        _chatService = chatService;
        _productService = productService;
        _mercadoPagoService = mercadoPagoService;
        _saleItemService = saleItemService;
    }

    public async Task<IEnumerable<Sale>> GetAllSalesAsync()
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        return await _context.Sales
            .Where(c => c.EnterpriseId == enterpriseId)
            .ToListAsync();
    }

    public async Task<SaleWithProductsDto> CreateSaleAsync(CreateSaleDto createSaleDto, int enterpriseId)
    {
        // var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        var sale = createSaleDto.Sale;

        sale.EnterpriseId = enterpriseId;
        Console.WriteLine($"sale.EnterpriseId: {sale.EnterpriseId}");

        if (sale.SaleItems != null)
        {
            foreach (var item in sale.SaleItems)
            {
                item.EnterpriseId = enterpriseId;

                var product = await _productService.GetProductById2(item.ProductId);
                item.ProductUnityPrice = product.UnityPrice;
            }
        }

        sale.Description = "Venda de produtos";
        sale.Status = 1;
        sale.TotalAmount = await _saleItemService.CalculateTotalPrice(sale.SaleItems);
        sale.TotalAmountReceivable = await _saleItemService.CalculateTotalPriceReceivable(sale.SaleItems);
        sale.Discount = (sale.TotalAmountReceivable / sale.TotalAmount) - 1;
        sale.DateCreated = DateTime.Now;

        var request = new PixPaymentRequest((double)sale.TotalAmountReceivable,
            "Compra de produtos",
            createSaleDto.Client.Email,
            "CPF",
            createSaleDto.Client.Number);

        var paymentTicket = await _mercadoPagoService.CreatePixPayment(request);
        sale.PaymentId = paymentTicket.Id;



        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();


        var saleProducts = await _saleItemService.GetSaleProductsBySaleIdAsync2(sale.SaleItems, enterpriseId);
        var payment = await _mercadoPagoService.GetPaymentByIdAsync((long)sale.PaymentId);
        var returnObject = new SaleWithProductsDto(sale, saleProducts, payment);

        return returnObject;
    }

    public async Task<PagedResult<SaleWithProductsDto>> SearchSalesAsync(
    string? searchTerm,
    int pageNumber,
    int pageSize,
    string order = "desc",
    DateTime? startDate = null,
    DateTime? endDate = null,
    int? status = null)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        var query = _context.Sales
            .Where(s => s.EnterpriseId == enterpriseId)
            .Include(s => s.SaleItems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            if (int.TryParse(searchTerm, out int id))
            {
                query = query.Where(s => s.Id == id);
            }
            else
            {
                var productIds = await _context.Products
                    .Where(p => p.Name.Contains(searchTerm))
                    .Select(p => p.Id)
                    .ToListAsync();

                query = query.Where(s => s.SaleItems.Any(i => productIds.Contains(i.ProductId)));
            }
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status);
        }

        if (startDate.HasValue)
        {
            query = query.Where(s => s.DateCreated >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var endDateInclusive = endDate.Value.AddDays(1);
            query = query.Where(s => s.DateCreated < endDateInclusive);
        }

        query = order.ToLower() == "asc"
            ? query.OrderBy(s => s.DateCreated)
            : query.OrderByDescending(s => s.DateCreated);

        var totalCount = await query.CountAsync();
        var sales = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new List<SaleWithProductsDto>();
        foreach (var sale in sales)
        {
            var saleProducts = await _saleItemService.GetSaleProductsBySaleIdAsync(sale.Id);
            var payment = sale.PaymentId.HasValue
                ? await _mercadoPagoService.GetPaymentByIdAsync(sale.PaymentId.Value)
                : null;
            result.Add(new SaleWithProductsDto(sale, saleProducts, payment));
        }

        return new PagedResult<SaleWithProductsDto>(result, totalCount, pageNumber, pageSize);
    }

    public async Task<List<SaleWithProductsDto>> GetAllSaleWithProductsAsync()
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var sales = await _context.Sales
            .Where(c => c.EnterpriseId == enterpriseId)
            .ToListAsync();

        List<SaleWithProductsDto> returnList = new List<SaleWithProductsDto>();

        foreach (var sale in sales)
        {
            var saleProducts = await _saleItemService.GetSaleProductsBySaleIdAsync(sale.Id);
            var payment = await _mercadoPagoService.GetPaymentByIdAsync((long)sale.PaymentId);
            var newReturnListDto = new SaleWithProductsDto(sale, saleProducts, payment);
            returnList.Add(newReturnListDto);
        }

        return returnList;
    }


    public async Task<Sale?> GetSaleByIdAsync(int id)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        return await _context.Sales
        .Where(c => c.EnterpriseId == enterpriseId && c.Id == id)
        .FirstOrDefaultAsync();
    }

    public async Task<SaleWithProductsDto?> GetSaleByIdWithProductsAsync(int id)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var sale = await _context.Sales
            .Where(c => c.EnterpriseId == enterpriseId && c.Id == id)
            .FirstOrDefaultAsync();
        var payment = await _mercadoPagoService.GetPaymentByIdAsync((long)sale.PaymentId);


        if (sale == null) return null;

        var saleProducts = await _saleItemService.GetSaleProductsBySaleIdAsync(sale.Id);
        return new SaleWithProductsDto(sale, saleProducts, payment);
    }
}

