using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Avant.Api.Tests; // pra enxergar o CustomWebApplicationFactory

namespace Avant.Api.Tests.Integration;

public class AutenticacaoControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AutenticacaoControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static JsonSerializerOptions JsonOptions => new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task RegistrarGerente_DeveRetornar201Created()
    {
        var dto = new
        {
            nome = "Gerente Teste",
            email = "gerente.teste@avant.com",
            senha = "Senha123!"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Autenticacao/registrar-gerente", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarToken()
    {
        var registro = new
        {
            nome = "Gerente Login",
            email = "gerente.login@avant.com",
            senha = "Senha123!"
        };

        await _client.PostAsJsonAsync("/api/v1/Autenticacao/registrar-gerente", registro);

        var loginDto = new
        {
            email = registro.email,
            senha = registro.senha
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Autenticacao/login", loginDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        Assert.True(corpo.TryGetProperty("token", out var tokenProp));
        Assert.False(string.IsNullOrWhiteSpace(tokenProp.GetString()));
    }

    [Fact]
    public async Task Login_ComSenhaInvalida_DeveRetornar401()
    {
        var registro = new
        {
            nome = "Gerente SenhaErrada",
            email = "gerente.senha@avant.com",
            senha = "Senha123!"
        };

        await _client.PostAsJsonAsync("/api/v1/Autenticacao/registrar-gerente", registro);

        var loginDto = new
        {
            email = registro.email,
            senha = "SenhaErrada!"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Autenticacao/login", loginDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
