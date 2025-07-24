using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

var jwtSecretKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtSecretKey))
{
    throw new InvalidOperationException("Jwt:Key no está configurado en appsettings.json.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
        };
    });

builder.Services.AddAuthorization();

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
