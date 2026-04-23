using Bitacora;
using Bitacora.Repository;
using Bitacora.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS abierto para que cualquier microservicio del ecosistema pueda consumirlo
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

// Infraestructura de base de datos
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// Capas de repositorio y servicio
builder.Services.AddScoped<IBitacoraRepository, BitacoraRepository>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Debe estar antes de mapear endpoints
app.UseCors("AllowAll");

app.MapBitacoraEndpoints();
app.Run();