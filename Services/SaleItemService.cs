using backend.Models;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;
using System.Globalization;
using backend.DTOs;

namespace backend.Services;

public class SaleItemService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly ProductService _productService;

    public SaleItemService(ApplicationDbContext context, ITenantService tenantService, ProductService productService)
    {
        _context = context;
        _tenantService = tenantService;
        _productService = productService;
    }

    public async Task<IEnumerable<SaleItem>> GetSaleItemsBySaleIdAsync(int saleId)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        return await _context.SaleItems
            .Where(si => si.SaleId == saleId && si.EnterpriseId == enterpriseId)
            .ToListAsync();
    }

    public async Task<List<SaleProductDto>> GetSaleProductsBySaleIdAsync(int saleId)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var saleItems = await _context.SaleItems
            .Where(si => si.SaleId == saleId && si.EnterpriseId == enterpriseId)
            .ToListAsync();


        var saleProducts = new List<SaleProductDto>();

        Console.WriteLine($"saleItems: {saleItems.Count}");

        foreach (var saleItem in saleItems)
        {

            var product = await _productService.GetProductById(saleItem.ProductId);
            Console.WriteLine($"product.Id: {product.Id}");

            var productDto = new SaleProductDto(
                product.Id,
                product.Name,
                product.UnityPrice,
                saleItem.ProductQtt,
                (double)(saleItem.ProductQtt * product.UnityPrice - ((saleItem.ProductQtt * product.UnityPrice) * saleItem.Discount)),
                saleItem.Description,
                (double)saleItem.Discount
            );
            saleProducts.Add(productDto);
        }

        return saleProducts;
    }

    public async Task<List<SaleProductDto>> GetSaleProductsBySaleIdAsync2(List<SaleItem> saleItems, int enterpriseId)
    {


        var saleProducts = new List<SaleProductDto>();

        Console.WriteLine($"saleItems: {saleItems.Count}");

        foreach (var saleItem in saleItems)
        {

            var product = await _productService.GetProductById2(saleItem.ProductId);
            Console.WriteLine($"product.Id: {product.Id}");

            var productDto = new SaleProductDto(
                product.Id,
                product.Name,
                product.UnityPrice,
                saleItem.ProductQtt,
                (double)(saleItem.ProductQtt * product.UnityPrice - ((saleItem.ProductQtt * product.UnityPrice) * saleItem.Discount)),
                saleItem.Description,
                (double)saleItem.Discount
            );
            saleProducts.Add(productDto);
        }

        return saleProducts;
    }

    public async Task<double> CalculateTotalPrice(IEnumerable<SaleItem> saleItems)
    {
        double totalPrice = 0;
        foreach (var item in saleItems)
        {
            var product = await _productService.GetProductById2(item.ProductId);
            totalPrice += item.ProductQtt * product.UnityPrice;
        }
        return totalPrice;
    }

    public async Task<double> CalculateTotalPriceReceivable(IEnumerable<SaleItem> saleItems)
    {
        double totalPriceReceivable = 0;
        foreach (var item in saleItems)
        {
            var product = await _productService.GetProductById2(item.ProductId);
            var productTotalPrice = item.ProductQtt * product.UnityPrice;
            totalPriceReceivable += (double)(productTotalPrice - (productTotalPrice * item.Discount));
        }
        return totalPriceReceivable;
    }

    public async Task<double> CalculateTotalDiscount(IEnumerable<SaleItem> saleItems)
    {
        double totalDiscount = 0;
        foreach (var item in saleItems)
        {
            var product = await _productService.GetProductById(item.ProductId);
            var productTotalPrice = item.ProductQtt * product.UnityPrice;
            totalDiscount += (double)(productTotalPrice * item.Discount);
        }
        return totalDiscount;
    }
}