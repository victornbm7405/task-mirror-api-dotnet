using Microsoft.EntityFrameworkCore;
using TaskMirror.Models;

namespace TaskMirror.Data;

public class TaskMirrorDbContext : DbContext
{
    public TaskMirrorDbContext(DbContextOptions<TaskMirrorDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<TipoTarefa> TiposTarefa => Set<TipoTarefa>();
    public DbSet<StatusTarefa> StatusesTarefa => Set<StatusTarefa>();
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Usuario>()
             .HasIndex(u => u.Username)
             .IsUnique();

        model.Entity<Usuario>()
             .HasOne(u => u.Lider)
             .WithMany(u => u.Subordinados)
             .HasForeignKey(u => u.IdLider)
             .OnDelete(DeleteBehavior.SetNull)
             .HasConstraintName("fk_usuarios_lider");

        model.Entity<Tarefa>()
             .HasOne(t => t.Usuario)
             .WithMany()
             .HasForeignKey(t => t.IdUsuario)
             .OnDelete(DeleteBehavior.Restrict)
             .HasConstraintName("fk_tarefas_usuario");

        model.Entity<Tarefa>()
             .HasOne(t => t.Lider)
             .WithMany()
             .HasForeignKey(t => t.IdLider)
             .OnDelete(DeleteBehavior.Restrict)
             .HasConstraintName("fk_tarefas_lider");

        model.Entity<Tarefa>()
             .HasOne(t => t.TipoTarefa)
             .WithMany()
             .HasForeignKey(t => t.IdTipoTarefa)
             .OnDelete(DeleteBehavior.Restrict)
             .HasConstraintName("fk_tarefas_tipo");

        model.Entity<Tarefa>()
             .HasOne(t => t.StatusTarefa)
             .WithMany()
             .HasForeignKey(t => t.IdStatusTarefa)
             .OnDelete(DeleteBehavior.Restrict)
             .HasConstraintName("fk_tarefas_status");

        model.Entity<Tarefa>()
             .Property(t => t.TempoEstimado)
             .HasColumnType("NUMBER(5,2)");

        model.Entity<Tarefa>()
             .Property(t => t.TempoReal)
             .HasColumnType("NUMBER(5,2)");

        model.Entity<Feedback>()
             .HasIndex(f => f.IdTarefa)
             .IsUnique();

        model.Entity<Tarefa>()
             .HasOne(t => t.Feedback)
             .WithOne(f => f.Tarefa)
             .HasForeignKey<Feedback>(f => f.IdTarefa)
             .OnDelete(DeleteBehavior.Cascade)
             .HasConstraintName("fk_feedback_tarefa");

        base.OnModelCreating(model);
    }
}
