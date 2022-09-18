using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Services.Contract;

public interface INewsConfigService
{
    Task AddOrUpdate(NewsConfig configDto);

    Task<IList<NewsConfig>> ListConfigsAsync(bool onlyActive = true);

    Task UpdateFetchDateAsync(long id, DateTime date);

    Task<bool> ToggleConfig(long id);

    Task<NewsConfig> GetById(long id);
}
