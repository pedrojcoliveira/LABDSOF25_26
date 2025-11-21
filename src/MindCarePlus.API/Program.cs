using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MindCarePlus.Application.Configuration;
using MindCarePlus.Application.Interfaces;
using MindCarePlus.Application.Services;
using MindCarePlus.API.Services;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Domain.Services;
using MindCarePlus.Infrastructure.Data;
using MindCarePlus.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// ===== CONFIGURAR DbContext =====
builder.Services.AddDbContext<MindCareDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// ===== CONFIGURAR JWT SETTINGS =====
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

// ===== CONFIGURAR AUTENTICAÇÃO JWT =====
var secretKey = jwtSettings.Get<JwtSettings>()?.SecretKey
                ?? throw new InvalidOperationException("JWT SecretKey não configurada");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Get<JwtSettings>()?.Issuer,
            ValidAudience = jwtSettings.Get<JwtSettings>()?.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero // Remove o delay padrão de 5 minutos
        };
    });

builder.Services.AddAuthorization();

// ===== CONFIGURAR SERVICES =====
// ===== SERVIÇOS =====
builder.Services.AddHttpClient();
builder.Services.AddScoped<IRelatorioIAService, RelatorioIAService>();

// ===== CONFIGURAR UNIT OF WORK =====
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IDesafioService, DesafioService>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== CONFIGURAR CORS =====
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowMobile",
//        policy =>
//        {
//            policy.WithOrigins("http://localhost", "https://localhost")
//                  .AllowAnyHeader()
//                  .AllowAnyMethod()
//                  .AllowCredentials();
//        });
//});

// ===== CONFIGURAR CORS ===== Vindo do Claude
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMobile", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Seed initial data
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<MindCareDbContext>();
        MindCareSeedData.Initialize(context);
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ===== USAR CORS ===== Vindo do Claude
app.UseCors("AllowMobile");

app.UseAuthorization();

app.MapControllers();

app.Run();
