using AUX1;
using AUX1.Repository;
using AUX1.Services;

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

// HttpClient para PERMISOS
builder.Services.AddHttpClient("Permisos");

// Infraestructura de base de datos
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

// Repositorio
builder.Services.AddScoped<IAutenticacionRepository, AutenticacionRepository>();

// Servicios
builder.Services.AddScoped<IAutenticacionService, AutenticacionService>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IPermisosService, PermisosService>();  // cliente HTTP PERMISOS
builder.Services.AddSingleton<IJwtService, JwtService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapAutenticacionEndpoints();
app.Run();