using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Services
{
    public class CvutAuthorizationService : IAuthorizationService
    {
        private readonly HonzaBotnerDbContext _dbContext;
        private readonly CvutConfig _cvutConfig;
        private readonly IUsermapInfoService _usermapInfoService;
        private readonly IDiscordRoleManager _roleManager;
        private readonly HttpClient _client;
        private readonly IHashService _hashService;
        private readonly ILogger<CvutAuthorizationService> _logger;

        public CvutAuthorizationService(HonzaBotnerDbContext dbContext, IOptions<CvutConfig> cvutConfig,
            IUsermapInfoService usermapInfoService, IDiscordRoleManager roleManager, HttpClient client,
            IHashService hashService, ILogger<CvutAuthorizationService> logger)
        {
            _dbContext = dbContext;
            _cvutConfig = cvutConfig.Value;
            _usermapInfoService = usermapInfoService;
            _roleManager = roleManager;
            _client = client;
            _hashService = hashService;
            _logger = logger;
        }

        public async Task<IAuthorizationService.AuthorizeResult> AuthorizeAsync(string accessToken, string username,
            ulong userId, RolesPool rolesPool)
        {
            bool discordIdPresent = await IsUserVerified(userId);

            UsermapPerson? person = await _usermapInfoService.GetUserInfoAsync(accessToken, username);
            if (person == null)
            {
                _logger.LogWarning("Couldn't fetch info from UserMap");
                return IAuthorizationService.AuthorizeResult.UserMapError;
            }

            string authId = _hashService.Hash(person.Username);
            bool authPresent = await _dbContext.Verifications.AnyAsync(v => v.AuthId == authId);

            IReadOnlySet<DiscordRole> discordRoles = _roleManager.MapUsermapRoles(person.Roles, rolesPool);

            // discord and auth -> update roles
            if (discordIdPresent && authPresent)
            {
                bool verificationExists =
                    await _dbContext.Verifications.AnyAsync(v => v.UserId == userId && v.AuthId == authId);

                if (verificationExists)
                {
                    bool revoked = await _roleManager.RevokeRolesPoolAsync(userId, rolesPool);
                    if (!revoked)
                    {
                        _logger.LogWarning("Revoking roles pool {RolesPool} for {Username} (id {Id}) failed", userId,
                            username, rolesPool);
                        return IAuthorizationService.AuthorizeResult.Failed;
                    }

                    bool granted = await _roleManager.GrantRolesAsync(userId, discordRoles);
                    return granted
                        ? IAuthorizationService.AuthorizeResult.OK
                        : IAuthorizationService.AuthorizeResult.Failed;
                }

                return IAuthorizationService.AuthorizeResult.DifferentMember;
            }

            // discord xor auth -> user already verified, error
            if (discordIdPresent || authPresent)
            {
                return IAuthorizationService.AuthorizeResult.DifferentMember;
            }

            // nothing -> create database entry, update roles
            {
                bool rolesGranted = await _roleManager.GrantRolesAsync(userId, discordRoles);

                if (rolesGranted)
                {
                    Verification verification = new() {AuthId = authId, UserId = userId};

                    await _dbContext.Verifications.AddAsync(verification);
                    await _dbContext.SaveChangesAsync();
                    await _roleManager.RevokeHostRolesAsync(userId);
                    return IAuthorizationService.AuthorizeResult.OK;
                }

                return IAuthorizationService.AuthorizeResult.Failed;
            }
        }

        public Task<string> GetAuthLinkAsync(string redirectUri)
        {
            const string authLink =
                "https://auth.fit.cvut.cz/oauth/authorize?response_type=code&client_id={0}&redirect_uri={1}";

            if (string.IsNullOrEmpty(_cvutConfig.ClientId))
            {
                throw new ArgumentNullException(null, "Invalid config");
            }

            return Task.FromResult(string.Format(authLink, _cvutConfig.ClientId, redirectUri));
        }

        public async Task<bool> IsUserVerified(ulong userId)
        {
            return await _dbContext.Verifications
                .AnyAsync(v => v.UserId == userId);
        }

        public async Task<string> GetAccessTokenAsync(string code, string redirectUri)
        {
            const string tokenUri = "https://auth.fit.cvut.cz/oauth/token";

            string credentials =
                Convert.ToBase64String(Encoding.UTF8.GetBytes(_cvutConfig.ClientId + ":" + _cvutConfig.ClientSecret));
            NameValueCollection queryCollection = new()
            {
                {"grant_type", "authorization_code"}, {"code", code}, {"redirect_uri", redirectUri}
            };

            UriBuilder uriBuilder = new(tokenUri) {Query = queryCollection.GetQueryString()};

            HttpRequestMessage requestMessage = new()
            {
                RequestUri = uriBuilder.Uri,
                Headers = {Authorization = new AuthenticationHeaderValue("Basic", credentials)},
                Method = HttpMethod.Post
            };

            HttpResponseMessage tokenResponse = await _client.SendAsync(requestMessage);

            try
            {
                tokenResponse.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Couldn't authorize user, status code is not successful.", e);
            }

            JsonDocument response = await JsonDocument.ParseAsync(await tokenResponse.Content.ReadAsStreamAsync());

            return response.RootElement.GetProperty("access_token").GetString()
                   ?? throw new InvalidOperationException("Couldn't authorize user.");
        }

        public async Task<string> GetUserNameAsync(string accessToken)
        {
            const string checkTokenUri = "https://auth.fit.cvut.cz/oauth/check_token";

            UriBuilder uriBuilder = new(checkTokenUri) {Query = $"token={accessToken}"};
            HttpRequestMessage request = new(HttpMethod.Get, uriBuilder.Uri);

            HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            string responseText = await response.Content.ReadAsStringAsync();
            JsonDocument user = JsonDocument.Parse(responseText);

            return user.RootElement.GetProperty("user_name").GetString()
                   ?? throw new InvalidOperationException("Couldn't load information about user");
        }

        public async Task<string> GetServiceTokenAsync(string scope)
        {
            const string tokenUri = "https://auth.fit.cvut.cz/oauth/oauth/token";

            // TOOD(ostorc): add cache keyed by scope, with lifetime based on response

            UriBuilder uriBuilder = new(tokenUri);

            List<KeyValuePair<string?, string?>> contentValues = new()
            {
                new("grant_type", "client_credentials"),
                new("client_id", _cvutConfig.ServiceId),
                new("client_secret", _cvutConfig.ServiceSecret),
                new("scope", scope)
            };

            FormUrlEncodedContent content = new(contentValues);

            HttpRequestMessage requestMessage = new()
            {
                RequestUri = uriBuilder.Uri,
                Method = HttpMethod.Post,
                Content = content
            };

            HttpResponseMessage tokenResponse = await _client.SendAsync(requestMessage).ConfigureAwait(false);
            tokenResponse.EnsureSuccessStatusCode();

            JsonDocument response = await JsonDocument.ParseAsync(await tokenResponse.Content.ReadAsStreamAsync().ConfigureAwait(false));

            return response.RootElement.GetProperty("access_token").GetString()
                   ?? throw new InvalidOperationException("Couldn't get service token from CTU");
        }
    }
}
