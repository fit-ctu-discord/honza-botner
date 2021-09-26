using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HonzaBotner.Services.Contract
{
    public interface INewsConfigService
    {
        Task AddOrUpdate(NewsConfigDto configDto, bool active);

        Task<IList<NewsConfigDto>> ListActiveConfigsAsync();

        Task UpdateFetchDateAsync(int id, DateTime date);
    }
}
