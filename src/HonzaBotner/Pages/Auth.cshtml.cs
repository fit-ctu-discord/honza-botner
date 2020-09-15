using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonzaBotner.Pages
{
    [Authorize]
    public class AuthCallbackModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public ulong Gid { get; set; }

        [BindProperty(SupportsGet = true)]
        public ulong Uid { get; set; }
        private DiscordClient Client { get; }

        public AuthCallbackModel(Discord.DiscordWrapper discordWrapper)
        {
            Client = discordWrapper.Client;
        }

        public async Task OnGetAsync()
        {
            // TODO: Do this in service.
            var guild = await Client.GetGuildAsync(Gid);
            var user = await guild.GetMemberAsync(Uid);

            //user.GrantRoleAsync()

            var dm = await Client.CreateDmAsync(user);

            await dm.SendMessageAsync("Authenticated");
        }
    }
}
