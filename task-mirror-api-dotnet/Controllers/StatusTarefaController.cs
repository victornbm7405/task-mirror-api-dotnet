using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMirror.Data;
using TaskMirror.DTOs;
using TaskMirror.Models;

namespace TaskMirror.Controllers;

[ApiController]
[Route("api/v1/status-tarefa")]
public class StatusTarefaController : ControllerBase
{
    private readonly TaskMirrorDbContext _db;
    private readonly IMapper _mapper;

    public StatusTarefaController(TaskMirrorDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    [HttpGet]
    public async Task<IEnumerable<StatusTarefaDto>> Get()
        => _mapper.Map<IEnumerable<StatusTarefaDto>>(await _db.StatusesTarefa.AsNoTracking().ToListAsync());

    [HttpPost]
    public async Task<ActionResult<StatusTarefaDto>> Post(StatusTarefaCreateDto dto)
    {
        var e = _mapper.Map<StatusTarefa>(dto);
        _db.StatusesTarefa.Add(e);
        await _db.SaveChangesAsync();
        var outDto = _mapper.Map<StatusTarefaDto>(e);
        return CreatedAtAction(nameof(Get), new { id = e.Id }, outDto);
    }
}
