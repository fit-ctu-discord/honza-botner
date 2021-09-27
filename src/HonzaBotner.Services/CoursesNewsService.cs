using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Services
{
    public class CoursesNewsService : INewsService
    {
#nullable disable
        private class CoursesNews
        {
            internal class Person
            {
                [JsonPropertyName("name")]
                public string Name { get; set; }

                [JsonPropertyName("uri")]
                public string Uri { get; set; }
            }

            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("content")]
            public string Content { get; set; }

            [JsonPropertyName("createdAt")]
            public DateTime CreatedAt { get; set; }

            [JsonPropertyName("createdBy")]
            public Person CreatedBy { get; set; }

            [JsonPropertyName("publishedAt")]
            public DateTime PublishedAt { get; set; }

            [JsonPropertyName("ref")]
            public string Ref { get; set; }

            [JsonPropertyName("deleted")]
            public bool Deleted { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("course")]
            public string Course { get; set; }

            [JsonPropertyName("audience")]
            public List<string> Audience { get; set; }

            [JsonPropertyName("modifiedAt")]
            public DateTime? ModifiedAt { get; set; }

            [JsonPropertyName("modifiedBy")]
            public Person ModifiedBy { get; set; }
        }
#nullable enable

        private const string CoursesScope = "cvut:cpages:common:read";
        private readonly ILogger<CoursesNewsService> _logger;
        private readonly HttpClient _client;
        private readonly IAuthorizationService _authorizationService;

        public CoursesNewsService(ILogger<CoursesNewsService> logger, HttpClient client, IAuthorizationService authorizationService)
        {
            _logger = logger;
            _client = client;
            _authorizationService = authorizationService;
        }

        public async IAsyncEnumerable<News> FetchDataAsync(string source, DateTime since)
        {
            string accessToken = await _authorizationService.GetServiceTokenAsync(CoursesScope).ConfigureAwait(false);

            NameValueCollection queryParams = new()
            {
                //{ "access_token", accessToken },
                // type: {  "default", "grouped", "jsonfeed" }
                { "type", "default" },
                // courses: BI-XY,BI-YZ,...
                { "courses", source },
                { "limit", "50" },
                { "since", since.ToString("yyyy-MM-dd") }
            };

            UriBuilder uriBuilder = new("https://courses.fit.cvut.cz/api/v1/cpages/news")
            {
                Query = queryParams.GetQueryString()
            };

            HttpRequestMessage requestMessage = new(HttpMethod.Get, uriBuilder.Uri)
            {
                Headers = { Authorization = new("Bearer", accessToken) }
            };

            HttpResponseMessage responseMessage = await _client.SendAsync(requestMessage);
            responseMessage.EnsureSuccessStatusCode();

            CoursesNews[]? coursesNews = await JsonSerializer.DeserializeAsync<CoursesNews[]>(await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false));

            if (coursesNews is null)
            {
                yield break;
            }

            foreach (CoursesNews item in coursesNews)
            {
                // TODO: Think about the content. Is it good enough how it is now, or should we imporve it by adding audience or so.

                yield return new News(item.Url, item.CreatedBy.Name, item.Title, item.Content, item.PublishedAt);
            }
        }
    }
}
