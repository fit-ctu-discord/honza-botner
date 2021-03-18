using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warning = HonzaBotner.Services.Contract.Dto.Warning;

namespace HonzaBotner.Services
{
    public class WarningService : IWarningService
    {
        private readonly HonzaBotnerDbContext _dbContext;
        private readonly ILogger<WarningService> _logger;

        public WarningService(HonzaBotnerDbContext dbContext, ILogger<WarningService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<Warning>> GetAllWarningsAsync()
        {
            return (await _dbContext.Warnings.OrderByDescending(w => w.IssuedAt).ToListAsync()).Select(GetDto).ToList();
        }

        public async Task<List<Warning>> GetWarningsAsync(ulong userId)
        {
            return (await _dbContext.Warnings.Where(w => w.UserId == userId)
                .OrderByDescending(w => w.IssuedAt).ToListAsync()).Select(GetDto).ToList();
        }

        public async Task<Warning?> GetWarningAsync(int id)
        {
            return GetDto(await _dbContext.Warnings.FirstOrDefaultAsync(w => w.Id == id));
        }

        public async Task<bool> DeleteWarningAsync(int id)
        {
            Database.Warning? warning = await _dbContext.Warnings.FirstOrDefaultAsync(w => w.Id == id);

            if (warning == null)
            {
                return false;
            }

            _dbContext.Warnings.Remove(warning);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Couldn't remove warning {@Warning}", warning);
                return false;
            }

            return true;
        }

        public async Task<int?> AddWarningAsync(ulong userId, string reason, ulong issuerId)
        {
            Database.Warning warning = new() {UserId = userId, Reason = reason, IssuerId = issuerId};

            await _dbContext.Warnings.AddAsync(warning);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Couldn't add warning {@Warning}", warning);
                return null;
            }

            return warning.Id;
        }

        public async Task<int> GetNumberOfWarnings(ulong userId)
        {
            return await _dbContext.Warnings.Where(w => w.UserId == userId).CountAsync();
        }

        private static Warning GetDto(Database.Warning warning) =>
            new(warning.Id, warning.UserId, warning.Reason, warning.IssuedAt, warning.IssuerId);
    }
}
