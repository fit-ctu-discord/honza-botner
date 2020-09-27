using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using HonzaBotner.Core.Contract;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Services
{
    public sealed class UserMapInfoService : IUsermapInfoService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthInfoProvider _authInfoProvider;

        public UserMapInfoService(HttpClient httpClient, IAuthInfoProvider authInfoProvider)
        {
            _httpClient = httpClient;
            _authInfoProvider = authInfoProvider;
        }

        public async Task<UsermapPerson?> GetUserInfoAsync()
        {
            string? accessToken = await _authInfoProvider.GetTokenAsync();
            string? username = await _authInfoProvider.GetUsernameAsync();
            UriBuilder uriBuilder = new UriBuilder($"https://kosapi.fit.cvut.cz/usermap/v1/people/{username}?'");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();


            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return await JsonSerializer.DeserializeAsync<UsermapPerson>(await response.Content.ReadAsStreamAsync(), options);
        }
    }
}
