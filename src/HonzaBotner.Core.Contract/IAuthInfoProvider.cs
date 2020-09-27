using System.Threading.Tasks;

namespace HonzaBotner.Core.Contract
{
    public interface IAuthInfoProvider
    {
        Task<string?> GetTokenAsync();
        Task<string?> GetUsernameAsync();
    }
}
