using System.Collections.Generic;
using System.Threading.Tasks;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Services.Contract
{
    public interface IWarningService
    {
        Task<List<Warning>> GetAllWarningsAsync();

        Task<List<Warning>> GetWarningsAsync(ulong userId);

        Task<Warning?> GetWarningAsync(int id);

        Task<bool> DeleteWarningAsync(int id);

        Task<int?> AddWarningAsync(ulong userId, string reason, ulong issuerId);

        Task<int> GetNumberOfWarnings(ulong userId);
    }
}
