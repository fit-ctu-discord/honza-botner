using System;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Services
{
    public class AppUrlProvider : IUrlProvider
    {
        private readonly CvutConfig _cvutConfig;

        public AppUrlProvider(IOptions<CvutConfig> config)
        {
            _cvutConfig = config.Value;
        }

        public string GetAuthLink(ulong userId, RolesPool pool)
        {
            const string authPath = "/Auth/Authenticate/";

            if (_cvutConfig.AppBaseUrl == null)
            {
                throw new InvalidOperationException("Invalid CVUT config");
            }

            return $"{_cvutConfig.AppBaseUrl}{authPath}{userId}/{pool.ToString().ToLowerInvariant()}";
        }
    }
}
