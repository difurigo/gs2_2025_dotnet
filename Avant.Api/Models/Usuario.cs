using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Avant.Api.Models
{
    public class Usuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = default!;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = default!;

        [Required]
        [StringLength(300)]
        public string SenhaHash { get; set; } = default!;

        [Required]
        public PerfilUsuario Perfil { get; set; }

        // Plano de carreira – faz sentido principalmente para Funcionário
        [StringLength(200)]
        public string? PlanoCarreira { get; set; }

        // Funcionário pertence a uma equipe (opcional até ser vinculado)
        public Guid? EquipeId { get; set; }
        public Equipe? Equipe { get; set; }

        // Se for Gerente, pode gerenciar várias equipes
        public ICollection<Equipe> EquipesGerenciadas { get; set; } = new List<Equipe>();
    }
}
