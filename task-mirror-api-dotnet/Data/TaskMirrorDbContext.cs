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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PKs autogeradas (Oracle Identity)
        modelBuilder.Entity<Usuario>().Property(p => p.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<TipoTarefa>().Property(p => p.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<StatusTarefa>().Property(p => p.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<Tarefa>().Property(p => p.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<Feedback>().Property(p => p.Id).ValueGeneratedOnAdd();

        // Usuario self reference (Lider ? Subordinados)
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Lider)
            .WithMany(u => u!.Subordinados)
            .HasForeignKey(u => u.IdLider)
            .OnDelete(DeleteBehavior.Restrict);

        // 1:1 Tarefa ? Feedback (unique em TarefaId)
        modelBuilder.Entity<Feedback>()
            .HasIndex(f => f.TarefaId)
            .IsUnique();

        modelBuilder.Entity<Tarefa>()
            .HasOne(t => t.Feedback)
            .WithOne(f => f.Tarefa)
            .HasForeignKey<Feedback>(f => f.TarefaId);

        base.OnModelCreating(modelBuilder);
    }
}
