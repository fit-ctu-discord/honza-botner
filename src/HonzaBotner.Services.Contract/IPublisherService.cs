using System.Threading.Tasks;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Services.Contract
{
    public interface IPublisherService
    {
        Task Publish(News news, params ulong[] channels);
    }
}
