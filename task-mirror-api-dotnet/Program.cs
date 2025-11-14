using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Oracle.ManagedDataAccess.Client;
using TaskMirror.Auth;
using TaskMirror.Data;
using TaskMirror.Mapping;
using TaskMirror.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.DependencyInjection;



var builder = WebApplication.CreateBuilder(args);

// =====================================================
// Swagger + JWT (Authorize)
// =====================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Documento básico
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskMirror API",
        Version = "v1",
        Description = "API de gestão de tarefas com líderes e colaboradores (JWT + Roles)."
    });

    // Esquema de segurança JWT
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Informe: Bearer {seu_token_jwt}",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    // Exige o esquema em TODAS as operações (ícone de cadeado + botão Authorize)
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

// =======================================
// IA - Semantic Kernel + Ollama Cloud
// =======================================
#pragma warning disable SKEXP0010 // custom endpoint OpenAI-compatible

builder.Services.AddSingleton<IChatCompletionService>(sp =>
{
    var kernelBuilder = Kernel.CreateBuilder();

    var apiKey = Environment.GetEnvironmentVariable("OLLAMA_API_KEY")
        ?? throw new InvalidOperationException("Defina a variável de ambiente OLLAMA_API_KEY com sua API key da Ollama.");

    // Modelo cloud da Ollama
    var modelId = "gpt-oss:120b-cloud";

    kernelBuilder.AddOpenAIChatCompletion(
        modelId: modelId,
        apiKey: apiKey,
        endpoint: new Uri("https://ollama.com/v1")
    );

    var kernel = kernelBuilder.Build();

    // Pega o serviço de chat do kernel
    return kernel.GetRequiredService<IChatCompletionService>();
});

#pragma warning restore SKEXP0010;
// =====================================================
// Controllers (evita ciclos na serialização)
// =====================================================
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.WriteIndented = true;
    });

// =====================================================
// AutoMapper
// =====================================================
builder.Services.AddAutoMapper(typeof(TaskMirrorProfile));

// =====================================================
// Oracle DbContext (EF Core)
// =====================================================
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TaskMirrorDbContext>(o =>
{
    o.UseOracle(cs);
});

// =====================================================
// Health Checks
// =====================================================
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<TaskMirrorDbContext>(
        name: "oracle-db",
        tags: new[] { "ready" }
    );

// =====================================================
// Serviços de domínio
// =====================================================
builder.Services.AddScoped<TarefaService>();
builder.Services.AddScoped<OllamaIaService>();

// =====================================================
// Auth: JWT + Authorization
// =====================================================
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.ASCII.GetBytes(jwtSection["Key"] ?? "CHAVE_SECRETA_DEV");

// Serviço que gera o token
builder.Services.AddScoped<TokenService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // dev
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// =====================================================
// Swagger
// =====================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =====================================================
// Migrations + Seed (Oracle)
// =====================================================
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<TaskMirrorDbContext>();
        db.Database.Migrate();
        DbInitializer.Seed(db);
        logger.LogInformation("✅ Migração e Seed executados com sucesso.");
    }
    catch (OracleException ex)
    {
        logger.LogError(ex, "❌ Oracle indisponível no startup. API vai subir mesmo assim.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Erro inesperado ao migrar/seed. API vai subir mesmo assim.");
    }
}

app.UseHttpsRedirection();

// Auth
app.UseAuthentication();
app.UseAuthorization();

// Health
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = reg => reg.Tags.Contains("ready"),
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                error = e.Value.Exception?.Message
            })
        });
        await ctx.Response.WriteAsync(json);
    }
});

app.MapControllers();
app.Run();

// Necessário para testes de integração
public partial class Program { }
