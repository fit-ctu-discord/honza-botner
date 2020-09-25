using System.Threading.Tasks;

namespace HonzaBotner.Core.Contract
{
    public interface IAccessTokenProvider
    {
        Task<string?> GetTokenAsync();
    }
}
