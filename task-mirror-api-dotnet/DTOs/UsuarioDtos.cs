namespace TaskMirror.DTOs;

public record UsuarioDto(int Id, string Username, string Role, string Funcao, int? IdLider);
public record UsuarioCreateDto(string Username, string Password, string Role, string Funcao, int? IdLider);
public record UsuarioUpdateDto(string? Password, string? Role, string? Funcao, int? IdLider);
