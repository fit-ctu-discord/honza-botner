using System.Threading.Tasks;
using HonzaBotner.Core.Contract;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace HonzaBotner.Services
{
    public class AccessTokenProvider : IAccessTokenProvider
    {
        private const string TokenName = "access_token";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccessTokenProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string?> GetTokenAsync()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return null;
            }

            return await _httpContextAccessor.HttpContext.GetTokenAsync(TokenName);
        }
    }
}
