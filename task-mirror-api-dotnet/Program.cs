using System.Linq;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using TaskMirror.Data;
using TaskMirror.Mapping;
using TaskMirror.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// Swagger
// =====================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    // Opcional: se quiser ver SQL no log:
    // o.EnableSensitiveDataLogging();
});

// =====================================================
// Health Checks
// - liveness: confirma que o processo está vivo
// - readiness: verifica dependências (Oracle/EF)
// =====================================================
builder.Services
    .AddHealthChecks()
    // readiness: verifica se o EF/Oracle está OK
    .AddDbContextCheck<TaskMirrorDbContext>(
        name: "oracle-db",
        tags: new[] { "ready" }
    );

// =====================================================
// Serviços de domínio
// =====================================================
builder.Services.AddScoped<TarefaService>();

var app = builder.Build();

// =====================================================
// Swagger (apenas em Development)
// =====================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =====================================================
// Migrations + Seed (Oracle) — protegido para não derrubar a API
// =====================================================
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<TaskMirrorDbContext>();
        db.Database.Migrate();  // aplica migrations
        DbInitializer.Seed(db); // popula dados mínimos (status/tipos/usuarios/tarefas)
        logger.LogInformation("✅ Migração e Seed executados com sucesso.");
    }
    catch (OracleException ex)
    {
        logger.LogError(ex, "❌ Oracle indisponível no startup. API vai subir mesmo assim.");
        // Se precisar, limpe pools:
        // OracleConnection.ClearAllPools();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Erro inesperado ao migrar/seed. API vai subir mesmo assim.");
    }
}

app.UseHttpsRedirection();

// =====================================================
// Endpoints de Health
// =====================================================

// Liveness: sem checks — apenas indica que a API está de pé.
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    // Liveness não executa nenhum check (só confirma que o processo está vivo)
    Predicate = _ => false
});

// Readiness: roda os checks com tag "ready" (DbContext/Oracle)
// e retorna um JSON resumido do status.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    // Readiness roda só os checks com tag "ready" (DbContext/Oracle)
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
