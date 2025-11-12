using System.Collections.Generic;

namespace TaskMirror.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string Role { get; set; } = "Colaborador"; // "Lider" ou "Colaborador"
    public string Funcao { get; set; } = default!;

    public int? IdLider { get; set; }
    public Usuario? Lider { get; set; }
    public ICollection<Usuario> Subordinados { get; set; } = new List<Usuario>();

    public ICollection<Tarefa> Tarefas { get; set; } = new List<Tarefa>();
}
