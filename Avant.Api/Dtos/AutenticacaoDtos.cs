namespace Avant.Api.Dtos
{
    public class RegistroGerenteDto
    {
        public string Nome { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Senha { get; set; } = default!;
    }

    public class RegistroFuncionarioDto
    {
        public string Nome { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Senha { get; set; } = default!;
        public string? PlanoCarreira { get; set; }
        public Guid EquipeId { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; } = default!;
        public string Senha { get; set; } = default!;
    }

    public class RespostaAutenticacaoDto
    {
        public string Token { get; set; } = default!;
        public string Perfil { get; set; } = default!;
        public string Nome { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}
