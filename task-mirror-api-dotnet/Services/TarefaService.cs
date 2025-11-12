using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskMirror.Data;
using TaskMirror.Models;

namespace TaskMirror.Services
{
    public class TarefaService
    {
        private readonly TaskMirrorDbContext _db;

        public TarefaService(TaskMirrorDbContext db) => _db = db;

        // Busca Id do status pelo nome (Pendente / Em Andamento / Finalizado)
        private async Task<int> GetStatusIdAsync(string nome)
        {
            var s = await _db.StatusesTarefa.AsNoTracking().FirstOrDefaultAsync(x => x.Nome == nome);
            if (s is null) throw new InvalidOperationException($"Status '{nome}' não existe na tabela tbl_status_tarefa.");
            return s.IdStatusTarefa;
        }

        /// <summary>
        /// 1) Líder cria a tarefa para um subordinado.
        /// Regras:
        /// - IdLider deve existir
        /// - IdUsuario deve existir e ter IdLider == IdLider informado
        /// - Status = "Pendente"
        /// - DataInicio/TempoReal/DataFim nulos
        /// </summary>
        public async Task<Tarefa> CriarPorLiderAsync(Tarefa input)
        {
            var lider = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == input.IdLider);
            if (lider is null) throw new InvalidOperationException("Líder informado não existe.");

            var usuario = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == input.IdUsuario);
            if (usuario is null) throw new InvalidOperationException("Usuário (funcionário) informado não existe.");

            if (usuario.IdLider != input.IdLider)
                throw new InvalidOperationException("O usuário informado não é subordinado do líder informado.");

            input.IdStatusTarefa = await GetStatusIdAsync("Pendente");
            input.DataInicio = null;
            input.DataFim = null;
            input.TempoReal = null;

            _db.Tarefas.Add(input);
            await _db.SaveChangesAsync();
            return input;
        }

        /// <summary>
        /// 2) Funcionário inicia a tarefa:
        /// - Só pode iniciar se ainda não tiver DataInicio
        /// - Força status "Em Andamento"
        /// - Valida se o idUsuario é o dono da tarefa
        /// </summary>
        public async Task<Tarefa?> IniciarAsync(int idTarefa, int idUsuario)
        {
            var tarefa = await _db.Tarefas.FirstOrDefaultAsync(t => t.IdTarefa == idTarefa);
            if (tarefa is null) return null;

            if (tarefa.IdUsuario != idUsuario)
                throw new InvalidOperationException("Esta tarefa não pertence a este usuário.");

            if (tarefa.DataInicio is null)
            {
                tarefa.DataInicio = DateTime.UtcNow;
                tarefa.IdStatusTarefa = await GetStatusIdAsync("Em Andamento");
                await _db.SaveChangesAsync();
            }

            return tarefa;
        }

        /// <summary>
        /// 3) Funcionário finaliza a tarefa:
        /// - Precisa ter sido iniciada
        /// - Calcula TempoReal em minutos
        /// - Seta DataFim = agora e status "Finalizado"
        /// - (Feedback removido por enquanto)
        /// - Valida se o idUsuario é o dono da tarefa
        /// </summary>
        public async Task<Tarefa?> FinalizarAsync(int idTarefa, int idUsuario)
        {
            var tarefa = await _db.Tarefas.FirstOrDefaultAsync(t => t.IdTarefa == idTarefa);
            if (tarefa is null) return null;

            if (tarefa.IdUsuario != idUsuario)
                throw new InvalidOperationException("Esta tarefa não pertence a este usuário.");

            if (tarefa.DataInicio is null)
                throw new InvalidOperationException("A tarefa ainda não foi iniciada.");

            tarefa.DataFim = DateTime.UtcNow;

            var diffMin = (tarefa.DataFim.Value - tarefa.DataInicio.Value).TotalMinutes;
            var tempoRealDecimal = Math.Round(Convert.ToDecimal(diffMin, CultureInfo.InvariantCulture), 2);
            tarefa.TempoReal = tempoRealDecimal;

            tarefa.IdStatusTarefa = await GetStatusIdAsync("Finalizado");

            await _db.SaveChangesAsync();
            return tarefa;
        }
    }
}
