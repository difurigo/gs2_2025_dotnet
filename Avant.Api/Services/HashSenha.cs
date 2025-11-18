using System.Security.Cryptography;
using System.Text;

namespace Avant.Api.Services
{
    public static class HashSenha
    {
        public static string GerarHash(string senha)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(senha));
            return Convert.ToBase64String(bytes);
        }

        public static bool Verificar(string senha, string hash) =>
            GerarHash(senha) == hash;
    }
}
