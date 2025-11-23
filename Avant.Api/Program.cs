using Avant.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Avant.Api.Services;
using Avant.Api.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddHealthChecks();

// Versionamento
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader(); 
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? "ChaveSuperSecretaAvant2025!@#_MUUITO_GRANDE_123";
var jwtIssuer = jwtSection["Issuer"] ?? "AvantApi";
var jwtAudience = jwtSection["Audience"] ?? "AvantClientes";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Banco de Dados (Oracle no runtime)
builder.Services.AddDbContext<AvantDbContext>(options =>
{
    if (builder.Environment.IsEnvironment("Testing"))
    {
        options.UseInMemoryDatabase("AvantDbTests");
    }
    else
    {
        options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

// Serviços
builder.Services.AddScoped<IServicoToken, ServicoToken>();
builder.Services.AddSingleton<PlanoCarreiraMlService>();

builder.Services.AddEndpointsApiExplorer();

// Swagger com múltiplas versões
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

var app = builder.Build();

// Swagger UI com múltiplas versões
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            $"API Avant {description.GroupName.ToUpperInvariant()}"
        );
    }

    options.RoutePrefix = "swagger";
});

app.UseDefaultFiles();
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Logging + traceId
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
        .CreateLogger("Requisicao");
    var traceId = Guid.NewGuid().ToString();

    context.Response.Headers["X-Trace-Id"] = traceId;
    logger.LogInformation("TraceId {TraceId} - {Method} {Path}",
        traceId, context.Request.Method, context.Request.Path);

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

// Migrações (fora do ambiente de teste)
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AvantDbContext>();
        await context.Database.MigrateAsync();
    }
}

app.Run();

public partial class Program { }
