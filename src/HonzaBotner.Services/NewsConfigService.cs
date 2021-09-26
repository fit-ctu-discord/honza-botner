using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;
using Dto = HonzaBotner.Services.Contract.Dto;
using Microsoft.EntityFrameworkCore;

namespace HonzaBotner.Services
{
    public class NewsConfigService : INewsConfigService
    {
        private readonly HonzaBotnerDbContext _context;

        private DbSet<NewsConfig> Configs => _context.NewsConfigs;

        public NewsConfigService(HonzaBotnerDbContext context)
        {
            _context = context;
        }

        public async Task AddOrUpdate(Dto.NewsConfig configDto, bool active)
        {
            Database.NewsConfig config = await Configs.FirstOrDefaultAsync(c => c.Id == configDto.Id);

            if (config is null)
            {
                config = new Database.NewsConfig
                {
                    Id = configDto.Id,
                    Name = configDto.Name,
                    Source = configDto.Source,
                    Active = active,
                    Channels = configDto.Channels,
                    LastFetched = configDto.LastFetched,
                    NewsProviderType = configDto.NewsProviderType,
                    PublisherType = configDto.PublisherType
                };

                Configs.Add(config);
            }
            else
            {
                config.Name = configDto.Name;
                config.Source = configDto.Source;
                config.Active = active;
                config.Channels = configDto.Channels;
                config.NewsProviderType = configDto.NewsProviderType;
                config.PublisherType = configDto.PublisherType;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IList<Dto.NewsConfig>> ListActiveConfigsAsync()
        {
            return await Configs
                .Where(c => c.Active)
                .Select(c => new Dto.NewsConfig(c.Id, c.Name, c.Source, c.LastFetched, c.NewsProviderType, c.PublisherType, c.Channels))
                .ToListAsync();
        }

        public async Task UpdateFetchDateAsync(int id, DateTime date)
        {
            Database.NewsConfig config = await Configs.FirstOrDefaultAsync(c => c.Id == id);

            if (config is null)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Invalid id, must be known");
            }

            config.LastFetched = date;

            await _context.SaveChangesAsync();
        }
    }
}
