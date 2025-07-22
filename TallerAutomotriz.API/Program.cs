using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TallerAutomotriz.DataAccess.Data;
using TallerAutomotriz.DataAccess.Interfaces;
using TallerAutomotriz.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_Taller"; 
builder.Services.AddCors();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Inyectar Repositorios
builder.Services.AddScoped<IUsuario, UsuarioAD>();
builder.Services.AddScoped<IRepuesto, RepuestoAD>();
builder.Services.AddScoped<ISolicitudRepuesto, SolicitudRepuestoAD>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseCors(options =>
{
    options.WithOrigins("https://localhost:44360", "http://localhost:44360")
        .AllowAnyHeader()
        .AllowAnyMethod();
});

app.UseAuthorization();
app.MapControllers();

app.Run();
