using AUX8Correcta;
using AUX8Correcta.Endpoints;
using AUX8Correcta.Repository;
using AUX8Correcta.Services;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AUX8Correcta API",
        Version = "v1"
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// HttpClient para AUTH (AUX1)
builder.Services.AddHttpClient("AUX1", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["AUX1Service:BaseUrl"]!
    );
});
// DB
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// Services
builder.Services.AddScoped<Itercerosrepository, TercerosRepository>();
builder.Services.AddScoped<ITercerosservice, TercerosService>();
builder.Services.AddScoped<IAux1Service, Aux1Service>();

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.SwaggerEndpoint("v1/swagger.json", "AUX8Correcta v1");
});

// Middlewares
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Endpoints
app.MapTercerosEndpoints();

app.Run();