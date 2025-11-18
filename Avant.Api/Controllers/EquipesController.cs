using System.Security.Claims;
using Asp.Versioning;
using Avant.Api.Data;
using Avant.Api.Dtos;
using Avant.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avant.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Gerente")]
    public class EquipesController : ControllerBase
    {
        private readonly AvantDbContext _contexto;

        public EquipesController(AvantDbContext contexto)
        {
            _contexto = contexto;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<EquipeRespostaDto>> CriarEquipe([FromBody] CriarEquipeDto dto, ApiVersion? version = null)
        {
            var idUsuario = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var gerente = await _contexto.Usuarios
                .FirstOrDefaultAsync(u => u.Id == idUsuario && u.Perfil == PerfilUsuario.Gerente);

            if (gerente is null)
                return Forbid();

            var equipe = new Equipe
            {
                Nome = dto.Nome,
                GerenteId = gerente.Id
            };

            await _contexto.Equipes.AddAsync(equipe);
            await _contexto.SaveChangesAsync();

            var resposta = new EquipeRespostaDto
            {
                Id = equipe.Id,
                Nome = equipe.Nome,
                GerenteId = equipe.GerenteId
            };

            return CreatedAtAction(nameof(ObterPorId), new { id = equipe.Id, version = version?.ToString() ?? "1.0" }, resposta);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EquipeRespostaDto>> ObterPorId(Guid id)
        {
            var equipe = await _contexto.Equipes.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

            if (equipe is null)
                return NotFound();

            return Ok(new EquipeRespostaDto
            {
                Id = equipe.Id,
                Nome = equipe.Nome,
                GerenteId = equipe.GerenteId
            });
        }
    }
}
