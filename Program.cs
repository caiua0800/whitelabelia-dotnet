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
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CategoryService>();
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
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddSingleton<WebSocketConnections>();
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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


app.UseAuthentication();

app.UseMiddleware<TenantMiddleware>();

app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
