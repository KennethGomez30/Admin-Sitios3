using AUX9;
using AUX9.Repository;
using AUX9.Services;

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

// HttpClient para AUX1 (validación de tokens)
builder.Services.AddHttpClient("AUX1");

// Infraestructura de base de datos
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

// Repositorio
builder.Services.AddScoped<ITerceroRepository, TerceroRepository>();

// Servicios
builder.Services.AddScoped<ITerceroService, TerceroService>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IAux1Service, Aux1Service>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapTerceroEndpoints();

app.Run();