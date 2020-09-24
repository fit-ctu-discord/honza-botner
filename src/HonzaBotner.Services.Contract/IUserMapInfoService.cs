using System.Threading.Tasks;

namespace HonzaBotner.Services.Contract
{
    public interface IUserMapInfoService
    {
        Task<UsermapPerson?> GetUserInfoAsync(string username);
    }
}
