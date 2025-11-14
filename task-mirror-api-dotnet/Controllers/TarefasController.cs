using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "LIDER,USER")] // 🔐 Todo mundo aqui precisa estar autenticado
    public class TarefasController : ControllerBase
    {
        private readonly TaskMirrorDbContext _db;
        private readonly TarefaService _service;

        public TarefasController(TaskMirrorDbContext db, TarefaService service)
        {
            _db = db;
            _service = service;
        }

        // ================= Helpers =================

        private int GetUsuarioIdFromToken()
        {
            var claim = User.FindFirst("idUsuario");
            if (claim == null)
                throw new System.Exception("Claim 'idUsuario' não encontrada no token.");

            return int.Parse(claim.Value);
        }

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
        // LIDER -> vê tudo
        // USER  -> vê apenas as próprias tarefas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var userId = GetUsuarioIdFromToken();
            var isLeader = User.IsInRole("LIDER");

            var query = _db.Tarefas
                .AsNoTracking()
                .Include(t => t.Usuario)
                .Include(t => t.Lider)
                .Include(t => t.TipoTarefa)
                .Include(t => t.StatusTarefa)
                .AsQueryable();

            if (!isLeader)
            {
                // USER só enxerga as tarefas em que ele é o responsável
                query = query.Where(t => t.IdUsuario == userId);
            }

            var list = await query
                .Select(t => ToThinResponseProjection(t))
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/tarefas/5  (detalhe enxuto)
        // LIDER -> pode ver qualquer
        // USER  -> só se for tarefa dele
        [HttpGet("{idTarefa:int}")]
        public async Task<ActionResult<object>> GetById(int idTarefa)
        {
            var userId = GetUsuarioIdFromToken();
            var isLeader = User.IsInRole("LIDER");

            var tarefa = await _db.Tarefas
                .AsNoTracking()
                .Include(t => t.Usuario)
                .Include(t => t.Lider)
                .Include(t => t.TipoTarefa)
                .Include(t => t.StatusTarefa)
                .FirstOrDefaultAsync(t => t.IdTarefa == idTarefa);

            if (tarefa is null)
                return NotFound();

            if (!isLeader && tarefa.IdUsuario != userId)
                return Forbid(); // USER tentando acessar tarefa de outro

            return Ok(ToThinResponse(tarefa));
        }

        // POST: api/tarefas  (líder cria para subordinado)
        // 🔒 SOMENTE LIDER pode criar tarefa
        [HttpPost]
        [Authorize(Roles = "LIDER")]
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

        // POST: api/tarefas/{idTarefa}/iniciar
        // 🔓 USER pode iniciar tarefa, mas APENAS a própria
        // 🔒 LIDER NÃO PODE iniciar tarefas
        [HttpPost("{idTarefa:int}/iniciar")]
        [Authorize(Roles = "USER")]
        public async Task<ActionResult<object>> Iniciar(int idTarefa)
        {
            var userId = GetUsuarioIdFromToken();

            try
            {
                var tarefa = await _service.IniciarAsync(idTarefa, userId);
                if (tarefa is null) return NotFound();

                tarefa = await _db.Tarefas
                    .Include(t => t.Usuario)
                    .Include(t => t.Lider)
                    .Include(t => t.TipoTarefa)
                    .Include(t => t.StatusTarefa)
                    .FirstAsync(t => t.IdTarefa == tarefa.IdTarefa);

                // Garantia extra: mesmo no Service tendo checado, aqui garantimos que USER não veja tarefa de outro
                if (tarefa.IdUsuario != userId)
                    return Forbid();

                return Ok(ToThinResponse(tarefa));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST: api/tarefas/{idTarefa}/finalizar
        // 🔓 USER pode finalizar tarefa, mas APENAS a própria
        // 🔒 LIDER NÃO PODE finalizar tarefas
        [HttpPost("{idTarefa:int}/finalizar")]
        [Authorize(Roles = "USER")]
        public async Task<ActionResult<object>> Finalizar(int idTarefa)
        {
            var userId = GetUsuarioIdFromToken();

            try
            {
                var tarefa = await _service.FinalizarAsync(idTarefa, userId);
                if (tarefa is null) return NotFound();

                tarefa = await _db.Tarefas
                    .Include(t => t.Usuario)
                    .Include(t => t.Lider)
                    .Include(t => t.TipoTarefa)
                    .Include(t => t.StatusTarefa)
                    .FirstAsync(t => t.IdTarefa == tarefa.IdTarefa);

                if (tarefa.IdUsuario != userId)
                    return Forbid();

                return Ok(ToThinResponse(tarefa));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT: api/tarefas/5  (manutenção geral — líder/admin)
        // 🔒 SOMENTE LIDER
        [HttpPut("{idTarefa:int}")]
        [Authorize(Roles = "LIDER")]
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
        // 🔒 SOMENTE LIDER
        [HttpDelete("{idTarefa:int}")]
        [Authorize(Roles = "LIDER")]
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
