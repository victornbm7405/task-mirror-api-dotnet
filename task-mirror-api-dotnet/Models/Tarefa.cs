using System;

namespace TaskMirror.Models;

public class Tarefa
{
    public int Id { get; set; }
    public string Titulo { get; set; } = default!;
    public string? Descricao { get; set; }

    public int TempoEstimadoMin { get; set; }
    public int? TempoRealMin { get; set; }

    public DateTimeOffset? DataInicio { get; set; }
    public DateTimeOffset? DataFim { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = default!;

    public int? LiderId { get; set; }
    public Usuario? Lider { get; set; }

    public int TipoTarefaId { get; set; }
    public TipoTarefa TipoTarefa { get; set; } = default!;

    public int StatusTarefaId { get; set; }
    public StatusTarefa StatusTarefa { get; set; } = default!;

    public Feedback? Feedback { get; set; } // 1:1
}
