using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskMirror.Data;
using TaskMirror.Models;

namespace TaskMirror.Services;

public static class StatusNames
{
    public const string Pendente = "Pendente";
    public const string EmAndamento = "Em Andamento";
    public const string Finalizado = "Finalizado";
}

public class TarefaService
{
    private readonly TaskMirrorDbContext _db;

    public TarefaService(TaskMirrorDbContext db) => _db = db;

    // Busca case-insensitive
    private async Task<int> GetStatusIdAsync(string nome)
    {
        var nomeNorm = nome.Trim().ToUpperInvariant();
        var s = await _db.StatusesTarefa
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Nome.ToUpper() == nomeNorm);

        if (s is null)
            throw new InvalidOperationException($"Status '{nome}' não existe na tabela tbl_status_tarefa.");
        return s.IdStatusTarefa;
    }

    public async Task<Tarefa> CriarPorLiderAsync(Tarefa input)
    {
        var lider = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == input.IdLider);
        if (lider is null) throw new InvalidOperationException("Líder informado não existe.");

        var usuario = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == input.IdUsuario);
        if (usuario is null) throw new InvalidOperationException("Usuário (funcionário) informado não existe.");

        if (usuario.IdLider != input.IdLider)
            throw new InvalidOperationException("O usuário informado não é subordinado do líder informado.");

        input.IdStatusTarefa = await GetStatusIdAsync(StatusNames.Pendente);
        input.DataInicio = null;
        input.DataFim = null;
        input.TempoReal = null;

        _db.Tarefas.Add(input);
        await _db.SaveChangesAsync();
        return input;
    }

    public async Task<Tarefa?> IniciarAsync(int idTarefa, int idUsuario)
    {
        var tarefa = await _db.Tarefas.FirstOrDefaultAsync(t => t.IdTarefa == idTarefa);
        if (tarefa is null) return null;

        if (tarefa.IdUsuario != idUsuario)
            throw new InvalidOperationException("Esta tarefa não pertence a este usuário.");

        if (tarefa.DataInicio is null)
        {
            tarefa.DataInicio = DateTime.UtcNow;
            tarefa.IdStatusTarefa = await GetStatusIdAsync(StatusNames.EmAndamento);
            await _db.SaveChangesAsync();
        }

        return tarefa;
    }

    public async Task<Tarefa?> FinalizarAsync(int idTarefa, int idUsuario)
    {
        var tarefa = await _db.Tarefas.FirstOrDefaultAsync(t => t.IdTarefa == idTarefa);
        if (tarefa is null) return null;

        if (tarefa.IdUsuario != idUsuario)
            throw new InvalidOperationException("Esta tarefa não pertence a este usuário.");

        if (tarefa.DataInicio is null)
            throw new InvalidOperationException("A tarefa ainda não foi iniciada.");

        tarefa.DataFim = DateTime.UtcNow;

        // ✅ Agora TempoReal volta a ser salvo em MINUTOS (com 2 casas)
        var diffMin = (tarefa.DataFim.Value - tarefa.DataInicio.Value).TotalMinutes;
        tarefa.TempoReal = Math.Round(Convert.ToDecimal(diffMin, CultureInfo.InvariantCulture), 2);

        tarefa.IdStatusTarefa = await GetStatusIdAsync(StatusNames.Finalizado);

        await _db.SaveChangesAsync();
        return tarefa;
    }
}
