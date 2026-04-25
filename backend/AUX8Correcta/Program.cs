using AUX8Correcta;
using AUX8Correcta.Endpoints;
using AUX8Correcta.Repository;
using AUX8Correcta.Services;
using Microsoft.AspNetCore.Connections;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

builder.Services.AddScoped<Itercerosrepository, TercerosRepository>();
builder.Services.AddScoped<ITercerosservice, TercerosService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapTercerosEndpoints();

app.Run();
