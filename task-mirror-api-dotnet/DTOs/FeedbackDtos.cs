using System;

namespace TaskMirror.DTOs;

public record FeedbackDto(int Id, int TarefaId, int Nota, string? Comentario, bool GeradoAutomatico, DateTimeOffset DataCriacao);
