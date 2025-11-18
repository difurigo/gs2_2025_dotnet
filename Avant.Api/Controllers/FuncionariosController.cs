using System.Security.Claims;
using Asp.Versioning;
using Avant.Api.Data;
using Avant.Api.Dtos;
using Avant.Api.Models;
using Avant.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avant.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class FuncionariosController : ControllerBase
    {
        private readonly AvantDbContext _contexto;
        private readonly ILogger<FuncionariosController> _logger;

        public FuncionariosController(AvantDbContext contexto, ILogger<FuncionariosController> logger)
        {
            _contexto = contexto;
            _logger = logger;
        }

        /// <summary>
        /// Cadastro de funcionário (feito pelo gerente).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Gerente")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegistrarFuncionario(
            [FromBody] RegistroFuncionarioDto dto,
            ApiVersion? version = null)
        {
            var equipe = await _contexto.Equipes.FindAsync(dto.EquipeId);
            if (equipe is null)
                return BadRequest("Equipe inválida.");

            var usuarioExistente = await _contexto.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuarioExistente is not null)
                return BadRequest("E-mail já cadastrado.");

            var funcionario = new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                SenhaHash = HashSenha.GerarHash(dto.Senha),
                Perfil = PerfilUsuario.Funcionario,
                PlanoCarreira = dto.PlanoCarreira,
                EquipeId = dto.EquipeId
            };

            await _contexto.Usuarios.AddAsync(funcionario);
            await _contexto.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterPorId),
                new { id = funcionario.Id, version = version?.ToString() ?? "1.0" }, null);
        }

        /// <summary>
        /// Funcionário atualiza seu plano de carreira.
        /// </summary>
        [HttpPut("{id:guid}/plano-carreira")]
        [Authorize(Roles = "Funcionario")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AtualizarPlanoCarreira(Guid id, [FromBody] string novoPlano)
        {
            var idUsuarioToken = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (idUsuarioToken != id)
                return Forbid();

            var usuario = await _contexto.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id && u.Perfil == PerfilUsuario.Funcionario);

            if (usuario is null)
                return NotFound();

            usuario.PlanoCarreira = novoPlano;
            await _contexto.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Listagem de funcionários com paginação + HATEOAS (apenas gerente).
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Gerente")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ResultadoPaginadoDto<object>>> Listar(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 10,
            ApiVersion? version = null)
        {
            if (pagina <= 0) pagina = 1;
            if (tamanhoPagina <= 0 || tamanhoPagina > 50) tamanhoPagina = 10;

            var query = _contexto.Usuarios
                .Include(u => u.Equipe)
                .Where(u => u.Perfil == PerfilUsuario.Funcionario)
                .AsNoTracking();

            var total = await query.CountAsync();

            // 🔥 Oracle-friendly: OrderBy + materializa antes de projetar
            var funcionariosRaw = await query
                .OrderBy(u => u.Nome)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            var funcionarios = funcionariosRaw
                .Select(u => new
                {
                    u.Id,
                    u.Nome,
                    u.Email,
                    u.PlanoCarreira,
                    Equipe = u.Equipe == null ? null : new
                    {
                        u.Equipe.Id,
                        u.Equipe.Nome
                    }
                })
                .ToList();

            var baseUrl = Url.ActionLink(nameof(Listar), "Funcionarios",
                new { version = version?.ToString() ?? "1.0" })!;

            var links = new List<LinkDto>
            {
                new()
                {
                    Rel = "self",
                    Href = $"{baseUrl}?pagina={pagina}&tamanhoPagina={tamanhoPagina}",
                    Metodo = "GET"
                }
            };

            if (pagina * tamanhoPagina < total)
            {
                links.Add(new LinkDto
                {
                    Rel = "proximo",
                    Href = $"{baseUrl}?pagina={pagina + 1}&tamanhoPagina={tamanhoPagina}",
                    Metodo = "GET"
                });
            }

            if (pagina > 1)
            {
                links.Add(new LinkDto
                {
                    Rel = "anterior",
                    Href = $"{baseUrl}?pagina={pagina - 1}&tamanhoPagina={tamanhoPagina}",
                    Metodo = "GET"
                });
            }

            var resultado = new ResultadoPaginadoDto<object>
            {
                Itens = funcionarios,
                Pagina = pagina,
                TamanhoPagina = tamanhoPagina,
                TotalItens = total,
                Links = links
            };

            return Ok(resultado);
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> ObterPorId(Guid id)
        {
            var usuario = await _contexto.Usuarios
                .Include(u => u.Equipe)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id && u.Perfil == PerfilUsuario.Funcionario);

            if (usuario is null)
                return NotFound();

            return Ok(new
            {
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.PlanoCarreira,
                Equipe = usuario.Equipe == null ? null : new
                {
                    usuario.Equipe.Id,
                    usuario.Equipe.Nome
                }
            });
        }
    }
}
