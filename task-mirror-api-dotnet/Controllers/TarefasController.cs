using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMirror.Data;
using TaskMirror.Models;
using TaskMirror.Services;
using TaskMirror.DTOs;

namespace TaskMirror.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // => api/tarefas
    public class TarefasController : ControllerBase
    {
        private readonly TaskMirrorDbContext _db;
        private readonly TarefaService _service;

        public TarefasController(TaskMirrorDbContext db, TarefaService service)
        {
            _db = db;
            _service = service;
        }

        // =============== Helpers de projeção (respostas enxutas) ===============

        private static object ToThinResponse(Tarefa t) => new
        {
            idTarefa = t.IdTarefa,
            descricao = t.Descricao,
            tempoEstimado = t.TempoEstimado,
            tempoReal = t.TempoReal,
            dataInicio = t.DataInicio,
            dataFim = t.DataFim,
            usuario = t.Usuario is null ? null : new { idUsuario = t.Usuario.IdUsuario, username = t.Usuario.Username },
            lider = t.Lider is null ? null : new { idUsuario = t.Lider.IdUsuario, username = t.Lider.Username },
            tipo = t.TipoTarefa is null ? null : new { idTipoTarefa = t.TipoTarefa.IdTipoTarefa, nome = t.TipoTarefa.Nome },
            status = t.StatusTarefa is null ? null : new { idStatusTarefa = t.StatusTarefa.IdStatusTarefa, nome = t.StatusTarefa.Nome }
        };

        private static object ToThinResponseProjection(Tarefa t) => new
        {
            idTarefa = t.IdTarefa,
            descricao = t.Descricao,
            tempoEstimado = t.TempoEstimado,
            tempoReal = t.TempoReal,
            dataInicio = t.DataInicio,
            dataFim = t.DataFim,
            usuario = new { idUsuario = t.Usuario!.IdUsuario, username = t.Usuario!.Username },
            lider = new { idUsuario = t.Lider!.IdUsuario, username = t.Lider!.Username },
            tipo = new { idTipoTarefa = t.TipoTarefa!.IdTipoTarefa, nome = t.TipoTarefa!.Nome },
            status = new { idStatusTarefa = t.StatusTarefa!.IdStatusTarefa, nome = t.StatusTarefa!.Nome }
        };

        // ============================== ENDPOINTS ==============================

        // GET: api/tarefas  (lista enxuta)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var list = await _db.Tarefas
                .AsNoTracking()
                .Include(t => t.Usuario)
                .Include(t => t.Lider)
                .Include(t => t.TipoTarefa)
                .Include(t => t.StatusTarefa)
                .Select(t => ToThinResponseProjection(t))
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/tarefas/5  (detalhe enxuto)
        [HttpGet("{idTarefa:int}")]
        public async Task<ActionResult<object>> GetById(int idTarefa)
        {
            var tarefa = await _db.Tarefas
                .AsNoTracking()
                .Include(t => t.Usuario)
                .Include(t => t.Lider)
                .Include(t => t.TipoTarefa)
                .Include(t => t.StatusTarefa)
                .FirstOrDefaultAsync(t => t.IdTarefa == idTarefa);

            if (tarefa is null) return NotFound();
            return Ok(ToThinResponse(tarefa));
        }

        // POST: api/tarefas  (líder cria para subordinado)
        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] TarefaCreateRequest body)
        {
            try
            {
                // mapeamento manual -> Entity
                var entity = new Tarefa
                {
                    IdUsuario = body.IdUsuario,
                    IdLider = body.IdLider,
                    IdTipoTarefa = body.IdTipoTarefa,
                    Descricao = body.Descricao,
                    TempoEstimado = body.TempoEstimado
                    // Status/DataInicio/TempoReal/DataFim são definidos no service
                };

                var tarefa = await _service.CriarPorLiderAsync(entity);

                // Recarrega com includes para resposta enxuta
                tarefa = await _db.Tarefas
                    .Include(t => t.Usuario)
                    .Include(t => t.Lider)
                    .Include(t => t.TipoTarefa)
                    .Include(t => t.StatusTarefa)
                    .FirstAsync(t => t.IdTarefa == tarefa.IdTarefa);

                return CreatedAtAction(nameof(GetById), new { idTarefa = tarefa.IdTarefa }, ToThinResponse(tarefa));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST: api/tarefas/{idTarefa}/iniciar?usuario=123
        [HttpPost("{idTarefa:int}/iniciar")]
        public async Task<ActionResult<object>> Iniciar(int idTarefa, [FromQuery] int usuario)
        {
            try
            {
                var tarefa = await _service.IniciarAsync(idTarefa, usuario);
                if (tarefa is null) return NotFound();

                tarefa = await _db.Tarefas
                    .Include(t => t.Usuario)
                    .Include(t => t.Lider)
                    .Include(t => t.TipoTarefa)
                    .Include(t => t.StatusTarefa)
                    .FirstAsync(t => t.IdTarefa == tarefa.IdTarefa);

                return Ok(ToThinResponse(tarefa));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST: api/tarefas/{idTarefa}/finalizar?usuario=123
        [HttpPost("{idTarefa:int}/finalizar")]
        public async Task<ActionResult<object>> Finalizar(int idTarefa, [FromQuery] int usuario)
        {
            try
            {
                var tarefa = await _service.FinalizarAsync(idTarefa, usuario);
                if (tarefa is null) return NotFound();

                tarefa = await _db.Tarefas
                    .Include(t => t.Usuario)
                    .Include(t => t.Lider)
                    .Include(t => t.TipoTarefa)
                    .Include(t => t.StatusTarefa)
                    .FirstAsync(t => t.IdTarefa == tarefa.IdTarefa);

                return Ok(ToThinResponse(tarefa));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT: api/tarefas/5  (manutenção geral — líder/admin)
        [HttpPut("{idTarefa:int}")]
        public async Task<IActionResult> Update(int idTarefa, [FromBody] TarefaUpdateRequest input)
        {
            var tarefa = await _db.Tarefas.FirstOrDefaultAsync(t => t.IdTarefa == idTarefa);
            if (tarefa is null) return NotFound();

            // aplica somente se veio no request
            if (input.IdUsuario.HasValue) tarefa.IdUsuario = input.IdUsuario.Value;
            if (input.IdLider.HasValue) tarefa.IdLider = input.IdLider.Value;
            if (input.IdTipoTarefa.HasValue) tarefa.IdTipoTarefa = input.IdTipoTarefa.Value;
            if (input.IdStatusTarefa.HasValue) tarefa.IdStatusTarefa = input.IdStatusTarefa.Value;

            if (input.Descricao != null) tarefa.Descricao = input.Descricao;
            if (input.TempoEstimado.HasValue) tarefa.TempoEstimado = input.TempoEstimado.Value;
            if (input.TempoReal.HasValue) tarefa.TempoReal = input.TempoReal.Value;
            if (input.DataInicio.HasValue) tarefa.DataInicio = input.DataInicio.Value;
            if (input.DataFim.HasValue) tarefa.DataFim = input.DataFim.Value;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/tarefas/5
        [HttpDelete("{idTarefa:int}")]
        public async Task<IActionResult> Delete(int idTarefa)
        {
            var tarefa = await _db.Tarefas.FindAsync(idTarefa);
            if (tarefa is null) return NotFound();

            _db.Tarefas.Remove(tarefa);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
