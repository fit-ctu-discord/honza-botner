using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using HonzaBotner.Discord;
using HonzaBotner.Discord.Extensions;
using HonzaBotner.Discord.Services.Options;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Controllers
{
    [ApiController]
    public class ErrorController : BaseController
    {
        private readonly IGuildProvider _guildProvider;
        private readonly IOptions<DiscordConfig> _discordOptions;

        public ErrorController(
            IGuildProvider guildProvider,
            IOptions<DiscordConfig> options,
            IOptions<InfoOptions> infoOptions
        ) : base(infoOptions)
        {
            _guildProvider = guildProvider;
            _discordOptions = options;
        }

        [Route("/error")]
        public async Task<IActionResult> Index()
        {
            IExceptionHandlerFeature? context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();

            ulong logChannelId = _discordOptions.Value.LogChannelId;

            if (logChannelId == default)
            {
                return Page("Something went wrong. Please contact @mod at server.", 500);
            }

            DiscordChannel channel = guild.GetChannel(logChannelId);

            await channel.ReportException("ASP Core .NET", context?.Error ?? new ArgumentException());

            return Page("Something went wrong. They were notified, but still please contact @Mod at server.", 500);
        }
    }
}
