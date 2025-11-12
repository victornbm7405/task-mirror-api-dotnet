namespace TaskMirror.DTOs;

// DTO de saída (consulta)
public record UsuarioDto(
    int IdUsuario,
    string Username,
    string RoleUsuario,
    string Funcao,
    int? IdLider
);

// DTO de criação
public record UsuarioCreateDto(
    string Username,
    string Password,
    string RoleUsuario,
    string Funcao,
    int? IdLider
);

// DTO de atualização (todos opcionais)
public record UsuarioUpdateDto(
    string? Username,
    string? Password,
    string? RoleUsuario,
    string? Funcao,
    int? IdLider
);
