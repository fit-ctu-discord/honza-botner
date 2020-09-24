using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HonzaBotner.Core.Contract;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace HonzaBotner.Services
{
    public class AccessTokenProvider : IAccessTokenProvider
    {
        private const string TokenName = "access_token";

        private readonly HttpContext _httpContext;

        public AccessTokenProvider(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _httpContext.GetTokenAsync(TokenName);
        }
    }
}
