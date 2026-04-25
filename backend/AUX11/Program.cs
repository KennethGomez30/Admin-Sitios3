using AUX11;
using AUX11.Entities;
using AUX11.Repository;
using AUX11.Services;
using Dapper;                                             

var builder = WebApplication.CreateBuilder(args);

DefaultTypeMap.MatchNamesWithUnderscores = true;           

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
builder.Services.AddScoped<IDireccionRepository, DireccionRepository>();

// Servicios
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IDireccionService, DireccionService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapDireccionEndpoints();

app.Run();