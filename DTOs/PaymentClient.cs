using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using backend.Models;

namespace backend.DTOs;

public class PaymentClient
{
    private readonly HttpClient _httpClient;
    private readonly string _accessToken;

    public string AccessToken => _accessToken;

    public PaymentClient(string accessToken)
    {
        _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.mercadopago.com/v1/"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        ConfigureDefaultHeaders();
    }

    private void ConfigureDefaultHeaders()
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<Payment> CreateAsync(PaymentCreateRequest request)
    {
        try
        {
            var idempotencyKey = Guid.NewGuid().ToString();
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "payments")
            {
                Content = JsonContent.Create(request),
                Headers =
                {
                    { "X-Idempotency-Key", idempotencyKey }
                }
            };

            var response = await _httpClient.SendAsync(requestMessage);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro na API Mercado Pago: {response.StatusCode} - {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<Payment>() 
                ?? throw new InvalidOperationException("Resposta inválida da API");
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Falha na comunicação com o Mercado Pago", ex);
        }
    }

    public async Task<PaymentStatusResponse> GetPaymentStatusAsync(long paymentId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"payments/{paymentId}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro ao consultar pagamento: {response.StatusCode} - {errorContent}");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return await response.Content.ReadFromJsonAsync<PaymentStatusResponse>(options) 
                ?? throw new InvalidOperationException("Resposta inválida da API");
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Falha ao consultar status do pagamento", ex);
        }
    }

    public async Task<PaymentStatusResponse> GetPaymentStatusAsync(string paymentId)
    {
        if (!long.TryParse(paymentId, out var id))
        {
            throw new ArgumentException("ID de pagamento inválido", nameof(paymentId));
        }
        return await GetPaymentStatusAsync(id);
    }
}

