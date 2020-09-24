using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using HonzaBotner.Discord;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonzaBotner.Pages
{
    [Authorize]
    public class AuthCallbackModel : PageModel
    {
        private readonly IUsermapInfoService _userMapInfoService;
        private readonly IDiscordRoleManager _roleManager;
        private readonly DiscordClient _client;

        [BindProperty(SupportsGet = true)]
        public ulong Gid { get; set; }

        [BindProperty(SupportsGet = true)]
        public ulong Uid { get; set; }

        public AuthCallbackModel(DiscordWrapper discordWrapper, IUsermapInfoService userMapInfoService, IDiscordRoleManager roleManager)
        {
            _client = discordWrapper.Client;
            _userMapInfoService = userMapInfoService;
            _roleManager = roleManager;
        }

        public async Task OnGetAsync()
        {
            string? username = User?.Identity?.Name;

            if (username == null)
            {
                Unauthorized();
                return;
            }

            UsermapPerson? person = await _userMapInfoService.GetUserInfoAsync(username);
            if (person == null)
            {
                BadRequest();
                return;
            }

            IEnumerable<DiscordRole> discordRoles = _roleManager.MapUsermapRoles(person.Roles!.ToArray());
            bool result = await _roleManager.GrantRolesAsync(Gid, Uid, discordRoles);

            if (result)
            {
                // TODO: process result
            }
        }
    }
}
