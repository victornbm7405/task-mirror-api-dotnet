using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMirror.Data;
using TaskMirror.Models;
using TaskMirror.Services;

namespace TaskMirror.Controllers
{
    [ApiController]
    [Route("api/v1/feedbacks")]
    [Authorize(Roles = "LIDER,USER")] // 🔐 mesmo esquema das tarefas
    public class FeedbacksController : ControllerBase
    {
        private readonly TaskMirrorDbContext _db;

        public FeedbacksController(TaskMirrorDbContext db)
        {
            _db = db;
        }

        // ================= Helpers =================

        private int GetUsuarioIdFromToken()
        {
            var claim = User.FindFirst("idUsuario");
            if (claim == null)
                throw new Exception("Claim 'idUsuario' não encontrada no token.");

            return int.Parse(claim.Value);
        }

        private static object ToThinResponse(Feedback f) => new
        {
            idFeedback = f.IdFeedback,
            conteudo = f.Conteudo,
            dataGerado = f.DataGerado,
            tarefa = f.Tarefa is null
                ? null
                : new
                {
                    idTarefa = f.Tarefa.IdTarefa,
                    descricao = f.Tarefa.Descricao,
                    usuario = f.Tarefa.Usuario is null
                        ? null
                        : new
                        {
                            idUsuario = f.Tarefa.Usuario.IdUsuario,
                            username = f.Tarefa.Usuario.Username
                        },
                    lider = f.Tarefa.Lider is null
                        ? null
                        : new
                        {
                            idUsuario = f.Tarefa.Lider.IdUsuario,
                            username = f.Tarefa.Lider.Username
                        },
                    tipo = f.Tarefa.TipoTarefa is null
                        ? null
                        : new
                        {
                            idTipoTarefa = f.Tarefa.TipoTarefa.IdTipoTarefa,
                            nome = f.Tarefa.TipoTarefa.Nome
                        }
                }
        };

        private static object ToThinResponseProjection(Feedback f) => new
        {
            idFeedback = f.IdFeedback,
            conteudo = f.Conteudo,
            dataGerado = f.DataGerado,
            tarefa = f.Tarefa == null
                ? null
                : new
                {
                    idTarefa = f.Tarefa.IdTarefa,
                    descricao = f.Tarefa.Descricao,
                    usuario = f.Tarefa.Usuario == null
                        ? null
                        : new
                        {
                            idUsuario = f.Tarefa.Usuario.IdUsuario,
                            username = f.Tarefa.Usuario.Username
                        },
                    lider = f.Tarefa.Lider == null
                        ? null
                        : new
                        {
                            idUsuario = f.Tarefa.Lider.IdUsuario,
                            username = f.Tarefa.Lider.Username
                        },
                    tipo = f.Tarefa.TipoTarefa == null
                        ? null
                        : new
                        {
                            idTipoTarefa = f.Tarefa.TipoTarefa.IdTipoTarefa,
                            nome = f.Tarefa.TipoTarefa.Nome
                        }
                }
        };

        // ============================== ENDPOINTS ==============================

        // GET: api/v1/feedbacks
        // LIDER -> vê todos os feedbacks
        // USER  -> vê apenas feedbacks das tarefas dele
        [HttpGet]
        public async Task<ActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var userId = GetUsuarioIdFromToken();
            var isLeader = User.IsInRole("LIDER");

            var query = _db.Feedbacks
                .AsNoTracking()
                .Include(f => f.Tarefa)!
                    .ThenInclude(t => t.Usuario)
                .Include(f => f.Tarefa)!
                    .ThenInclude(t => t.Lider)
                .Include(f => f.Tarefa)!
                    .ThenInclude(t => t.TipoTarefa)
                .AsQueryable();

            if (!isLeader)
            {
                // USER só enxerga feedbacks de tarefas em que ele é o responsável
                query = query.Where(f =>
                    f.Tarefa != null &&
                    f.Tarefa.IdUsuario == userId);
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderBy(f => f.IdFeedback)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => ToThinResponseProjection(f))
                .ToListAsync();

            var result = new
            {
                data = list,
                total,
                page,
                pageSize,
                _links = Hateoas.BuildListLinks("/api/v1/feedbacks", page, pageSize, total)
            };

            return Ok(result);
        }

        // GET api/v1/feedbacks/{idFeedback}
        // LIDER -> pode ver qualquer
        // USER  -> só se a tarefa for dele
        [HttpGet("{idFeedback:int}")]
        public async Task<ActionResult> GetById(int idFeedback)
        {
            var userId = GetUsuarioIdFromToken();
            var isLeader = User.IsInRole("LIDER");

            var feedback = await _db.Feedbacks
                .AsNoTracking()
                .Include(f => f.Tarefa)!
                    .ThenInclude(t => t.Usuario)
                .Include(f => f.Tarefa)!
                    .ThenInclude(t => t.Lider)
                .Include(f => f.Tarefa)!
                    .ThenInclude(t => t.TipoTarefa)
                .FirstOrDefaultAsync(f => f.IdFeedback == idFeedback);

            if (feedback is null)
                return NotFound();

            if (!isLeader &&
                (feedback.Tarefa == null || feedback.Tarefa.IdUsuario != userId))
            {
                // USER tentando acessar feedback de tarefa de outro usuário
                return Forbid();
            }

            var thin = ToThinResponse(feedback);

            var result = new
            {
                data = thin,
                _links = Hateoas.BuildResourceLinks("/api/v1/feedbacks", idFeedback)
            };

            return Ok(result);
        }

        // GET api/v1/feedbacks/por-tarefa/{idTarefa}
        // LIDER -> pode ver qualquer
        // USER  -> só se a tarefa for dele
        [HttpGet("por-tarefa/{idTarefa:int}")]
        public async Task<ActionResult> GetByTarefa(int idTarefa)
        {
            var userId = GetUsuarioIdFromToken();
            var isLeader = User.IsInRole("LIDER");

            var feedback = await _db.Feedbacks
                .AsNoTracking()
                .Include(f => f.Tarefa)!
                    .ThenInclude(t => t.Usuario)
                .Include(f => f.Tarefa)!
                    .ThenInclude(t => t.Lider)
                .Include(f => f.Tarefa)!
                    .ThenInclude(t => t.TipoTarefa)
                .FirstOrDefaultAsync(f => f.IdTarefa == idTarefa);

            if (feedback is null)
                return NotFound();

            if (!isLeader &&
                (feedback.Tarefa == null || feedback.Tarefa.IdUsuario != userId))
            {
                return Forbid();
            }

            var thin = ToThinResponse(feedback);

            var result = new
            {
                data = thin,
                _links = Hateoas.BuildResourceLinks("/api/v1/feedbacks", feedback.IdFeedback)
            };

            return Ok(result);
        }
    }
}
