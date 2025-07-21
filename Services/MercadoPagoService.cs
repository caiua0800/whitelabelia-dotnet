using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class MercadoPagoService
{
    private readonly PaymentClient _paymentClient;
    private readonly ApplicationDbContext _context;

    public MercadoPagoService(IConfiguration configuration, ApplicationDbContext context)
    {
        var accessToken = configuration["MercadoPago:AccessToken"];
        _paymentClient = new PaymentClient(accessToken);
        _context = context;
    }

    public async Task<Payment> CreatePixPayment(PixPaymentRequest request)
    {
        try
        {
            var paymentRequest = new PaymentCreateRequest
            {
                TransactionAmount = request.Transaction_amount,
                Description = request.Description,
                PaymentMethodId = "pix",
                Payer = new PaymentPayerRequest
                {
                    Email = request.Email,
                    FirstName = "Nome", // Obrigatório mesmo para PIX
                    LastName = "Sobrenome", // Obrigatório mesmo para PIX
                    Identification = new PaymentPayerIdentificationRequest
                    {
                        Type = request.IdentificationType,
                        Number = request.Number
                    }
                },
                DateOfExpiration = DateTime.UtcNow.AddHours(1).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffzzz")
            };

            var mercadoPagoPayment = await _paymentClient.CreateAsync(paymentRequest);

            var payment = new Payment
            {
                Id = mercadoPagoPayment.Id,
                Status = mercadoPagoPayment.Status,
                StatusDetail = mercadoPagoPayment.StatusDetail,
                PaymentMethodId = mercadoPagoPayment.PaymentMethodId,
                PointOfInteraction = mercadoPagoPayment.PointOfInteraction,
                TransactionDetails = mercadoPagoPayment.TransactionDetails,
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return payment;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao criar pagamento PIX", ex);
        }
    }

    public async Task<Payment?> GetPaymentByIdAsync(long id)
    {
        return await _context.Payments
            .Where(si => si.Id == id)
            .FirstOrDefaultAsync();
    }


    public async Task<Payment> CreateBoletoPayment(BoletoPaymentRequest request)
    {
        try
        {
            var paymentRequest = new PaymentCreateRequest
            {
                TransactionAmount = request.Transaction_amount,
                Description = request.Description,
                PaymentMethodId = "bolbradesco",
                Payer = new PaymentPayerRequest
                {
                    Email = request.Email,
                    FirstName = request.First_name,
                    LastName = request.Last_name,
                    Identification = new PaymentPayerIdentificationRequest
                    {
                        Type = request.IdentificationType,
                        Number = request.Number
                    },
                    Address = new PaymentPayerAddressRequest
                    {
                        ZipCode = request.Zip_code,
                        StreetName = request.Street_name,
                        StreetNumber = request.Street_number,
                        Neighborhood = request.Neighborhood,
                        City = request.City,
                        FederalUnit = request.Federal_unit
                    }
                },
                DateOfExpiration = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")
            };

            return await _paymentClient.CreateAsync(paymentRequest);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao criar boleto", ex);
            Console.WriteLine(ex.ToString());
        }
    }
}