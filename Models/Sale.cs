using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.DTOs;
using backend.Interfaces;

namespace backend.Models;

public class Sale : IHasEnterpriseId
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("chat_id")]
    public string ClientId { get; set; }

    [Column("total_amount")]
    public double? TotalAmount { get; set; }

    [Column("enterprise_id")]
    public int EnterpriseId { get; set; }

    [Column("total_amount_receivable")]
    public double? TotalAmountReceivable { get; set; }

    [Column("discount")]
    public double? Discount { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("payment_id")]
    public long? PaymentId { get; set; }

    [Column("status")]
    public int? Status { get; set; }

    [Column("tracking_url")]
    public string? TrackingUrl { get; set; }

    [Column("date_created")]
    public DateTime? DateCreated { get; set; }

    [Column("sale_items")]
    public List<SaleItem>? SaleItems { get; set; }
}

