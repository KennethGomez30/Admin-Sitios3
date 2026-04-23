using AUX7;
using AUX7.Repository;
using AUX7.Services;
using Microsoft.AspNetCore.Connections;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// HttpClient para Bitácora
builder.Services.AddHttpClient("Bitacora");

// Infraestructura de base de datos
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

// Repositorio
builder.Services.AddScoped<ICentroCostoRepository, CentroCostoRepository>();

// Servicios
builder.Services.AddScoped<ICentroCostoService, CentroCostoService>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapCentroCostoEndpoints();

app.Run();
