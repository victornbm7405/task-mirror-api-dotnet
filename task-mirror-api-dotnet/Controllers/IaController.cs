using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMirror.Data;
using TaskMirror.Services;

namespace TaskMirror.Controllers
{
    [ApiController]
    [Route("api/v1/ia")]
    [Authorize(Roles = "LIDER")] // 👈 só líder pode usar esses relatórios
    public class IaController : ControllerBase
    {
        private readonly TaskMirrorDbContext _db;
        private readonly OllamaIaService _iaService;

        public IaController(TaskMirrorDbContext db, OllamaIaService iaService)
        {
            _db = db;
            _iaService = iaService;
        }

        /// <summary>
        /// Gera uma visão geral do colaborador para o líder com base nos feedbacks existentes.
        /// </summary>
        /// <param name="idUsuario">Id do colaborador</param>
        [HttpPost("analise-usuario/{idUsuario:int}")]
        public async Task<ActionResult> AnaliseUsuario(int idUsuario)
        {
            // 1) Garante que o usuário existe
            var usuario = await _db.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario is null)
                return NotFound(new { error = "Usuário não encontrado." });

            // 2) Busca feedbacks das tarefas desse usuário
            var feedbacks = await _db.Feedbacks
                .AsNoTracking()
                .Include(f => f.Tarefa)
                .Where(f => f.Tarefa != null && f.Tarefa.IdUsuario == idUsuario)
                .OrderByDescending(f => f.DataGerado)
                .ToListAsync();

            if (!feedbacks.Any())
            {
                return NotFound(new
                {
                    error = "Nenhum feedback encontrado para este usuário.",
                    usuario = new { usuario.IdUsuario, usuario.Username }
                });
            }

            // 3) Monta um texto concatenando tarefas + feedbacks
            var sb = new StringBuilder();
            foreach (var f in feedbacks)
            {
                var desc = f.Tarefa?.Descricao ?? "(tarefa sem descrição)";
                sb.AppendLine($"Tarefa: {desc}");
                sb.AppendLine($"Feedback: {f.Conteudo}");
                sb.AppendLine();
            }

            var textoFeedbacks = sb.ToString();

            // 4) Chama a IA para gerar o resumo para o líder
            var resumo = await _iaService.GerarResumoColaboradorAsync(
                usuario.Username,
                textoFeedbacks
            );

            // 5) Retorna um objeto bonitinho para o líder
            var result = new
            {
                usuario = new
                {
                    idUsuario = usuario.IdUsuario,
                    username = usuario.Username
                },
                quantidadeFeedbacks = feedbacks.Count,
                resumoGerado = resumo
            };

            return Ok(result);
        }

        /// <summary>
        /// Prevê a chance de atraso de uma tarefa específica com base no histórico do colaborador.
        /// Só é permitido para tarefas com status 'Pendente'.
        /// </summary>
        /// <param name="idTarefa">Id da tarefa a ser prevista</param>
        [HttpPost("prever-atraso/{idTarefa:int}")]
        public async Task<ActionResult> PreverAtraso(int idTarefa)
        {
            // 1) Busca a tarefa alvo
            var tarefa = await _db.Tarefas
                .Include(t => t.Usuario)
                .Include(t => t.TipoTarefa)
                .Include(t => t.StatusTarefa)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.IdTarefa == idTarefa);

            if (tarefa is null)
                return NotFound(new { error = "Tarefa não encontrada." });

            if (tarefa.Usuario is null)
                return BadRequest(new { error = "Tarefa não possui usuário associado." });

            // ✅ Regra que você pediu:
            // Só prever atraso se a tarefa estiver PENDENTE
            if (tarefa.StatusTarefa == null || tarefa.StatusTarefa.Nome != StatusNames.Pendente)
            {
                return BadRequest(new
                {
                    error = "Só é possível prever atraso para tarefas com status 'Pendente'.",
                    statusAtual = tarefa.StatusTarefa?.Nome
                });
            }

            var usuario = tarefa.Usuario;

            // 2) Busca histórico de tarefas finalizadas desse usuário (sem a tarefa atual)
            var historico = await _db.Tarefas
                .AsNoTracking()
                .Include(t => t.TipoTarefa)
                .Where(t =>
                    t.IdUsuario == usuario.IdUsuario &&
                    t.IdTarefa != tarefa.IdTarefa &&
                    t.DataFim != null &&
                    t.TempoEstimado != null &&
                    t.TempoReal != null)
                .OrderByDescending(t => t.DataFim)
                .Take(20)
                .ToListAsync();

            // 3) Monta texto do histórico em formato simples
            var sb = new StringBuilder();
            if (historico.Any())
            {
                foreach (var h in historico)
                {
                    var tipo = h.TipoTarefa != null
                        ? h.TipoTarefa.Nome
                        : "Não informado";

                    sb.AppendLine($"Tarefa: {h.Descricao ?? "(sem descrição)"}");
                    sb.AppendLine($"Tipo: {tipo}");
                    sb.AppendLine($"Tempo estimado: {h.TempoEstimado} min | Tempo real: {h.TempoReal} min");
                    sb.AppendLine();
                }
            }

            var historicoTexto = historico.Any()
                ? sb.ToString()
                : "Nenhum histórico anterior disponível. Considere apenas os dados da tarefa atual.";

            var descricaoAtual = tarefa.Descricao ?? "Tarefa sem descrição.";
            var tempoEstimadoAtual = tarefa.TempoEstimado ?? 0m;
            var tipoAtual = tarefa.TipoTarefa?.Nome;

            // 4) Chama a IA para gerar a previsão de atraso
            var previsaoTexto = await _iaService.GerarPrevisaoAtrasoAsync(
                usuario.Username,
                descricaoAtual,
                tempoEstimadoAtual,
                tipoAtual,
                historicoTexto
            );

            // 5) Monta resposta
            var result = new
            {
                tarefa = new
                {
                    idTarefa = tarefa.IdTarefa,
                    descricao = tarefa.Descricao,
                    tempoEstimado = tarefa.TempoEstimado,
                    tempoReal = tarefa.TempoReal,
                    tipo = tarefa.TipoTarefa is null
                        ? null
                        : new { idTipoTarefa = tarefa.TipoTarefa.IdTipoTarefa, nome = tarefa.TipoTarefa.Nome },
                    usuario = new { idUsuario = usuario.IdUsuario, username = usuario.Username },
                    status = tarefa.StatusTarefa is null
                        ? null
                        : new { idStatusTarefa = tarefa.StatusTarefa.IdStatusTarefa, nome = tarefa.StatusTarefa.Nome }
                },
                possuiHistorico = historico.Any(),
                quantidadeTarefasHistorico = historico.Count,
                previsao = previsaoTexto
            };

            return Ok(result);
        }
    }
}
