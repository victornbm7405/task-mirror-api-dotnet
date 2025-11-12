using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMirror.Models;

[Table("tbl_usuarios")]
public class Usuario
{
    [Key]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Required]
    [Column("username")]
    [MaxLength(100)]
    public string Username { get; set; } = null!;

    [Required]
    [Column("password")]
    [MaxLength(255)]
    public string Password { get; set; } = null!;

    [Required]
    [Column("role_usuario")]
    [MaxLength(100)]
    public string RoleUsuario { get; set; } = null!;

    [Required]
    [Column("funcao")]
    [MaxLength(100)]
    public string Funcao { get; set; } = null!;

    [Column("id_lider")]
    public int? IdLider { get; set; }         // nullable para SetNull

    public Usuario? Lider { get; set; }       // self-join
    public ICollection<Usuario> Subordinados { get; set; } = new List<Usuario>();
}
