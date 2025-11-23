namespace Avant.Api.Dtos
{
    // Entrada do endpoint de recomendação
    public class RecomendacaoPlanoEntradaDto
    {
        public int Idade { get; set; }
        public int AnosExperiencia { get; set; }
        public int CursosConcluidos { get; set; }

        // 0 = Júnior, 1 = Pleno, 2 = Sênior
        public int NivelAtual { get; set; }

        // 0 = Não deseja remoto, 1 = Deseja remoto
        public int DesejaTrabalhoRemoto { get; set; }
    }

    // Saída do endpoint de recomendação
    public class RecomendacaoPlanoSaidaDto
    {
        public float PontuacaoRecomendacao { get; set; }
        public string NivelSugerido { get; set; } = default!;
        public string SugestaoPlanoCarreira { get; set; } = default!;
    }
}
