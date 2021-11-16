using System.Threading;
using System.Threading.Tasks;

namespace HonzaBotner.Scheduler.Contract
{
    public interface IJob
    {
        string Name { get; }

        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
