using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Services
{
    public sealed class UserMapInfoService : IUsermapInfoService
    {
        private readonly HttpClient _httpClient;

        public UserMapInfoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UsermapPerson?> GetUserInfoAsync(string accessToken, string userName)
        {
            UriBuilder uriBuilder = new($"https://kosapi.fit.cvut.cz/usermap/v1/people/{userName}?'");

            HttpRequestMessage request = new(HttpMethod.Get, uriBuilder.Uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true
            };

            return await JsonSerializer.DeserializeAsync<UsermapPerson>(await response.Content.ReadAsStreamAsync(), options);
        }
    }
}
