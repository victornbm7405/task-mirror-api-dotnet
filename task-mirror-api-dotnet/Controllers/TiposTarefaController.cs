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
[Route("api/v1/tipos-tarefa")]
public class TiposTarefaController : ControllerBase
{
    private readonly TaskMirrorDbContext _db;
    private readonly IMapper _mapper;

    public TiposTarefaController(TaskMirrorDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    [HttpGet]
    public async Task<IEnumerable<TipoTarefaDto>> Get()
        => _mapper.Map<IEnumerable<TipoTarefaDto>>(await _db.TiposTarefa.AsNoTracking().ToListAsync());

    [HttpPost]
    public async Task<ActionResult<TipoTarefaDto>> Post(TipoTarefaCreateDto dto)
    {
        var e = _mapper.Map<TipoTarefa>(dto);
        _db.TiposTarefa.Add(e);
        await _db.SaveChangesAsync();
        var outDto = _mapper.Map<TipoTarefaDto>(e);
        return CreatedAtAction(nameof(Get), new { id = e.Id }, outDto);
    }
}
