using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TaskMirror.Controllers;

[ApiController]
[Route("api/v1/health")]
[AllowAnonymous] // normalmente health é aberto, mas você pode exigir auth se quiser
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthChecks;

    public HealthController(HealthCheckService healthChecks)
    {
        _healthChecks = healthChecks;
    }

    // GET api/v1/health/live
    [HttpGet("live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Live()
    {
        // Aqui geralmente não checamos nada complexo, só respondemos "ok"
        var report = await _healthChecks.CheckHealthAsync(_ => false);
        return Ok(new
        {
            status = report.Status.ToString()
        });
    }

    // GET api/v1/health/ready
    [HttpGet("ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Ready()
    {
        var report = await _healthChecks.CheckHealthAsync(
            reg => reg.Tags.Contains("ready"));

        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                error = e.Value.Exception?.Message
            })
        };

        return Ok(result);
    }
}
