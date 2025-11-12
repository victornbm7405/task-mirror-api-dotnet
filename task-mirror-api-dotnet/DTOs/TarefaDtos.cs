using System;

namespace TaskMirror.DTOs;

// =============== REQUESTS (entrada) ===============
public sealed record TarefaCreateRequest(
    string? Descricao,
    decimal TempoEstimado,
    int IdUsuario,     // funcionário dono da tarefa
    int IdLider,       // líder que atribui
    int IdTipoTarefa   // tipo
);

public sealed record TarefaUpdateRequest(
    string? Descricao,
    decimal? TempoEstimado,
    int? IdUsuario,
    int? IdLider,
    int? IdTipoTarefa,
    int? IdStatusTarefa,
    DateTime? DataInicio,
    DateTime? DataFim,
    decimal? TempoReal
);

// =============== RESPONSES (saída) ===============
public sealed record TarefaListItemDto(
    int IdTarefa,
    string? Descricao,
    string Status,
    string Tipo,
    int IdUsuario,
    string Usuario,
    int IdLider,
    string Lider,
    decimal? TempoEstimado,
    decimal? TempoReal,
    DateTime? DataInicio,
    DateTime? DataFim
);

public sealed record TarefaDetailsDto(
    int IdTarefa,
    string? Descricao,
    string Status,
    string Tipo,
    int IdUsuario,
    string Usuario,
    int IdLider,
    string Lider,
    decimal? TempoEstimado,
    decimal? TempoReal,
    DateTime? DataInicio,
    DateTime? DataFim
);
