using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Services.Contract;

public interface INewsConfigService
{
    Task AddOrUpdate(NewsConfig configDto);

    Task<IList<NewsConfig>> ListConfigsAsync(bool onlyActive = true);

    Task UpdateFetchDateAsync(int id, DateTime date);

    Task<bool> ToggleConfig(int id);

    Task<NewsConfig> GetById(int id);
}
