using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace backend.Services;

public class MercadoPagoService
{
    private readonly PaymentClient _paymentClient;
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly SubscriptionService _subscriptionService;

    public MercadoPagoService(IConfiguration configuration, ApplicationDbContext context, ITenantService tenantService, SubscriptionService subscriptionService)
    {
        var accessToken = configuration["MercadoPago:AccessToken"];
        _paymentClient = new PaymentClient(accessToken);
        _context = context;
        _tenantService = tenantService;
        _subscriptionService = subscriptionService;
    }

    public async Task<Payment> CreatePixPayment(PixPaymentRequest request)
    {
        try
        {
            var paymentRequest = new PaymentCreateRequest
            {
                TransactionAmount = (double)request.Transaction_amount,
                Description = request.Description,
                PaymentMethodId = "pix",
                Payer = new PaymentPayerRequest
                {
                    Email = request.Email,
                    FirstName = "Nome",
                    LastName = "Sobrenome",
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


    public async Task<Payment> CreateSignaturePixPayment(PixPaymentRequest request)
    {
        var enterprise = _tenantService.GetCurrentEnterprise();
        Console.WriteLine($"enterprise id: {enterprise.Id}");
        var subscriptionDetails = await _subscriptionService.GetSubscriptionByEnterpriseId(enterprise.Id);

        try
        {
            var paymentRequest = new PaymentCreateRequest
            {
                TransactionAmount = subscriptionDetails.SubscriptionType.Value,
                Description = request.Description,
                PaymentMethodId = "pix",
                Payer = new PaymentPayerRequest
                {
                    Email = enterprise.Email,
                    FirstName = "Nome",
                    LastName = "Sobrenome",
                    Identification = new PaymentPayerIdentificationRequest
                    {
                        Type = request.IdentificationType,
                        Number = enterprise.Cnpj
                    }
                },
                DateOfExpiration = DateTime.UtcNow.AddHours(1).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffzzz"),
            };

            var mercadoPagoPayment = await _paymentClient.CreateAsync(paymentRequest);

            var signaturePix = new SignaturePix
            {
                TicketId = mercadoPagoPayment.Id.ToString(),
                TicketUrl = mercadoPagoPayment.PointOfInteraction?.TransactionData?.TicketUrl,
                QrCode = mercadoPagoPayment.PointOfInteraction?.TransactionData?.QrCodeBase64
            };

            var subscription = await _context.Subscriptions
                .Where(s => s.EnterpriseId == enterprise.Id)
                .FirstOrDefaultAsync();

            if (subscription != null)
            {
                subscription.Ticket = signaturePix;
                _context.Subscriptions.Update(subscription);
            }

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

    public async Task<Subscription> VerifyAndUpdatePaymentStatus(long paymentId)
    {
        try
        {
            var paymentStatus = await _paymentClient.GetPaymentStatusAsync(paymentId);

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                throw new KeyNotFoundException("Pagamento não encontrado");
            }

            payment.Status = paymentStatus.Status;
            payment.StatusDetail = paymentStatus.StatusDetail;
            _context.Payments.Update(payment);

            if (paymentStatus.Status == "approved" || paymentStatus.Status == "paid")
            {
                var subscription = await _context.Subscriptions
                    .FirstOrDefaultAsync(s => s.Ticket != null && s.Ticket.TicketId == paymentId.ToString());

                if (subscription != null)
                {
                    await _subscriptionService.RenewSubscriptionResources(subscription.Id);
                    _context.Subscriptions.Update(subscription);
                }

                await _context.SaveChangesAsync();
                return subscription;
            }

            await _context.SaveChangesAsync();
            return null;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao verificar status do pagamento", ex);
        }
    }

    public async Task<PaymentStatusResponse> GetPaymentStatusAsync(long paymentId)
    {
        try
        {
            return await _paymentClient.GetPaymentStatusAsync(paymentId);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao consultar status do pagamento", ex);
        }
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