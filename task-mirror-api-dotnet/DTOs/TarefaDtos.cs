using System;

namespace TaskMirror.DTOs;

public record TarefaDto(
    int Id,
    string Titulo,
    string? Descricao,
    int TempoEstimadoMin,
    int? TempoRealMin,
    DateTimeOffset? DataInicio,
    DateTimeOffset? DataFim,
    int UsuarioId,
    int? LiderId,
    int TipoTarefaId,
    int StatusTarefaId
);

public record TarefaCreateDto(
    string Titulo,
    string? Descricao,
    int TempoEstimadoMin,
    int UsuarioId,
    int? LiderId,
    int TipoTarefaId,
    int StatusTarefaId
);

public record TarefaUpdateDto(
    string? Titulo,
    string? Descricao,
    int? TempoEstimadoMin,
    int? UsuarioId,
    int? LiderId,
    int? TipoTarefaId,
    int? StatusTarefaId
);

public record FinalizarTarefaResponse(
    int TarefaId, int? TempoRealMin, int Nota, string? Comentario
);
