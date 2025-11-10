using Microsoft.EntityFrameworkCore;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// ===== CONFIGURAR DbContext =====
builder.Services.AddDbContext<MindCareDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== CONFIGURAR UNIT OF WORK =====
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


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

app.UseAuthorization();

app.MapControllers();

app.Run();
