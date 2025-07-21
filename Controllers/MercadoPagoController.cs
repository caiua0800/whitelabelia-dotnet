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