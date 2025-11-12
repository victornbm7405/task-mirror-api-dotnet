using System;

namespace TaskMirror.Models;

public class Feedback
{
    public int Id { get; set; }

    public int TarefaId { get; set; }
    public Tarefa Tarefa { get; set; } = default!;

    public int Nota { get; set; } // 1-5
    public string? Comentario { get; set; }
    public bool GeradoAutomatico { get; set; } = true;
    public DateTimeOffset DataCriacao { get; set; } = DateTimeOffset.UtcNow;
}
