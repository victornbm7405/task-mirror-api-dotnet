using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMirror.Data;
using TaskMirror.DTOs;
using TaskMirror.Models;
using TaskMirror.Services;

namespace TaskMirror.Controllers;

[ApiController]
[Route("api/v1/tarefas")]
public class TarefasController : ControllerBase
{
    private readonly TaskMirrorDbContext _db;
    private readonly IMapper _mapper;
    private readonly TarefaService _service;

    public TarefasController(TaskMirrorDbContext db, IMapper mapper, TarefaService service)
    {
        _db = db; _mapper = mapper; _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<object>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.Tarefas.AsNoTracking().OrderBy(t => t.Id);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var result = new PagedResult<TarefaDto>
        {
            Items = _mapper.Map<IEnumerable<TarefaDto>>(items),
            Total = total,
            Page = page,
            PageSize = pageSize
        };

        var links = Hateoas.BuildListLinks($"{Request.Path}", page, pageSize, total);
        Response.Headers.Append("Link", string.Join(", ", links.Select(l => $"<{l.Href}>; rel=\"{l.Rel}\"")));

        return Ok(new { result.Items, result.Total, result.Page, result.PageSize, links });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TarefaDto>> GetById(int id)
    {
        var e = await _db.Tarefas.FindAsync(id);
        return e is null ? NotFound() : Ok(_mapper.Map<TarefaDto>(e));
    }

    [HttpPost]
    public async Task<ActionResult<TarefaDto>> Post(TarefaCreateDto dto)
    {
        var e = _mapper.Map<Tarefa>(dto);
        _db.Tarefas.Add(e);
        await _db.SaveChangesAsync();
        var outDto = _mapper.Map<TarefaDto>(e);
        return CreatedAtAction(nameof(GetById), new { id = e.Id }, outDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, TarefaUpdateDto dto)
    {
        var e = await _db.Tarefas.FindAsync(id);
        if (e is null) return NotFound();
        _mapper.Map(dto, e);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.Tarefas.FindAsync(id);
        if (e is null) return NotFound();
        _db.Tarefas.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/finalizar")]
    public async Task<ActionResult<FinalizarTarefaResponse>> Finalizar(int id)
    {
        try
        {
            var (tarefa, fb) = await _service.FinalizarAsync(id);
            return Ok(new FinalizarTarefaResponse(tarefa.Id, tarefa.TempoRealMin, fb.Nota, fb.Comentario));
        }
        catch (System.InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
