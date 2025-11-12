using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMirror.Data;
using TaskMirror.DTOs;

namespace TaskMirror.Controllers;

[ApiController]
[Route("api/v1/feedbacks")]
public class FeedbacksController : ControllerBase
{
    private readonly TaskMirrorDbContext _db;
    private readonly IMapper _mapper;

    public FeedbacksController(TaskMirrorDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FeedbackDto>> GetById(int id)
    {
        var e = await _db.Feedbacks.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
        return e is null ? NotFound() : Ok(_mapper.Map<FeedbackDto>(e));
    }

    [HttpGet("por-tarefa/{tarefaId:int}")]
    public async Task<ActionResult<FeedbackDto>> GetByTarefa(int tarefaId)
    {
        var e = await _db.Feedbacks.AsNoTracking().FirstOrDefaultAsync(f => f.TarefaId == tarefaId);
        return e is null ? NotFound() : Ok(_mapper.Map<FeedbackDto>(e));
    }
}
