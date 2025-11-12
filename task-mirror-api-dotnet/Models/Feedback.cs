using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMirror.Models;

[Table("tbl_feedbacks")]
public class Feedback
{
    [Key]
    [Column("id_feedback")]
    public int IdFeedback { get; set; }

    [Required]
    [Column("id_tarefa")]
    public int IdTarefa { get; set; }     // FK única p/ Tarefa

    [Required]
    [Column("conteudo")]
    [MaxLength(1000)]
    public string Conteudo { get; set; } = null!;

    [Required]
    [Column("data_gerado")]
    public DateTime DataGerado { get; set; }

    // Navegação 1:1
    public Tarefa Tarefa { get; set; } = null!;
}
