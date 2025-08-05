using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.DTOs;
using Microsoft.Extensions.Logging;

namespace backend.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly MercadoPagoService _mercadoPagoService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        MercadoPagoService mercadoPagoService,
        ILogger<PaymentsController> logger)
    {
        _mercadoPagoService = mercadoPagoService;
        _logger = logger;
    }

    [HttpPost("pix")]
    public async Task<IActionResult> CreatePixPaymentMercadoPago([FromBody] PixPaymentRequest request)
    {
        try
        {
            var result = await _mercadoPagoService.CreatePixPayment(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pagamento PIX");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("signature-pix")]
    public async Task<IActionResult> CreateSignaturePixPaymentMercadoPago([FromBody] PixPaymentRequest request)
    {
        try
        {
            var result = await _mercadoPagoService.CreateSignaturePixPayment(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pagamento PIX");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("verify/{paymentId}")]
    public async Task<IActionResult> VerifyPaymentStatus(long paymentId)
    {
        try
        {
            var result = await _mercadoPagoService.VerifyAndUpdatePaymentStatus(paymentId);

            if (result == null)
            {
                return Ok(new { message = "Pagamento ainda não foi aprovado", status = "pending" });
            }

            return Ok(new
            {
                message = "Assinatura atualizada com sucesso",
                status = "paid",
                subscription = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao verificar status do pagamento ID: {paymentId}");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("status/{paymentId}")]
    public async Task<IActionResult> GetPaymentStatus(long paymentId)
    {
        try
        {
            var result = await _mercadoPagoService.GetPaymentStatusAsync(paymentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao consultar status do pagamento ID: {paymentId}");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("boleto")]
    public async Task<IActionResult> CreateBoletoPaymentMercadoPago([FromBody] BoletoPaymentRequest request)
    {
        try
        {
            var result = await _mercadoPagoService.CreateBoletoPayment(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar boleto");
            return BadRequest(new { error = ex.Message });
        }
    }
}