using System;
using System.Threading.Tasks;

namespace HonzaBotner.Core.Contract
{
    public interface IAccesTokenProvider
    {
        Task<string?> GetTokenAsync();
    }
}
