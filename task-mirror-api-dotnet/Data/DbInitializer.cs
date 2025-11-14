using System.Linq;
using Microsoft.EntityFrameworkCore;
using TaskMirror.Models;

namespace TaskMirror.Data
{
    public static class DbInitializer
    {
        public static void Seed(TaskMirrorDbContext db)
        {
            // Se você já chama Migrate() no Program.cs, pode comentar a linha abaixo.
            db.Database.Migrate();

            // ----------------------------------------------------------
            // Helpers que NÃO projetam True/False no SQL (compat. Oracle)
            // ----------------------------------------------------------
            static bool HasAny<TEntity>(IQueryable<TEntity> q) where TEntity : class
            {
                // Gera: SELECT 1 FROM ... FETCH FIRST 1 ROWS ONLY
                var one = q.AsNoTracking().Select(_ => 1).Take(1).FirstOrDefault();
                return one == 1;
            }

            static bool HasStatusByName(TaskMirrorDbContext ctx, string nome)
            {
                // Comparação case-insensitive sem retornar True/False
                var nomeUP = nome.Trim().ToUpper();
                var one = ctx.StatusesTarefa
                    .AsNoTracking()
                    .Where(s => s.Nome.ToUpper() == nomeUP)
                    .Select(_ => 1)
                    .Take(1)
                    .FirstOrDefault();
                return one == 1;
            }

            // ----------------------------------------------------------
            // TIPOS DE TAREFA (seed simples)
            // ----------------------------------------------------------
            if (!HasAny(db.TiposTarefa))
            {
                db.TiposTarefa.AddRange(
                    new TipoTarefa { Nome = "Desenvolvimento" },
                    new TipoTarefa { Nome = "Correção" },
                    new TipoTarefa { Nome = "Documentação" }
                );
                db.SaveChanges();
            }

            // ----------------------------------------------------------
            // STATUS DE TAREFA — padrão único:
            // "Pendente", "Em Andamento", "Finalizado"
            // + normalização de nomes antigos
            // ----------------------------------------------------------
            // 1) Normaliza nomes antigos, se existirem
            var existentes = db.StatusesTarefa.ToList();
            if (existentes.Count > 0)
            {
                foreach (var s in existentes)
                {
                    if (string.IsNullOrWhiteSpace(s.Nome)) continue;

                    var up = s.Nome.Trim().ToUpperInvariant();
                    if (up is "EM PROGRESSO" or "EM_PROGRESSO")
                        s.Nome = "Em Andamento";
                    else if (up is "CONCLUIDA" or "CONCLUÍDA")
                        s.Nome = "Finalizado";
                }
                db.SaveChanges();
            }

            // 2) Garante os 3 nomes canônicos
            void EnsureStatus(string nome)
            {
                if (!HasStatusByName(db, nome))
                {
                    db.StatusesTarefa.Add(new StatusTarefa { Nome = nome });
                }
            }

            EnsureStatus("Pendente");
            EnsureStatus("Em Andamento");
            EnsureStatus("Finalizado");
            db.SaveChanges();

            // ----------------------------------------------------------
            // USUÁRIOS EXEMPLO (líder e dev)
            // ----------------------------------------------------------
            if (!HasAny(db.Usuarios))
            {
                var lider = new Usuario
                {
                    Username = "lider",
                    Password = "123456",
                    RoleUsuario = "LIDER",
                    Funcao = "Líder"
                };
                db.Usuarios.Add(lider);
                db.SaveChanges();

                var dev = new Usuario
                {
                    Username = "dev",
                    Password = "123456",
                    RoleUsuario = "USER",
                    Funcao = "Desenvolvedor",
                    IdLider = lider.IdUsuario
                };
                db.Usuarios.Add(dev);
                db.SaveChanges();
            }

            // ----------------------------------------------------------
            // TAREFAS EXEMPLO (duas tarefas pendentes para o dev)
            // ----------------------------------------------------------
            if (!HasAny(db.Tarefas))
            {
                var tipo = db.TiposTarefa.AsNoTracking().First();

                var pendenteId = db.StatusesTarefa.AsNoTracking()
                    .Where(s => s.Nome.ToUpper() == "PENDENTE")
                    .Select(s => s.IdStatusTarefa)
                    .First();

                var liderId = db.Usuarios.AsNoTracking()
                    .Where(u => u.Username == "lider")
                    .Select(u => u.IdUsuario)
                    .First();

                var devId = db.Usuarios.AsNoTracking()
                    .Where(u => u.Username == "dev")
                    .Select(u => u.IdUsuario)
                    .First();

                db.Tarefas.AddRange(
                    new Tarefa
                    {
                        IdUsuario = devId,
                        IdLider = liderId,
                        IdTipoTarefa = tipo.IdTipoTarefa,
                        IdStatusTarefa = pendenteId,
                        Descricao = "Implementar endpoint X",
                        TempoEstimado = 90
                    },
                    new Tarefa
                    {
                        IdUsuario = devId,
                        IdLider = liderId,
                        IdTipoTarefa = tipo.IdTipoTarefa,
                        IdStatusTarefa = pendenteId,
                        Descricao = "Ajustar Swagger e Healthchecks",
                        TempoEstimado = 60
                    }
                );
                db.SaveChanges();
            }
        }
    }
}
