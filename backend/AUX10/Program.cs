using AUX10;
using AUX10.Repository;
using AUX10.Services;


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

builder.Services.AddHttpClient("Bitacora");
builder.Services.AddHttpClient("AUX1");

builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

//Repository 
builder.Services.AddScoped<IReporteRepository, ReporteRepository>();

// Services
builder.Services.AddScoped<IReporteService, ReporteService>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IAux1Service, Aux1Service>();


// Add services to the container.

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapReporteCentroCostoEndpoints();

app.Run();

