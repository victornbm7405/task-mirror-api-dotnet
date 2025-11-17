namespace TaskMirror.DTOs;

// ===== tbl_tipo_tarefa =====
public record TipoTarefaDto(int IdTipoTarefa, string Nome);
public record TipoTarefaCreateDto(string Nome);
public record TipoTarefaUpdateDto(int IdTipoTarefa, string Nome);


// 🔹 NOVO DTO DE RESUMO
public record TipoTarefaResumoDto(int IdTipoTarefa, string Nome, int Quantidade);

// ===== tbl_status_tarefa =====
public record StatusTarefaDto(int IdStatusTarefa, string Nome);
public record StatusTarefaCreateDto(string Nome);
public record StatusTarefaUpdateDto(int IdStatusTarefa, string Nome);
