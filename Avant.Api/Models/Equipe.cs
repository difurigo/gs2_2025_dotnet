using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Avant.Api.Models
{
    public class Equipe
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = default!;

        // Gerente responsável pela equipe
        [Required]
        public Guid GerenteId { get; set; }

        public Usuario Gerente { get; set; } = default!;

        // Funcionários da equipe
        public ICollection<Usuario> Funcionarios { get; set; } = new List<Usuario>();
    }
}
