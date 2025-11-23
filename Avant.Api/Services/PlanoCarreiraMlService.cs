using Microsoft.ML;
using Microsoft.ML.Data;
using Avant.Api.Dtos;

namespace Avant.Api.Services
{
    // Classe de features usadas pelo modelo
    public class PlanoCarreiraFeatures
    {
        [LoadColumn(0)] public float Idade { get; set; }
        [LoadColumn(1)] public float AnosExperiencia { get; set; }
        [LoadColumn(2)] public float CursosConcluidos { get; set; }
        [LoadColumn(3)] public float NivelAtual { get; set; }
        [LoadColumn(4)] public float DesejaTrabalhoRemoto { get; set; }

        // Label = "potencial" de evolução
        [LoadColumn(5)] public float Label { get; set; }
    }

    // Saída bruta do modelo
    public class PlanoCarreiraPrediction
    {
        [ColumnName("Score")]
        public float Score { get; set; }
    }

    public class PlanoCarreiraMlService
    {
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<PlanoCarreiraFeatures, PlanoCarreiraPrediction> _predictionEngine;

        public PlanoCarreiraMlService()
        {
            _mlContext = new MLContext();

            // Dados de treino simples e "didáticos" só pra demonstrar o uso do ML.NET
            var dadosTreino = new List<PlanoCarreiraFeatures>
            {
                // Idade, Exp, Cursos, NivelAtual, Remoto, Label (potencial)
                new PlanoCarreiraFeatures { Idade = 20, AnosExperiencia = 0, CursosConcluidos = 1, NivelAtual = 0, DesejaTrabalhoRemoto = 1, Label = 0.3f },
                new PlanoCarreiraFeatures { Idade = 25, AnosExperiencia = 2, CursosConcluidos = 3, NivelAtual = 0, DesejaTrabalhoRemoto = 1, Label = 0.5f },
                new PlanoCarreiraFeatures { Idade = 28, AnosExperiencia = 4, CursosConcluidos = 4, NivelAtual = 1, DesejaTrabalhoRemoto = 1, Label = 0.7f },
                new PlanoCarreiraFeatures { Idade = 32, AnosExperiencia = 7, CursosConcluidos = 5, NivelAtual = 1, DesejaTrabalhoRemoto = 0, Label = 0.8f },
                new PlanoCarreiraFeatures { Idade = 40, AnosExperiencia = 10, CursosConcluidos = 6, NivelAtual = 2, DesejaTrabalhoRemoto = 0, Label = 0.9f },
            };

            var dataView = _mlContext.Data.LoadFromEnumerable(dadosTreino);

            var pipeline = _mlContext.Transforms.Concatenate(
                    "Features",
                    nameof(PlanoCarreiraFeatures.Idade),
                    nameof(PlanoCarreiraFeatures.AnosExperiencia),
                    nameof(PlanoCarreiraFeatures.CursosConcluidos),
                    nameof(PlanoCarreiraFeatures.NivelAtual),
                    nameof(PlanoCarreiraFeatures.DesejaTrabalhoRemoto)
                )
                .Append(_mlContext.Regression.Trainers.Sdca(
                    labelColumnName: "Label",
                    featureColumnName: "Features"));

            var model = pipeline.Fit(dataView);

            _predictionEngine = _mlContext.Model.CreatePredictionEngine<PlanoCarreiraFeatures, PlanoCarreiraPrediction>(model);
        }

        public RecomendacaoPlanoSaidaDto RecomendarPlano(RecomendacaoPlanoEntradaDto entrada)
        {
            var features = new PlanoCarreiraFeatures
            {
                Idade = entrada.Idade,
                AnosExperiencia = entrada.AnosExperiencia,
                CursosConcluidos = entrada.CursosConcluidos,
                NivelAtual = entrada.NivelAtual,
                DesejaTrabalhoRemoto = entrada.DesejaTrabalhoRemoto,
                Label = 0 // não usado na previsão
            };

            var prediction = _predictionEngine.Predict(features);
            var score = prediction.Score;

            // Regras simples para converter score em nível sugerido
            string nivelSugerido;
            string sugestaoPlano;

            if (score < 0.45)
            {
                nivelSugerido = "Júnior";
                sugestaoPlano = "Focar em formação técnica básica, cursos introdutórios e projetos guiados.";
            }
            else if (score < 0.75)
            {
                nivelSugerido = "Pleno";
                sugestaoPlano = "Aprofundar especialização, assumir pequenas lideranças de projeto e mentorias pontuais.";
            }
            else
            {
                nivelSugerido = "Sênior";
                sugestaoPlano = "Atuar como referência técnica, liderar times e apoiar decisões estratégicas da área.";
            }

            return new RecomendacaoPlanoSaidaDto
            {
                PontuacaoRecomendacao = score,
                NivelSugerido = nivelSugerido,
                SugestaoPlanoCarreira = sugestaoPlano
            };
        }
    }
}
