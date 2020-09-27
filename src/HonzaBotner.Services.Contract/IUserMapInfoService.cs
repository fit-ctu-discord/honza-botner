using System.Threading.Tasks;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Services.Contract
{
    public interface IUsermapInfoService
    {
        Task<UsermapPerson?> GetUserInfoAsync();
    }
}
