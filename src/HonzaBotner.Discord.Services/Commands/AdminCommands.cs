using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("admin")]
    [Description("Administrativní příkazy")]
    [Hidden]
    [RequirePermissions(Permissions.Administrator)]
    public class AdminCommands : BaseCommandModule
    {
        [Command("sudo"), Description("Executes a command as another user.")]
        public async Task Sudo(CommandContext ctx, [Description("Member to execute as.")] DiscordMember member,
            [RemainingText, Description("Command text to execute.")]
            string command)
        {
            await ctx.TriggerTypingAsync();

            var cmds = ctx.CommandsNext;

            // retrieve the command and its arguments from the given string
            var cmd = cmds.FindCommand(command, out var customArgs);

            // create a fake CommandContext
            var fakeContext = cmds.CreateFakeContext(member, ctx.Channel, command, ctx.Prefix, cmd, customArgs);

            // and perform the sudo
            await cmds.ExecuteCommandAsync(fakeContext);
        }

        [Command("edit")]
        public async Task SendMessage(CommandContext ctx, string url, [RemainingText] string newText)
        {
            DiscordMessage? oldMessage = await DiscordHelper.FindMessageFromLink(ctx.Message.Channel.Guild, url);

            if (oldMessage == null)
            {
                // TODO
                return;
            }

            await oldMessage.ModifyAsync(newText);
        }

        [Command("role")]
        public async Task AddRole(CommandContext ctx, DiscordRole role, DiscordRole newRole)
        {
            var __ = Task.Run(async () =>
            {
                foreach ((_, DiscordMember member) in ctx.Guild.Members)
                {
                    if (member.Roles.Contains(role) && !member.Roles.Contains(newRole))
                    {
                        await member.GrantRoleAsync(newRole);
                    }
                }
            });
        }

        [Command("countRole")]
        public async Task CountRole(CommandContext ctx, DiscordRole role)
        {
            int count = 0;

            foreach ((_, DiscordMember member) in ctx.Guild.Members)
            {
                if (member.Roles.Contains(role))
                {
                    count++;
                }
            }

            await ctx.Channel.SendMessageAsync(count.ToString());
        }
    }
}
