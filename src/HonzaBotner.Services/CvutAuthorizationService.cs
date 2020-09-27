using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Text.Json;

namespace HonzaBotner.Services
{
    public class CvutAuthorizationService : IAuthorizationService
    {
        private readonly HonzaBotnerDbContext _dbContext;
        private readonly CvutConfig _cvutConfig;
        private readonly IUsermapInfoService _usermapInfoService;
        private readonly IDiscordRoleManager _roleManager;
        private readonly HttpClient _client;

        public CvutAuthorizationService(HonzaBotnerDbContext dbContext, IOptions<CvutConfig> cvutConfig,
            IUsermapInfoService usermapInfoService, IDiscordRoleManager roleManager, HttpClient client)
        {
            _dbContext = dbContext;
            _cvutConfig = cvutConfig.Value;
            _usermapInfoService = usermapInfoService;
            _roleManager = roleManager;
            _client = client;
        }

        public async Task<string?> GetAuthorizationCodeAsync(ulong guildId, ulong userId)
        {
            Verification? verification = await _dbContext.Verifications
                .FirstOrDefaultAsync(v => v.GuildId == guildId && v.UserId == userId);

            if (verification != null)
            {
                return verification.Verified ? null : verification.VerificationId.ToString();
            }

            verification = new Verification
            {
                VerificationId = Guid.NewGuid(), Verified = false, GuildId = guildId, UserId = userId
            };

            await _dbContext.Verifications.AddAsync(verification);
            await _dbContext.SaveChangesAsync();

            return verification.VerificationId.ToString();
        }

        public async Task<bool> AuthorizeAsync(string accessToken, string userName, string code)
        {
            if (!Guid.TryParse(code, out Guid verificationId))
            {
                return false;
            }

            Verification? verification = await _dbContext.Verifications.FindAsync(verificationId);
            if (verification == null || verification.Verified)
            {
                return verification?.Verified ?? false;
            }

            UsermapPerson? person = await _usermapInfoService.GetUserInfoAsync(accessToken, userName);
            if (person == null ||
                await _dbContext.Verifications.AnyAsync(v => v.CvutUsername == person.Username))
            {
                return false;
            }

            IEnumerable<DiscordRole> discordRoles = _roleManager.MapUsermapRoles(person.Roles!.ToArray());
            bool rolesGranted =
                await _roleManager.GrantRolesAsync(verification.GuildId, verification.UserId, discordRoles);

            if (rolesGranted)
            {
                verification.Verified = true;
                verification.CvutUsername = person.Username;
                await _dbContext.SaveChangesAsync();
            }

            return rolesGranted;
        }

        public Task<string> GetAuthLink(string redirectUri)
        {
            const string authLink =
                "https://auth.fit.cvut.cz/oauth/authorize?response_type=code&client_id={0}&redirect_uri={1}";

            if (string.IsNullOrEmpty(_cvutConfig.ClientId))
            {
                throw new ArgumentNullException();
            }

            return Task.FromResult(string.Format(authLink, _cvutConfig.ClientId, redirectUri));
        }

        public async Task<bool> VerificationExistsAsync(string code)
        {
            if (!Guid.TryParse(code, out Guid verificationId))
            {
                return false;
            }

            Verification? verification = await _dbContext.Verifications.FindAsync(verificationId);
            if (verification == null)
            {
                return false;
            }

            return !verification.Verified;
        }

        private string GetQueryString(NameValueCollection queryCollection) 
            => string.Join('&', queryCollection.AllKeys.Select(k => $"{k}={HttpUtility.UrlEncode(queryCollection[k])}"));

        public async Task<string> GetAccessTokenAsync(string code, string redirectUri)
        {
            const string tokenUri = "https://auth.fit.cvut.cz/oauth/token";

            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(_cvutConfig.ClientId + ":" + _cvutConfig.ClientSecret));
            NameValueCollection queryCollection = new NameValueCollection
            {
                {"grant_type", "authorization_code"}, {"code", code}, {"redirect_uri", redirectUri}
            };

            var uriBuilder =new UriBuilder(tokenUri)
            {
                Query = GetQueryString(queryCollection)
            };

            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                RequestUri = uriBuilder.Uri,
                Headers = {Authorization = new AuthenticationHeaderValue("Basic", credentials)},
                Method = HttpMethod.Post
            };

            HttpResponseMessage tokenResponse = await _client.SendAsync(requestMessage);
            tokenResponse.EnsureSuccessStatusCode();

            JsonDocument response = await JsonDocument.ParseAsync(await tokenResponse.Content.ReadAsStreamAsync());

            return response.RootElement.GetProperty("access_token").GetString() ?? throw new InvalidOperationException();
        }

        public async Task<string> GetUserNameAsync(string accessToken)
        {
            const string checkTokenUri = "https://auth.fit.cvut.cz/oauth/check_token";
            
            var uriBuilder =new UriBuilder(checkTokenUri)
            {
                Query = $"token={accessToken}"
            };
            var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);

            HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            string responseText = await response.Content.ReadAsStringAsync();
            var user = JsonDocument.Parse(responseText);

            return user.RootElement.GetProperty("user_name").GetString() ?? throw new InvalidOperationException();
        }
    }
}
