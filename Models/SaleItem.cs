using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using backend.Interfaces;
using backend.Models;

public class SaleItem : IHasEnterpriseId
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("product_unity_price")]
    public double? ProductUnityPrice { get; set; }

    [Column("product_qtt")]
    public double ProductQtt { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("sale_id")]
    public int? SaleId { get; set; }

    [Column("total_amount")]
    public double? TotalAmount { get; set; }

    [Column("discount")]
    public double? Discount { get; set; }

    [Column("enterprise_id")]
    public int EnterpriseId { get; set; }
}