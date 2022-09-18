using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;
using Dto = HonzaBotner.Services.Contract.Dto;
using Microsoft.EntityFrameworkCore;

namespace HonzaBotner.Services;

public class NewsConfigService : INewsConfigService
{
    private readonly HonzaBotnerDbContext _context;

    private DbSet<NewsConfig> Configs => _context.NewsConfigs;

    public NewsConfigService(HonzaBotnerDbContext context)
    {
        _context = context;
    }

    private async Task<NewsConfig> GetConfig(long id)
    {
        NewsConfig? config = await Configs.FirstOrDefaultAsync(c => c.Id == id);

        if (config is null)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "Config id was not found.");
        }

        return config;
    }

    public async Task AddOrUpdate(Dto.NewsConfig configDto)
    {
        NewsConfig? config = await Configs.FirstOrDefaultAsync(c => c.Id == configDto.Id);

        if (config is null)
        {
            config = new NewsConfig
            {
                Id = configDto.Id,
                Name = configDto.Name,
                Source = configDto.Source,
                Active = configDto.Active,
                Channels = configDto.Channels,
                LastFetched = configDto.LastFetched.SetKindUtc(),
                NewsProviderType = configDto.NewsProvider.ToString(),
                PublisherType = configDto.Publisher.ToString()
            };

            Configs.Add(config);
        }
        else
        {
            config.Name = configDto.Name;
            config.Source = configDto.Source;
            config.Active = configDto.Active;
            config.Channels = configDto.Channels;
            config.LastFetched = configDto.LastFetched.SetKindUtc();
            config.NewsProviderType = configDto.NewsProvider.ToString();
            config.PublisherType = configDto.Publisher.ToString();
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IList<Dto.NewsConfig>> ListConfigsAsync(bool onlyActive = true)
    {
        return await Configs
            .Where(c => c.Active || !onlyActive)
            .Select(c => DomainToDto(c))
            .ToListAsync();
    }

    private static Dto.NewsConfig DomainToDto(NewsConfig config)
    {
        Dto.NewsProviderType newsProviderType =
            NewsConfigHelper.StringToEnum<Dto.NewsProviderType>(config.NewsProviderType);

        Dto.PublisherType publisherType =
            NewsConfigHelper.StringToEnum<Dto.PublisherType>(config.PublisherType);

        return new(config.Id, config.Name, config.Source, config.LastFetched, newsProviderType,
            publisherType, config.Active, config.Channels);
    }

    public async Task UpdateFetchDateAsync(long id, DateTime date)
    {
        NewsConfig config = await GetConfig(id);

        config.LastFetched = date.SetKindUtc();

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ToggleConfig(long id)
    {
        NewsConfig config = await GetConfig(id);

        config.Active = !config.Active;

        await _context.SaveChangesAsync();

        return config.Active;
    }

    public async Task<Dto.NewsConfig> GetById(long id)
    {
        NewsConfig config = await GetConfig(id);

        return DomainToDto(config);
    }
}
