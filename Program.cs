using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Middlewares;
using backend.Services;
using backend.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Cors;
using System.Net.WebSockets;
using DinkToPdf.Contracts;
using DinkToPdf;

var builder = WebApplication.CreateBuilder(args);

// Configuração de serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS configurado para produção e desenvolvimento
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
                "https://demelloagent.app",          // Frontend na Vercel
                "https://backend.demelloagent.app",  // Seu próprio backend
                "http://localhost:3000"              // Frontend local
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();  // Importante para WebSockets e autenticação
    });
});

// Registro de serviços (mantido conforme seu original)
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IChatService, ChatService>();
// ... (outros serviços permanecem iguais)

// Configuração do banco de dados
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração de autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
        
        // Configuração especial para WebSockets
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/ws"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// Configuração do pipeline HTTP
app.UseHttpsRedirection();
app.UseRouting();

// Habilitar CORS antes de outros middlewares
app.UseCors("AllowSpecificOrigins");

// Middlewares de autenticação/autorização
app.UseAuthentication();
app.UseAuthorization();

// WebSocket deve vir após CORS e Auth
app.UseWebSockets();

// Configuração do endpoint WebSocket
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        var buffer = new byte[1024 * 4];
        
        var webSocketConnections = context.RequestServices.GetRequiredService<WebSocketConnections>();
        var connectionId = webSocketConnections.AddConnection(ws);

        try
        {
            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    webSocketConnections.RemoveConnection(connectionId);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro WebSocket: {ex.Message}");
            webSocketConnections.RemoveConnection(connectionId);
        }
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

// Middleware customizado
app.UseMiddleware<TenantMiddleware>();

// Swagger apenas em desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Mapeamento de controllers
app.MapControllers();

// Configuração final de URLs
app.Urls.Add("http://0.0.0.0:5097");  // Para comunicação interna com Nginx
app.Urls.Add("https://0.0.0.0:5098"); // Opcional: HTTPS direto (se necessário)

app.Run();