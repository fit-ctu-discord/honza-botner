using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using HonzaBotner.Discord;
using HonzaBotner.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonzaBotner.Pages
{
    [Authorize]
    public class AuthCallbackModel : PageModel
    {
        private readonly DiscordWrapper _client;
        private readonly IUserMapInfoService _userMapInfoService;

        [BindProperty(SupportsGet = true)]
        public ulong Gid { get; set; }

        [BindProperty(SupportsGet = true)]
        public ulong Uid { get; set; }

        public AuthCallbackModel(DiscordWrapper discordWrapper, IUserMapInfoService userMapInfoService)
        {
            _client = discordWrapper;
            _userMapInfoService = userMapInfoService;
        }

        public async Task OnGetAsync()
        {
            // TODO: Do this in service.
            var guild = await _client.GetGuildAsync(Gid);
            if (guild == null)
            {
                BadRequest();
            }

            var user = await guild.GetMemberAsync(Uid);
            if (user == null)
            {
                BadRequest();
            }

            string? username = User?.Identity?.Name;

            if (username == null)
            {
                Unauthorized();
            }

            UsermapPerson? person = await _userMapInfoService.GetUserInfoAsync(username);
            if (person == null)
            {
                BadRequest();
            }



            await dm.SendMessageAsync("Authenticated");
        }
    }
}
