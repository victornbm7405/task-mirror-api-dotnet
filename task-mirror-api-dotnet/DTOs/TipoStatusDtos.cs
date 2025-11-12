namespace TaskMirror.DTOs;

public record TipoTarefaDto(int Id, string Nome);
public record TipoTarefaCreateDto(string Nome);

public record StatusTarefaDto(int Id, string Nome);
public record StatusTarefaCreateDto(string Nome);
