using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using LibreriaAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. CONEXIÓN A LA BASE DE DATOS (SQLite)
builder.Services.AddDbContext<LibreriaContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. AGREGAR SOPORTE PARA CONTROLADORES
// 2. AGREGAR SOPORTE PARA CONTROLADORES (Con bloqueo de ciclos infinitos)
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// 3. CONFIGURACIÓN DE SWAGGER (Para probar la API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. CONFIGURAR EL PIPELINE DE REQUISITOS HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Esto levanta la interfaz gráfica de Swagger
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();