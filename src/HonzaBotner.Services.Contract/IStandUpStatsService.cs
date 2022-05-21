using System.Threading.Tasks;
using HonzaBotner.Services.Contract.Dto;
namespace HonzaBotner.Services.Contract;

public interface IStandUpStatsService
{
    /// <summary>
    /// Get StandUp stats for a user with a given userId
    /// </summary>
    /// <param name="userId">Id of the user</param>
    /// <returns>Null if user not in database, else his stats</returns>
    Task<StandUpStat?> GetStreak(ulong userId);

    /// <summary>
    /// Update database record of given user regarding StandUp stats. Should be called just once per day per user.
    /// </summary>
    /// <param name="userId">Id of the user</param>
    /// <param name="completed">Amount of completed tasks yesterday</param>
    /// <param name="total">Total amount of tasks yesterday</param>
    Task UpdateStats(ulong userId, int completed, int total);
}
