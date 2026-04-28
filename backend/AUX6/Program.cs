using AUX6;
using AUX6.Services;
using AUX6.Repository;


var builder = WebApplication.CreateBuilder(args);


//swagger 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure the HTTP request pipeline.

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

builder.Services.AddHttpClient("Bitacora");
builder.Services.AddHttpClient("AUX1");

builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

builder.Services.AddScoped<ICentroCostoRepository, CentroCostoRepository>();

builder.Services.AddScoped<ICentroCostoService, CentroCostoService>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IAux1Service, Aux1Service>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapCentroCostoEndpoints();

app.Run();


