using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMirror.Data;
using TaskMirror.DTOs;
using TaskMirror.Services;

namespace TaskMirror.Controllers;

[ApiController]
[Route("api/v1/feedbacks")]
public class FeedbacksController : ControllerBase
{
    private readonly TaskMirrorDbContext _db;
    private readonly IMapper _mapper;

    public FeedbacksController(TaskMirrorDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    // GET api/v1/feedbacks/123  -> busca por id_feedback
    [HttpGet("{idFeedback:int}")]
    public async Task<ActionResult> GetById(int idFeedback)
    {
        var entity = await _db.Feedbacks
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.IdFeedback == idFeedback);

        if (entity is null) return NotFound();

        var dto = _mapper.Map<FeedbackDto>(entity);

        var result = new
        {
            data = dto,
            _links = Hateoas.BuildResourceLinks("/api/v1/feedbacks", idFeedback)
        };

        return Ok(result);
    }

    // GET api/v1/feedbacks/por-tarefa/45 -> busca por id_tarefa (1:1)
    [HttpGet("por-tarefa/{idTarefa:int}")]
    public async Task<ActionResult> GetByTarefa(int idTarefa)
    {
        var entity = await _db.Feedbacks
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.IdTarefa == idTarefa);

        if (entity is null) return NotFound();

        var dto = _mapper.Map<FeedbackDto>(entity);

        var result = new
        {
            data = dto,
            _links = Hateoas.BuildResourceLinks("/api/v1/feedbacks", entity.IdFeedback)
        };

        return Ok(result);
    }
}
