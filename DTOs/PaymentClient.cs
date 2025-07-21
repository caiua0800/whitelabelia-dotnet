using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace backend.DTOs;

public class PaymentClient
{
    private readonly HttpClient _httpClient;
    private readonly string _accessToken;

    public PaymentClient(string accessToken)
    {
        _accessToken = accessToken;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.mercadopago.com/v1/"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<Payment> CreateAsync(PaymentCreateRequest request)
    {
        try
        {
            // Adicione o header X-Idempotency-Key (usando GUID ou timestamp)
            var idempotencyKey = Guid.NewGuid().ToString();
            _httpClient.DefaultRequestHeaders.Add("X-Idempotency-Key", idempotencyKey);

            var response = await _httpClient.PostAsJsonAsync("payments", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro na API Mercado Pago: {response.StatusCode} - {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<Payment>();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Falha na comunicação com o Mercado Pago", ex);
        }
    }
}