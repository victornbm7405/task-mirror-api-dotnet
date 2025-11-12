using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskMirror.Data;
using TaskMirror.Models;

namespace TaskMirror.Services;

public class TarefaService
{
    private readonly TaskMirrorDbContext _db;

    public TarefaService(TaskMirrorDbContext db) => _db = db;

    public async Task<(Tarefa tarefa, Feedback feedback)> FinalizarAsync(int tarefaId)
    {
        var tarefa = await _db.Tarefas
            .Include(t => t.StatusTarefa)
            .Include(t => t.Feedback)
            .FirstOrDefaultAsync(t => t.Id == tarefaId);

        if (tarefa is null) throw new InvalidOperationException("Tarefa não encontrada.");
        if (tarefa.Feedback is not null) throw new InvalidOperationException("Tarefa já finalizada (feedback existente).");

        var now = DateTimeOffset.UtcNow;
        if (tarefa.DataInicio is null) tarefa.DataInicio = now;
        tarefa.DataFim = now;
        tarefa.TempoRealMin = (int)Math.Max(1, (tarefa.DataFim.Value - tarefa.DataInicio.Value).TotalMinutes);

        // status -> Concluída
        tarefa.StatusTarefaId = await _db.StatusesTarefa
            .Where(s => s.Nome == "Concluída")
            .Select(s => s.Id)
            .FirstAsync();

        // nota automática simples
        var ratio = (double)tarefa.TempoEstimadoMin / tarefa.TempoRealMin.Value;
        var nota = ratio >= 1.2 ? 5 :
                   ratio >= 1.0 ? 4 :
                   ratio >= 0.8 ? 3 :
                   ratio >= 0.6 ? 2 : 1;

        var fb = new Feedback
        {
            TarefaId = tarefa.Id,
            Nota = nota,
            Comentario = $"Auto-feedback: estimado {tarefa.TempoEstimadoMin}min vs real {tarefa.TempoRealMin}min.",
            GeradoAutomatico = true
        };

        _db.Feedbacks.Add(fb);
        await _db.SaveChangesAsync();

        return (tarefa, fb);
    }
}
