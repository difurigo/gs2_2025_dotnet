using Avant.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Avant.Api.Data
{
    public class AvantDbContext : DbContext
    {
        public AvantDbContext(DbContextOptions<AvantDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Equipe> Equipes => Set<Equipe>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Email único
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Relação Gerente (Usuario) 1 - N Equipes
            modelBuilder.Entity<Equipe>()
                .HasOne(e => e.Gerente)
                .WithMany(u => u.EquipesGerenciadas)
                .HasForeignKey(e => e.GerenteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relação Equipe 1 - N Funcionários (Usuarios)
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Equipe)
                .WithMany(e => e.Funcionarios)
                .HasForeignKey(u => u.EquipeId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
