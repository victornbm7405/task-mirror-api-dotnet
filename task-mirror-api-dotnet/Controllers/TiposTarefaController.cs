using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using TaskMirror.Data;
using TaskMirror.DTOs;
using TaskMirror.Services;

namespace TaskMirror.Controllers;

[ApiController]
[Route("api/v1/tipos-tarefa")]
[Authorize(Roles = "LIDER,USER")] // 🔐 Apenas usuários autenticados podem acessar
public class TiposTarefaController : ControllerBase
{
    private readonly TaskMirrorDbContext _db;
    private readonly IMapper _mapper;

    public TiposTarefaController(TaskMirrorDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    /// <summary>
    /// Lista todos os tipos de tarefa com paginação.
    /// </summary>
    /// <param name="page">Número da página (padrão = 1).</param>
    /// <param name="pageSize">Tamanho da página (padrão = 10, máx = 50).</param>
    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        var query = _db.TiposTarefa
            .AsNoTracking()
            .OrderBy(t => t.IdTipoTarefa)
            .AsQueryable();

        var total = await query.CountAsync();

        var list = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dto = _mapper.Map<IEnumerable<TipoTarefaDto>>(list);

        var result = new
        {
            data = dto,
            total,
            page,
            pageSize,
            _links = Hateoas.BuildListLinks("/api/v1/tipos-tarefa", page, pageSize, total)
        };

        return Ok(result);
    }

    /// <summary>
    /// Retorna a quantidade de tarefas por tipo de tarefa.
    /// Exemplo de resposta:
    /// [
    ///   { "idTipoTarefa": 1, "nome": "Reunião", "quantidade": 3 }
    /// ]
    /// </summary>
    [HttpGet("resumo")]
    public async Task<ActionResult<IEnumerable<TipoTarefaResumoDto>>> GetResumoPorTipo()
    {
        var resumo = await _db.TiposTarefa
            .GroupJoin(
                _db.Tarefas,
                tipo => tipo.IdTipoTarefa,
                tarefa => tarefa.IdTipoTarefa,
                (tipo, tarefas) => new TipoTarefaResumoDto(
                    tipo.IdTipoTarefa,
                    tipo.Nome,
                    tarefas.Count()
                )
            )
            .AsNoTracking()
            .ToListAsync();

        return Ok(resumo);
    }
}
