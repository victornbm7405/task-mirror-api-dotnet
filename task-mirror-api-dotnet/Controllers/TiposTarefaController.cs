using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using TaskMirror.Data;
using TaskMirror.DTOs;

namespace TaskMirror.Controllers;

[ApiController]
[Route("api/tipos-tarefa")]
public class TiposTarefaController : ControllerBase
{
    private readonly TaskMirrorDbContext _db;
    private readonly IMapper _mapper;

    public TiposTarefaController(TaskMirrorDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    // GET: api/tipos-tarefa
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TipoTarefaDto>>> GetAll()
    {
        var list = await _db.TiposTarefa
            .AsNoTracking()
            .ToListAsync();

        var dto = _mapper.Map<IEnumerable<TipoTarefaDto>>(list);
        return Ok(dto);
    }
}
