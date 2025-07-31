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
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configuração de serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3001",
        "http://localhost:3000",
        "https://sistema.demelloagent.app")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); 
    });
});

// Registro de serviços
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ICredentialGeneratorService, CredentialGeneratorService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<MessageModelService>();
builder.Services.AddScoped<EnterpriseService>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<SaleService>();
builder.Services.AddScoped<SaleItemService>();
builder.Services.AddScoped<MercadoPagoService>();
builder.Services.AddScoped<SubscriptionTypeService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ShotService>();
builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<IChatExportService, ChatExportService>();
builder.Services.AddScoped<IAgentService, AgentService>();

builder.Services.AddSingleton<WebSocketConnections>();
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
builder.Services.AddHttpContextAccessor();

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

// Habilitar CORS (deve vir antes de UseAuthentication e UseAuthorization)
app.UseCors("AllowAll");

// Middlewares de autenticação/autorização
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/ws"))
    {
        // Remove o prefixo "Bearer " se existir
        var accessToken = context.Request.Query["access_token"];
        if (!string.IsNullOrEmpty(accessToken))
        {
            context.Request.Headers["Authorization"] = $"Bearer {accessToken}";
        }
    }
    await next();
});

app.UseWebSockets();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        var buffer = new byte[1024 * 4];
        
        var connections = context.RequestServices.GetRequiredService<WebSocketConnections>();
        var connectionId = connections.AddConnection(ws);

        try
        {
            // Envia uma mensagem de confirmação imediatamente
            await connections.SendAsync(connectionId, 
                JsonSerializer.Serialize(new { type = "connection_established" }));

            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), 
                    CancellationToken.None);
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                        null, CancellationToken.None);
                    connections.RemoveConnection(connectionId);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket error: {ex.Message}");
            connections.RemoveConnection(connectionId);
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

app.Urls.Add("http://0.0.0.0:5097");

app.Run();