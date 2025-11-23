using Asp.Versioning;
using Avant.Api.Dtos;
using Avant.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Avant.Api.Controllers
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/Funcionarios")]
    public class FuncionariosV2Controller : ControllerBase
    {
        private readonly PlanoCarreiraMlService _planoCarreiraMlService;

        public FuncionariosV2Controller(PlanoCarreiraMlService planoCarreiraMlService)
        {
            _planoCarreiraMlService = planoCarreiraMlService;
        }

        /// <summary>
        /// Recomendação de plano de carreira com ML.NET (v2)
        /// </summary>
        [HttpPost("recomendacao-plano")]
        [Authorize(Roles = "Gerente,Funcionario")]
        [ProducesResponseType(typeof(RecomendacaoPlanoSaidaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<RecomendacaoPlanoSaidaDto> RecomendarPlano([FromBody] RecomendacaoPlanoEntradaDto entrada)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = _planoCarreiraMlService.RecomendarPlano(entrada);
            return Ok(resultado);
        }
    }
}
