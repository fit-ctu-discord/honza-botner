using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Services.Contract;

public interface ICanteenService
{
    Task<IList<CanteenDto>> ListCanteensAsync(bool onlyOpen = false, CancellationToken cancellationToken = default);
    Task<CanteenDto> GetCurrentMenuAsync(CanteenDto canteen, CancellationToken cancellationToken = default);
}
