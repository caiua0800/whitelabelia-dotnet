using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.DTOs;
using backend.Interfaces;

namespace backend.Models;

// public class SaleWithProductsDto
// {

//     [Column("sale")]
//     public Sale Sale { get; set; }

//     [Column("products")]
//     public List<SaleProductDto> Products { get; set; }

//     [Column("payment")]
//     public Payment? Payment { get; set; }

//     [Column("enterprise_id")]
//     public int? EnterpriseId { get; set; }

//     public SaleWithProductsDto(Sale sale, List<SaleProductDto> products, Payment payment, int? enterpriseId)
//     {
//         Sale = sale;
//         Products = products;
//         Payment = payment;
//         EnterpriseId = enterpriseId;
//     }

//     public SaleWithProductsDto(Sale sale, List<SaleProductDto> products, Payment payment)
//     {
//         Sale = sale;
//         Products = products;
//         Payment = payment;
//     }

// }


public class SaleWithProductsDto
{

    [Column("sale")]
    public Sale Sale { get; set; }

    [Column("products")]
    public List<SaleProductDto> Products { get; set; }

    [Column("payment_info")]
    public PaymentInfo? PaymentInfo { get; set; }

    [Column("enterprise_id")]
    public int? EnterpriseId { get; set; }

    public SaleWithProductsDto(Sale sale, List<SaleProductDto> products, PaymentInfo paymentInfo, int? enterpriseId)
    {
        Sale = sale;
        Products = products;
        PaymentInfo = paymentInfo;
        EnterpriseId = enterpriseId;
    }

        public SaleWithProductsDto(Sale sale, List<SaleProductDto> products, PaymentInfo paymentInfo)
    {
        Sale = sale;
        Products = products;
        PaymentInfo = paymentInfo;
    }
}

public class PaymentInfo{
    [Column("product_id")]
    public string? BeneficiaryName { get; set; }

    [Column("beneficiary_cpf_cnpj")]
    public string? BeneficiaryCpfCnpj { get; set; }

    [Column("bank_name")]
    public string? BankName { get; set; }

    [Column("pix_code")]
    public string? PixCode { get; set; }

    public PaymentInfo(string? beneficiaryName, string? beneficiaryCpfCnpj, string? bankName, string? pixCode)
    {
        BeneficiaryName = beneficiaryName;
        BeneficiaryCpfCnpj = beneficiaryCpfCnpj;
        BankName = bankName;
        PixCode = pixCode;
    }
}