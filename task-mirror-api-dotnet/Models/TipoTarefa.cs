using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMirror.Models;

[Table("tbl_tipo_tarefa")]
public class TipoTarefa
{
    [Key]
    [Column("id_tipo_tarefa")]
    public int IdTipoTarefa { get; set; }

    [Required]
    [Column("nome")]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    // relação 1:N com Tarefa (opcional carregar)
    public ICollection<Tarefa> Tarefas { get; set; } = new List<Tarefa>();
}
