using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMirror.Data;

namespace TaskMirror.Controllers
{
    [ApiController]
    [Route("api/status")]
    [Authorize(Roles = "LIDER,USER")] // 🔐 precisa estar logado (LIDER ou USER)
    public class StatusTarefaController : ControllerBase
    {
        private readonly TaskMirrorDbContext _db;

        public StatusTarefaController(TaskMirrorDbContext db)
        {
            _db = db;
        }

        // GET: api/status
        // Retorna SOMENTE os status que estão sendo usados em tarefas,
        // junto com a quantidade de tarefas em cada um (ajuda no dashboard).
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsados()
        {
            // Faz join implícito pela navegação e filtra apenas os que existem em Tarefas
            var usados = await _db.Tarefas
                .Include(t => t.StatusTarefa)
                .GroupBy(t => new { t.IdStatusTarefa, t.StatusTarefa!.Nome })
                .Select(g => new
                {
                    idStatusTarefa = g.Key.IdStatusTarefa,
                    nome = g.Key.Nome,
                    quantidade = g.Count()
                })
                .OrderBy(x => x.idStatusTarefa)
                .ToListAsync();

            return Ok(usados);
        }
    }
}
