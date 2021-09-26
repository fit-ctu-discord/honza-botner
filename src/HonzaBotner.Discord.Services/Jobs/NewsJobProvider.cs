using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HonzaBotner.Scheduler.Contract;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Jobs
{
    [Cron("0 */10 * * * *")]
    public class NewsJobProvider : IJob
    {
        public string Name { get; } = "news-publisher";

        private readonly ILogger<NewsJobProvider> _logger;
        private readonly INewsConfigService _configService;
        private readonly IServiceProvider _serviceProvider;

        private static readonly ConcurrentDictionary<string, Type> s_typesCache = new();

        public NewsJobProvider(ILogger<NewsJobProvider> logger, INewsConfigService configService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configService = configService;
            _serviceProvider = serviceProvider;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting news fetching");

            IList<NewsConfig> sources = await _configService.ListActiveConfigsAsync();

            using IServiceScope scope = _serviceProvider.CreateAsyncScope();

            foreach (NewsConfig newsSource in sources)
            {
                INewsService newsService = scope.ServiceProvider.GetRequiredService(GetType(newsSource.NewsProviderType)) as INewsService
                    ?? throw new InvalidCastException("Type must be INewsService");
                IPublisherService publisherService = scope.ServiceProvider.GetRequiredService(GetType(newsSource.PublisherType)) as IPublisherService
                    ?? throw new InvalidCastException("Type must be IPublisherService");

                DateTime now = DateTime.Now;
                IAsyncEnumerable<News> news = newsService.FetchDataAsync(newsSource.Source, now);

                await foreach (News item in news.WithCancellation(cancellationToken))
                {
                    await publisherService.Publish(item);
                }

                await _configService.UpdateFetchDateAsync(newsSource.Id, now);
            }
            
        }

        private static Type GetType(string typeName)
        {
            if (s_typesCache.TryGetValue(typeName, out Type? type))
            {
                return type;
            }

            type = Type.GetType(typeName) ?? throw new ArgumentOutOfRangeException(nameof(typeName), $"Invalid type: {typeName}");

            return s_typesCache[typeName] = type;
        }
    }
}
