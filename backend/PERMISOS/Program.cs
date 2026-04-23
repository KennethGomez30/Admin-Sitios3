using PERMISOS;
using PERMISOS.Repository;
using PERMISOS.Services;

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

// HttpClients hacia microservicios externos
builder.Services.AddHttpClient("AUX1");

// Infraestructura de base de datos
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

// Capas de repositorio y servicio
builder.Services.AddScoped<IPermisosRepository, PermisosRepository>();
builder.Services.AddScoped<IPermisosService, PermisosService>();

// Cliente HTTP de validación de tokens contra AUX1
builder.Services.AddScoped<IAux1Service, Aux1Service>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapPermisosEndpoints();
app.Run();