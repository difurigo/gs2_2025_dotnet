using Asp.Versioning;
using Avant.Api.Data;
using Avant.Api.Dtos;
using Avant.Api.Models;
using Avant.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avant.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AutenticacaoController : ControllerBase
    {
        private readonly AvantDbContext _contexto;
        private readonly IServicoToken _servicoToken;

        public AutenticacaoController(AvantDbContext contexto, IServicoToken servicoToken)
        {
            _contexto = contexto;
            _servicoToken = servicoToken;
        }

        /// <summary>
        /// Cadastro de gerente.
        /// </summary>
        [HttpPost("registrar-gerente")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegistrarGerente([FromBody] RegistroGerenteDto dto)
        {
            var usuarioExistente = await _contexto.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuarioExistente is not null)
                return BadRequest("E-mail já cadastrado.");

            var usuario = new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                SenhaHash = HashSenha.GerarHash(dto.Senha),
                Perfil = PerfilUsuario.Gerente
            };

            await _contexto.Usuarios.AddAsync(usuario);
            await _contexto.SaveChangesAsync();

            return CreatedAtAction(nameof(Login), new { email = usuario.Email }, null);
        }

        /// <summary>
        /// Login de usuário (gerente ou funcionário).
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RespostaAutenticacaoDto>> Login([FromBody] LoginDto dto)
        {
            var usuario = await _contexto.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario is null || !HashSenha.Verificar(dto.Senha, usuario.SenhaHash))
                return Unauthorized("Credenciais inválidas.");

            var token = _servicoToken.GerarToken(usuario);

            return Ok(new RespostaAutenticacaoDto
            {
                Token = token,
                Perfil = usuario.Perfil.ToString(),
                Nome = usuario.Nome,
                Email = usuario.Email
            });
        }
    }
}
