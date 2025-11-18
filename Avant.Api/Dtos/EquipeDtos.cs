namespace Avant.Api.Dtos
{
    public class CriarEquipeDto
    {
        public string Nome { get; set; } = default!;
    }

    public class EquipeRespostaDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = default!;
        public Guid GerenteId { get; set; }
    }
}
