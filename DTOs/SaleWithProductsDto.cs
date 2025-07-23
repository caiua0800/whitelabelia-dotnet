using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.DTOs;
using backend.Interfaces;

namespace backend.Models;

public class SaleWithProductsDto
{

    [Column("sale")]
    public Sale Sale { get; set; }

    [Column("products")]
    public List<SaleProductDto> Products { get; set; }

    [Column("payment")]
    public Payment? Payment { get; set; }

    [Column("enterprise_id")]
    public int? EnterpriseId { get; set; }

    public SaleWithProductsDto(Sale sale, List<SaleProductDto> products, Payment payment, int? enterpriseId)
    {
        Sale = sale;
        Products = products;
        Payment = payment;
        EnterpriseId = enterpriseId;
    }

    public SaleWithProductsDto(Sale sale, List<SaleProductDto> products, Payment payment)
    {
        Sale = sale;
        Products = products;
        Payment = payment;
    }

}

