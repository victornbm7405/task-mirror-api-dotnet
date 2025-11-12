using System.Linq;
using TaskMirror.Models;

namespace TaskMirror.Data;

public static class DbInitializer
{
    public static void Seed(TaskMirrorDbContext db)
    {
        if (db.Usuarios.Any()) return;

        var status = new[]
        {
            new StatusTarefa{ Nome = "Aberta" },
            new StatusTarefa{ Nome = "Em Progresso" },
            new StatusTarefa{ Nome = "Concluída" },
            new StatusTarefa{ Nome = "Atrasada" },
        };
        db.StatusesTarefa.AddRange(status);

        var tipos = new[]
        {
            new TipoTarefa{ Nome = "Curta" },
            new TipoTarefa{ Nome = "Média" },
            new TipoTarefa{ Nome = "Longa" },
        };
        db.TiposTarefa.AddRange(tipos);

        var lider = new Usuario { Username = "lider", PasswordHash = "hash", Role = "Lider", Funcao = "Coordenação" };
        var col1 = new Usuario { Username = "alice", PasswordHash = "hash", Role = "Colaborador", Funcao = "Dev", Lider = lider };
        var col2 = new Usuario { Username = "gus", PasswordHash = "hash", Role = "Colaborador", Funcao = "Dev", Lider = lider };
        var col3 = new Usuario { Username = "victor", PasswordHash = "hash", Role = "Colaborador", Funcao = "Dev", Lider = lider };

        db.Usuarios.AddRange(lider, col1, col2, col3);
        db.SaveChanges();

        var t1 = new Tarefa
        {
            Titulo = "Implementar endpoint",
            Descricao = "GET /tarefas",
            TempoEstimadoMin = 120,
            UsuarioId = col3.Id,
            LiderId = lider.Id,
            TipoTarefaId = tipos[1].Id,
            StatusTarefaId = status[0].Id
        };
        var t2 = new Tarefa
        {
            Titulo = "Ajustar DTOs",
            TempoEstimadoMin = 60,
            UsuarioId = col1.Id,
            LiderId = lider.Id,
            TipoTarefaId = tipos[0].Id,
            StatusTarefaId = status[0].Id
        };

        db.Tarefas.AddRange(t1, t2);
        db.SaveChanges();
    }
}
