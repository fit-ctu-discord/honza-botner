using System;
using System.Threading.Tasks;

namespace HonzaBotner.Services.Contract
{
    public interface IAuthorizationService
    {
        Task<bool> AuthorizeAsync(string accessToken, string username, ulong userId);

        Task<string> GetAuthLinkAsync(string redirectUri);

        Task<bool> IsUserVerified(ulong userId);

        Task<string> GetAccessTokenAsync(string code, string redirectUri);

        Task<string> GetUserNameAsync(string accessToken);
    }
}
