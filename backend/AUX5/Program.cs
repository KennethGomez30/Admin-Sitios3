using AUX5;
using AUX5.Repository;
using AUX5.Services;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS abierto para el ecosistema de microservicios
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// HttpClients hacia microservicios externos
builder.Services.AddHttpClient("Bitacora");
builder.Services.AddHttpClient("AUX1");

// Infraestructura de base de datos
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// Repositorio y servicios
builder.Services.AddScoped<ITerceroRepository, TerceroRepository>();
builder.Services.AddScoped<ITerceroService, TerceroService>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IAux1Service, Aux1Service>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.MapTercerosEndpoints();
app.Run();