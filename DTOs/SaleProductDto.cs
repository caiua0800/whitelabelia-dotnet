using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.DTOs;
using backend.Interfaces;

namespace backend.Models;

public class SaleProductDto
{

    [Column("product_id")]
    public int? ProductId { get; set; }

    [Column("product_name")]
    public string? ProductName { get; set; }

    [Column("product_unity_price")]
    public double? ProductUnityPrice { get; set; }

    [Column("product_qtt")]
    public double ?ProductQtt { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("total_amount")]
    public double? TotalAmount { get; set; }

    [Column("discount")]
    public double? Discount { get; set; }

    public SaleProductDto()
    {

    }

    public SaleProductDto(int productId, string productName, double productUnityPrice, double productQtt, double totalAmount, string description, double discount)
    {
        ProductId = productId;
        ProductName = productName;
        ProductUnityPrice = productUnityPrice;
        ProductQtt = productQtt;
        TotalAmount = totalAmount;
        Description = description;
        Discount = discount;
    }
   
}

