using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMirror.Models;

[Table("tbl_tarefas")]
public class Tarefa
{
    [Key]
    [Column("id_tarefa")]
    public int IdTarefa { get; set; }

    [Required]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Required]
    [Column("id_lider")]
    public int IdLider { get; set; }

    [Required]
    [Column("id_tipo_tarefa")]
    public int IdTipoTarefa { get; set; }

    [Required]
    [Column("id_status_tarefa")]
    public int IdStatusTarefa { get; set; }

    [Column("descricao")]
    [MaxLength(500)]
    public string? Descricao { get; set; }

    [Column("tempo_estimado", TypeName = "NUMBER(5,2)")]
    public decimal? TempoEstimado { get; set; }

    [Column("tempo_real", TypeName = "NUMBER(5,2)")]
    public decimal? TempoReal { get; set; }

    [Column("data_inicio")]
    public DateTime? DataInicio { get; set; }

    [Column("data_fim")]
    public DateTime? DataFim { get; set; }

    // Navegações
    public Usuario? Usuario { get; set; }
    public Usuario? Lider { get; set; }
    public TipoTarefa? TipoTarefa { get; set; }
    public StatusTarefa? StatusTarefa { get; set; }
    public Feedback? Feedback { get; set; }
}
