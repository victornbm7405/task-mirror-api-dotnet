namespace TaskMirror.Models;

public class StatusTarefa
{
    public int Id { get; set; }
    public string Nome { get; set; } = default!; // Aberta, Em Progresso, Concluída, Atrasada...
}
