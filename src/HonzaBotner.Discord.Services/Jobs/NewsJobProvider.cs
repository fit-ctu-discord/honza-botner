﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Jobs
{
    public class NewsJobProvider : IRecurringJobProvider
    {
        public const string Key = "news-publisher";

        private readonly ILogger<NewsJobProvider> _logger;
        private readonly INewsConfigService _configService;
        private readonly IServiceProvider _serviceProvider;

        public static string CronExpression => "*/10 * * * *"; // Every ten minute

        private static readonly ConcurrentDictionary<string, Type> _typesCache = new();

        public NewsJobProvider(ILogger<NewsJobProvider> logger, INewsConfigService configService ,IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configService = configService;
            _serviceProvider = serviceProvider;
        }

        public async Task Run()
        {
            _logger.LogInformation("Starting news fetching");

            IList<NewsConfig> sources = await _configService.ListConfigsAsync();

            using IServiceScope scope = _serviceProvider.CreateScope();

            foreach (NewsConfig newsSource in sources)
            {
                INewsService newsService = scope.ServiceProvider.GetRequiredService(GetType(NewsConfig.TypeFromProvider(newsSource.NewsProviderType))) as INewsService
                    ?? throw new InvalidCastException("Type must be INewsService");
                IPublisherService publisherService = scope.ServiceProvider.GetRequiredService(GetType(NewsConfig.TypeFromPublisher(newsSource.PublisherType))) as IPublisherService
                    ?? throw new InvalidCastException("Type must be IPublisherService");

                DateTime now = DateTime.Now;
                IAsyncEnumerable<News> news = newsService.FetchDataAsync(newsSource.Source, newsSource.LastFetched);

                await foreach (News item in news)
                {
                    await publisherService.Publish(item, newsSource.Channels);
                }

                await _configService.UpdateFetchDateAsync(newsSource.Id, now);
            }
        }

        private static Type GetType(string typeName)
        {
            if (_typesCache.TryGetValue(typeName, out Type? type))
            {
                return type;
            }

            type = Type.GetType(typeName) ?? throw new ArgumentOutOfRangeException(nameof(typeName), $"Invalid type: {typeName}");

            return _typesCache[typeName] = type;
        }
    }
}
