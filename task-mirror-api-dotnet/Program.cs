using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using TaskMirror.Data;
using TaskMirror.Mapping;
using TaskMirror.Services;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Controllers
builder.Services.AddControllers();

// AutoMapper
builder.Services.AddAutoMapper(typeof(TaskMirrorProfile));

// Oracle DbContext
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TaskMirrorDbContext>(o => o.UseOracle(cs));

// Health Checks
builder.Services
    .AddHealthChecks()
    // readiness: verifica se o EF/Oracle está OK
    .AddDbContextCheck<TaskMirrorDbContext>(
        name: "oracle-db",
        tags: new[] { "ready" }
    );

// Serviços de domínio
builder.Services.AddScoped<TarefaService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Aplica migrations e roda o seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskMirrorDbContext>();
    db.Database.Migrate();   // cria/atualiza o schema no Oracle
    DbInitializer.Seed(db);  // popula dados básicos
}

app.UseHttpsRedirection();

// Endpoints de Health
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    // Liveness não executa nenhum check (só confirma que o processo está de pé)
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    // Readiness roda só os checks com tag "ready" (DbContext/Oracle)
    Predicate = reg => reg.Tags.Contains("ready"),
    // (opcional) retornar JSON simples com status
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new {
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

public partial class Program { }
