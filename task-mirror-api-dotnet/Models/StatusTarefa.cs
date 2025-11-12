using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMirror.Models;

[Table("tbl_status_tarefa")]
public class StatusTarefa
{
    [Key]
    [Column("id_status_tarefa")]
    public int IdStatusTarefa { get; set; }

    [Required]
    [Column("nome")]
    [MaxLength(100)]
    public string Nome { get; set; } = null!;
}
