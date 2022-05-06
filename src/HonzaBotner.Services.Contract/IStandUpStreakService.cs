using System;
using System.Threading.Tasks;
using HonzaBotner.Services.Contract.Dto;
namespace HonzaBotner.Services.Contract;

public interface IStandUpStreakService
{
    Task<StandUpStreak?> GetStreak(ulong userId);

    Task UpdateStreak(ulong userId);

    Task<bool> IsValidStreak(ulong userId);
}
