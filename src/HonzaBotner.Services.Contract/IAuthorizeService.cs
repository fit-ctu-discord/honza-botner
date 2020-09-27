using System;
using System.Threading.Tasks;

namespace HonzaBotner.Services.Contract
{
    public interface IAuthorizationService
    {
        Task<string?> GetAuthorizationCodeAsync(ulong guildId, ulong userId);

        Task<bool> AuthorizeAsync(string code);

        Task<string> GetAuthLink(string redirectUri);

        Task<bool> VerificationExistsAsync(string code);
    }
}
