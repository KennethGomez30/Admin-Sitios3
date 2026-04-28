using AUX11;
using AUX11.Entities;
using AUX11.Repository;
using AUX11.Services;
using Dapper;                                             

var builder = WebApplication.CreateBuilder(args);

DefaultTypeMap.MatchNamesWithUnderscores = true;           

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Pega aquí tu token JWT (sin escribir 'Bearer ', solo el token)"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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
builder.Services.AddHttpClient("AUX1");

// Infraestructura de base de datos
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

// Repositorio
builder.Services.AddScoped<IDireccionRepository, DireccionRepository>();

// Servicios
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IDireccionService, DireccionService>();
builder.Services.AddScoped<IAux1Service, Aux1Service>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapDireccionEndpoints();

app.Run();