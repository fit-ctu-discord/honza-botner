using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Services
{
    public class CvutAuthorizationService : IAuthorizationService
    {
        private readonly HonzaBotnerDbContext _dbContext;
        private readonly CvutConfig _cvutConfig;
        private readonly IUsermapInfoService _usermapInfoService;
        private readonly IDiscordRoleManager _roleManager;

        public CvutAuthorizationService(HonzaBotnerDbContext dbContext, IOptions<CvutConfig> cvutConfig,
            IUsermapInfoService usermapInfoService, IDiscordRoleManager roleManager)
        {
            _dbContext = dbContext;
            _cvutConfig = cvutConfig.Value;
            _usermapInfoService = usermapInfoService;
            _roleManager = roleManager;
        }

        public async Task<string?> GetAuthorizationCodeAsync(ulong guildId, ulong userId)
        {
            Verification? verification = await _dbContext.Verifications
                .FirstOrDefaultAsync(v => v.GuildId == guildId && v.UserId == userId);

            if (verification != null)
            {
                return verification.Verified ? null : verification.VerificationId.ToString();
            }

            verification = new Verification
            {
                VerificationId = Guid.NewGuid(), Verified = false, GuildId = guildId, UserId = userId
            };

            await _dbContext.Verifications.AddAsync(verification);
            await _dbContext.SaveChangesAsync();

            return verification.VerificationId.ToString();
        }

        public async Task<bool> AuthorizeAsync(string code)
        {
            if (!Guid.TryParse(code, out Guid verificationId))
            {
                return false;
            }

            Verification? verification = await _dbContext.Verifications.FindAsync(verificationId);
            if (verification == null || verification.Verified)
            {
                return verification?.Verified ?? false;
            }

            UsermapPerson? person = await _usermapInfoService.GetUserInfoAsync();
            if (person == null ||
                await _dbContext.Verifications.AnyAsync(v => v.CvutUsername == person.Username))
            {
                return false;
            }

            IEnumerable<DiscordRole> discordRoles = _roleManager.MapUsermapRoles(person.Roles!.ToArray());
            bool rolesGranted =
                await _roleManager.GrantRolesAsync(verification.GuildId, verification.UserId, discordRoles);

            if (rolesGranted)
            {
                verification.Verified = true;
                verification.CvutUsername = person.Username;
                await _dbContext.SaveChangesAsync();
            }

            return rolesGranted;
        }

        public Task<string> GetAuthLink(string redirectUri)
        {
            const string authLink =
                "https://auth.fit.cvut.cz/oauth/authorize?response_type=code&client_id={0}&redirect_uri={1}";

            if (string.IsNullOrEmpty(_cvutConfig.ClientId))
            {
                throw new ArgumentNullException();
            }

            return Task.FromResult(string.Format(authLink, _cvutConfig.ClientId, redirectUri));
        }

        public async Task<bool> VerificationExistsAsync(string code)
        {
            if (!Guid.TryParse(code, out Guid verificationId))
            {
                return false;
            }

            Verification? verification = await _dbContext.Verifications.FindAsync(verificationId);
            if (verification == null)
            {
                return false;
            }

            return !verification.Verified;
        }
    }
}
