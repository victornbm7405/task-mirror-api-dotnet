using System;

namespace TaskMirror.DTOs;

public record FeedbackDto(
    int IdFeedback,
    int IdTarefa,
    string Conteudo,
    DateTime? DataGerado
);
