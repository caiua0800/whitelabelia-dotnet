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

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ICredentialGeneratorService, CredentialGeneratorService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddSingleton<WebSocketConnections>();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// CONFIGURAR AUTENTICAÇÃO JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Opcional: configure para true se você quiser validar o emissor
            ValidateAudience = false, // Opcional: configure para true se quiser validar o público
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])) // Sua chave secreta aqui
        };
    });

var app = builder.Build();
app.UseWebSockets();
app.UseCors("AllowAll");

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        var buffer = new byte[1024 * 4];
        
        // Adicione a conexão a um grupo de conexões ativas
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


// Use Authentication antes dos Controllers!
app.UseAuthentication();

// Adicione o middleware de tenant antes dos controllers
app.UseMiddleware<TenantMiddleware>();

// Authorization precisa vir depois da autenticação
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
