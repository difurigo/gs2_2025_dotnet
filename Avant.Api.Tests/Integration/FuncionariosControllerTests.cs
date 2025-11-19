using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Avant.Api.Tests;

namespace Avant.Api.Tests.Integration;

public class FuncionariosControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    private static JsonSerializerOptions JsonOptions => new()
    {
        PropertyNameCaseInsensitive = true
    };

    public FuncionariosControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> AutenticarGerenteEObterTokenAsync()
    {
        var registro = new
        {
            nome = "Gerente Funcionarios",
            email = "gerente.func@avant.com",
            senha = "Senha123!"
        };

        await _client.PostAsJsonAsync("/api/v1/Autenticacao/registrar-gerente", registro);

        var loginDto = new
        {
            email = registro.email,
            senha = registro.senha
        };

        var respLogin = await _client.PostAsJsonAsync("/api/v1/Autenticacao/login", loginDto);
        respLogin.EnsureSuccessStatusCode();

        var json = await respLogin.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        return json.GetProperty("token").GetString()!;
    }

    [Fact]
    public async Task ListarFuncionarios_DeveRetornarPaginadoComLinks()
    {
        // autentica gerente
        var token = await AutenticarGerenteEObterTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // cria equipe
        var equipeDto = new { nome = "Equipe Teste" };
        var respEquipe = await _client.PostAsJsonAsync("/api/v1/Equipes", equipeDto);
        respEquipe.EnsureSuccessStatusCode();

        var equipeJson = await respEquipe.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var equipeId = equipeJson.GetProperty("id").GetGuid();

        // cria funcionário
        var funcionarioDto = new
        {
            nome = "Funcionario Teste",
            email = "funcionario.teste@avant.com",
            senha = "Senha123!",
            planoCarreira = "Plano inicial",
            equipeId = equipeId
        };

        var respFunc = await _client.PostAsJsonAsync("/api/v1/Funcionarios", funcionarioDto);
        Assert.Equal(HttpStatusCode.Created, respFunc.StatusCode);

        // chama listagem
        var respLista = await _client.GetAsync("/api/v1/Funcionarios?pagina=1&tamanhoPagina=10");
        Assert.Equal(HttpStatusCode.OK, respLista.StatusCode);

        var jsonLista = await respLista.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        // verifica paginação
        var itens = jsonLista.GetProperty("itens");
        Assert.True(itens.ValueKind == JsonValueKind.Array);
        Assert.True(itens.GetArrayLength() >= 1);

        Assert.Equal(1, jsonLista.GetProperty("pagina").GetInt32());
        Assert.Equal(10, jsonLista.GetProperty("tamanhoPagina").GetInt32());
        Assert.True(jsonLista.GetProperty("totalItens").GetInt32() >= 1);

        // verifica HATEOAS
        var links = jsonLista.GetProperty("links");
        Assert.True(links.ValueKind == JsonValueKind.Array);
        Assert.True(links.GetArrayLength() >= 1);

        var self = links[0];
        Assert.Equal("self", self.GetProperty("rel").GetString());
        Assert.Contains("/api/v1/Funcionarios", self.GetProperty("href").GetString());
    }
}
