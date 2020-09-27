using System;
using System.Threading.Tasks;

namespace HonzaBotner.Services.Contract
{
    public interface IAuthorizationService
    {
        Task<string?> GetAuthorizationCodeAsync(ulong guildId, ulong userId);

        Task<bool> AuthorizeAsync(string code, string accessToken, string userName);

        Task<string> GetAuthLink(string redirectUri);

        Task<bool> VerificationExistsAsync(string code);

        Task<string> GetAccessTokenAsync(string code, string redirectUri);

        Task<string> GetUserNameAsync(string accessToken);
    }
}
