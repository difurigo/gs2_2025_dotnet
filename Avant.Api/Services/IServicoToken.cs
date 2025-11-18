using Avant.Api.Models;

namespace Avant.Api.Services
{
    public interface IServicoToken
    {
        string GerarToken(Usuario usuario);
    }
}
