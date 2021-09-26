using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Services.Contract
{
    public interface INewsConfigService
    {
        Task AddOrUpdate(NewsConfig configDto, bool active);

        Task<IList<NewsConfig>> ListActiveConfigsAsync();

        Task UpdateFetchDateAsync(int id, DateTime date);
    }
}
